using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OCB.Mediator.Helper.Abstractions.Messaging;
using OCB.Mediator.Helper.Abstractions.Notification;
using OCB.Mediator.Helper.Abstractions.Pipelines;
using OCB.Mediator.Helper.Abstractions.Sender;
using Polly;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace OCB.Mediator.Helper.Implementations.Notification;

/// <summary>
/// Provides functionality to dispatch notifications to their respective handlers,  with optional retry and pipeline
/// behaviors.
/// </summary>
/// <remarks>The <see cref="NotificationDispatcher"/> is responsible for resolving notification handlers  and
/// invoking them asynchronously. It supports retry policies for transient failures and  allows the use of pipeline
/// behaviors to modify or extend the dispatch process.</remarks>
internal sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly AsyncPolicy _retryPolicy;

    private static readonly ConcurrentDictionary<Type, NotificationDispatchInfo> _cachedDispatchInfo = new();

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

    public Task DispatchAsync<TNotification>(
        TNotification notification,
        bool useRetry = true,
        CancellationToken cancellationToken = default)
            where TNotification : INotification
        => HandleDispatchAsync(notification, useRetry, usePipeline: true, cancellationToken);

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

        Func<Task> handlerInvocation = () => DispatchToHandlersOptimized(scope.ServiceProvider, notification, cancellationToken);

        if (usePipeline)
        {
            var behaviors = scope.ServiceProvider
                .GetServices<INotificationPipelineBehavior<TNotification>>()
                .Reverse();

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

    private async Task DispatchToHandlersOptimized<TNotification>(
        IServiceProvider serviceProvider,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        Type notificationType = typeof(TNotification);

        NotificationDispatchInfo dispatchInfo = _cachedDispatchInfo.GetOrAdd(notificationType, static type =>
        {
            Type handlerInterface = typeof(INotificationHandler<>).MakeGenericType(type);
            MethodInfo? handleMethod = handlerInterface.GetMethod("HandleAsync");

            if (handleMethod == null)
                throw new InvalidOperationException($"HandleAsync method not found in {handlerInterface.FullName}");

            return new NotificationDispatchInfo(handlerInterface, handleMethod);
        });

        var handlers = serviceProvider.GetServices(dispatchInfo.HandlerInterface);
        List<Task> tasks = new();
        foreach (object? handler in handlers)
        {
            if (handler == null) continue;

            object? result = dispatchInfo.HandleMethod.Invoke(handler, new object[] { notification, cancellationToken });
            if (result is Task task)
                tasks.Add(task);
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    private readonly record struct NotificationDispatchInfo(Type HandlerInterface, MethodInfo HandleMethod);
}
