using Polly;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OCB.Mediator.Helper.Abstractions.Pipelines;
using OCB.Mediator.Helper.Abstractions.Notification;

namespace OCB.Mediator.Helper.Implementations.Notification;

/// <summary>
/// Provides functionality to dispatch notifications to their respective handlers,  with optional retry and pipeline
/// behaviors.
/// </summary>
/// <remarks>The <see cref="NotificationDispatcher"/> is responsible for resolving notification handlers  and
/// invoking them asynchronously. It supports retry policies for transient failures and  allows the use of pipeline
/// behaviors to modify or extend the dispatch process.</remarks>
internal sealed class NotificationDispatcher
    : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider; 
    private readonly ILogger<NotificationDispatcher> _logger; 
    private readonly AsyncPolicy _retryPolicy;

    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, INotification, CancellationToken, Task[]>> _cachedDispatchers = new();

    /// <summary>
    /// <see cref="NotificationDispatcher"/> public constructor 
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required for notification dispatching. Cannot be <see
    /// langword="null"/>.</param>
    /// <param name="logger">The logger used to log diagnostic and retry information during notification dispatching. Cannot be <see
    /// langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/> or <paramref name="logger"/> is <see langword="null"/>.</exception>
    public NotificationDispatcher(
        IServiceProvider serviceProvider, 
        ILogger<NotificationDispatcher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(250 * attempt),
                onRetry: (exception, delay, retryCount, _) 
                    => logger.LogWarning(exception, "--> [Polly Retry] next in {Delay}ms", delay.TotalMilliseconds));
    }

    /// <inheritdoc/>
    public Task DispatchAsync<TNotification>(
        TNotification notification,
        bool useRetry = true,
        CancellationToken cancellationToken = default)
            where TNotification : INotification
        => HandleDispatchAsync(notification, useRetry, usePipeline: true, cancellationToken);

    /// <inheritdoc/>
    public Task UnhandledDispatchAsync<TNotification>(
        TNotification notification,
        bool useRetry = true,
        CancellationToken cancellationToken = default) 
            where TNotification : INotification
        => HandleDispatchAsync(notification, useRetry, usePipeline: false, cancellationToken);

    /// <summary>
    /// Handles the dispatch of a notification to its registered handlers, optionally applying retry policies and
    /// pipeline behaviors.
    /// </summary>
    /// <remarks>This method creates a new service scope for resolving dependencies and dispatching the
    /// notification.  If <paramref name="usePipeline"/> is <see langword="true"/>, the notification will be processed
    /// through  all registered pipeline behaviors in reverse order. If <paramref name="useRetry"/> is <see
    /// langword="true"/>,  retry policies will be applied to the dispatch operation.</remarks>
    /// <typeparam name="TNotification">The type of the notification being dispatched. Must implement <see cref="INotification"/>.</typeparam>
    /// <param name="notification">The notification instance to be dispatched to handlers.</param>
    /// <param name="useRetry">A value indicating whether retry policies should be applied during the dispatch process.  <see langword="true"/>
    /// to apply retry policies; otherwise, <see langword="false"/>.</param>
    /// <param name="usePipeline">A value indicating whether pipeline behaviors should be applied during the dispatch process.  <see
    /// langword="true"/> to apply pipeline behaviors; otherwise, <see langword="false"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <returns></returns>
    private async Task HandleDispatchAsync<TNotification>(
        TNotification notification,
        bool useRetry = true,
        bool usePipeline = true,
        CancellationToken cancellationToken = default)
            where TNotification : INotification
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        using IServiceScope scope = _serviceProvider.CreateScope();

        Func<Task> handlerInvocation = async () =>
        {
            Func<IServiceProvider, INotification, CancellationToken, Task[]> dispatcher = _cachedDispatchers.GetOrAdd(
                typeof(TNotification),
                CreateDispatcher(typeof(TNotification)));

            Task[] tasks = dispatcher(scope.ServiceProvider, notification, cancellationToken);
            await Task.WhenAll(tasks);
        };

        if(usePipeline)
        {
            INotificationPipelineBehavior<TNotification>[] behaviors = scope.ServiceProvider
                .GetServices<INotificationPipelineBehavior<TNotification>>()
                .Reverse()
                .ToArray();

            foreach (INotificationPipelineBehavior<TNotification> behavior in behaviors)
            {
                Func<Task> next = handlerInvocation;
                handlerInvocation = () => behavior.HandleAsync(notification, next, cancellationToken);
            }
        }

        if (useRetry)
            await _retryPolicy.ExecuteAsync(handlerInvocation);
        else
            await handlerInvocation();

        stopwatch.Stop();
        _logger.LogInformation("--> DispatchAsync<{NotificationType}> took {ElapsedMilliseconds} ms",
            typeof(TNotification).Name, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Creates a dispatcher function that resolves and invokes notification handlers for a given notification type.
    /// </summary>
    /// <remarks>The returned dispatcher function resolves all registered handlers for the specified
    /// notification type from the  <see cref="IServiceProvider"/> and invokes their <c>HandleAsync</c> method. If no
    /// handlers are registered, the function  returns an empty array of tasks.  Handlers are invoked in reverse order
    /// of their registration in the service provider.</remarks>
    /// <param name="notificationType">The type of the notification for which handlers will be resolved and invoked.</param>
    /// <returns>A function that takes an <see cref="IServiceProvider"/>, a notification instance, and a <see
    /// cref="CancellationToken"/>,  and returns an array of <see cref="Task"/> objects representing the asynchronous
    /// operations performed by the handlers.</returns>
    private static Func<IServiceProvider, INotification, CancellationToken, Task[]> CreateDispatcher(Type notificationType)
    {
        Type handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        return (serviceProvider, notification, cancellationToken) =>
        {
            Type notificationType = notification.GetType();
            Type handlerInterface = typeof(INotificationHandler<>).MakeGenericType(notificationType);
            Task[] tasks = Enumerable.Empty<Task>().ToArray();

            object?[] handlers = serviceProvider.GetServices(handlerInterface).Reverse().ToArray();
            if (!handlers.Any()) return tasks;

            tasks = handlers
                .Select(handler =>
                {
                    MethodInfo? method = handlerInterface.GetMethod("HandleAsync");
                    if (method is null)
                        return Task.CompletedTask;

                    return (Task)method.Invoke(handler, new object[] { notification, cancellationToken })!;
                })
                .ToArray();

            return tasks;
        };
    }
}
