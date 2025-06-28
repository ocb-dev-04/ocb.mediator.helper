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
/// <see cref="INotificationDispatcher"/> implementation that dispatches notifications to all registered handlers.
/// </summary>
internal sealed class NotificationDispatcher
    : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider; 
    private readonly ILogger<NotificationDispatcher> _logger; 
    private readonly AsyncPolicy _retryPolicy;

    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, INotification, CancellationToken, Task[]>> _cachedDispatchers = new();

    /// <summary>
    /// <see cref="NotificationDispatcher"/> public constructor that accepts an <see cref="IServiceProvider"/> to resolve dependencies.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
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
