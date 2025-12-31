using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OCB.Mediator.Helper.Abstractions.Notification;
using OCB.Mediator.Helper.Abstractions.Pipelines;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System.Diagnostics;
using System.Reflection;

namespace OCB.Mediator.Helper.Implementations.Notification;

/// <summary>
/// Provides functionality to dispatch notifications to their respective handlers, with optional retry and pipeline
/// behaviors.
/// </summary>
/// <remarks>The <see cref="NotificationDispatcher"/> is responsible for resolving notification handlers and
/// invoking them asynchronously. It supports retry policies for transient failures and allows the use of pipeline
/// behaviors to modify or extend the dispatch process. This implementation uses direct reflection without caching
/// for minimal memory footprint.</remarks>
internal sealed class NotificationDispatcher 
    : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly AsyncPolicyWrap _resiliencePolicy;

    public NotificationDispatcher(
        IServiceProvider serviceProvider,
        ILogger<NotificationDispatcher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // ⭐ 1. Timeout Policy
        AsyncTimeoutPolicy timeoutPolicy = Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(30),
                timeoutStrategy: TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timeout, task) =>
                {
                    _logger.LogWarning(
                        "--> [Timeout] Notification handler exceeded {Timeout}s",
                        timeout.TotalSeconds);
                    return Task.CompletedTask;
                });

        // ⭐ 2. Bulkhead (limitar concurrencia)
        AsyncBulkheadPolicy bulkheadPolicy = Policy
            .BulkheadAsync(
                maxParallelization: 100,
                maxQueuingActions: 50,
                onBulkheadRejectedAsync: context =>
                {
                    _logger.LogWarning("--> [Bulkhead] Notification rejected - Too many concurrent dispatches");
                    return Task.CompletedTask;
                });

        // ⭐ 3. Circuit Breaker
        AsyncCircuitBreakerPolicy circuitBreakerPolicy = Policy
            .Handle<Exception>(ex => !(ex is TimeoutRejectedException || ex is BulkheadRejectedException))
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError(exception,
                        "--> [Circuit Breaker] OPEN for {Duration}s", duration.TotalSeconds);
                },
                onReset: () => _logger.LogInformation("--> [Circuit Breaker] CLOSED"),
                onHalfOpen: () => _logger.LogInformation("--> [Circuit Breaker] HALF-OPEN"));

        // ⭐ 4. Retry Policy
        AsyncRetryPolicy retryPolicy = Policy
            .Handle<Exception>(ex =>
                !(ex is BrokenCircuitException ||
                  ex is TimeoutRejectedException ||
                  ex is BulkheadRejectedException))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1)),
                onRetry: (exception, delay, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "--> [Retry] Attempt {RetryCount}/3 in {Delay}ms",
                        retryCount, delay.TotalMilliseconds);
                });

        // ⭐ Timeout → Bulkhead → Circuit Breaker → Retry
        _resiliencePolicy = Policy.WrapAsync(
            timeoutPolicy,
            bulkheadPolicy,
            circuitBreakerPolicy,
            retryPolicy
        );
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

        Func<Task> handlerInvocation = () => DispatchToHandlersDirect(scope.ServiceProvider, notification, cancellationToken);

        if (usePipeline)
        {
            IEnumerable<INotificationPipelineBehavior<TNotification>> behaviors = scope.ServiceProvider
                .GetServices<INotificationPipelineBehavior<TNotification>>()
                .Reverse();

            foreach (INotificationPipelineBehavior<TNotification> behavior in behaviors)
            {
                Func<Task> next = handlerInvocation;
                handlerInvocation = () => behavior.HandleAsync(notification, next, cancellationToken);
            }
        }

        if (useRetry)
            await _resiliencePolicy.ExecuteAsync(handlerInvocation);
        else
            await handlerInvocation();

        stopwatch.Stop();
        _logger.LogInformation("--> DispatchAsync<{NotificationType}> took {ElapsedMilliseconds} ms",
            notification.GetType().Name, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Dispatches notification to handlers using direct reflection without caching.
    /// </summary>
    /// <remarks>This method resolves handler interface and HandleAsync method via reflection on each call,
    /// trading performance for reduced memory usage and avoiding potential cache-related memory leaks.</remarks>
    private async Task DispatchToHandlersDirect<TNotification>(
        IServiceProvider serviceProvider,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        Type notificationType = notification.GetType();

        // Resolve handler interface through reflection (no caching)
        Type handlerInterface = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        // Get HandleAsync method via reflection
        MethodInfo handleMethod = handlerInterface.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException($"HandleAsync method not found in {handlerInterface.FullName}");

        // Resolve all handlers for this notification type
        var handlers = serviceProvider.GetServices(handlerInterface);

        // Create and execute tasks for each handler
        List<Task> tasks = new();
        foreach (object? handler in handlers)
        {
            if (handler == null) continue;

            object? result = handleMethod.Invoke(handler, new object[] { notification, cancellationToken });
            if (result is Task task)
                tasks.Add(task);
        }

        // Execute all handler tasks concurrently
        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }
}