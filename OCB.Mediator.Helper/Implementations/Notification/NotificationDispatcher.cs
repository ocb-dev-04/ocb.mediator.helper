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

namespace OCB.Mediator.Helper.Implementations.Notification;

/// <summary>
/// Provides functionality to dispatch notifications to their respective handlers, with optional retry and pipeline
/// behaviors.
/// </summary>
/// <remarks>The <see cref="NotificationDispatcher"/> is responsible for resolving notification handlers and
/// invoking them asynchronously. It is registered as a singleton so its resilience policies (circuit breaker,
/// bulkhead) accumulate state across the whole application; each dispatch creates its own DI scope to resolve
/// handlers. The resilience policy is applied per handler, so a retry never re-executes handlers that already
/// succeeded. Handlers are resolved generically — no reflection.</remarks>
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

        // ⭐ 1. Timeout Policy (optimistic: handlers must honor the CancellationToken they receive)
        AsyncTimeoutPolicy timeoutPolicy = Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(30),
                timeoutStrategy: TimeoutStrategy.Optimistic,
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
        bool useRetry,
        bool usePipeline,
        CancellationToken cancellationToken)
            where TNotification : INotification
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        using IServiceScope scope = _serviceProvider.CreateScope();

        Func<Task> handlerInvocation = () => DispatchToHandlers(scope.ServiceProvider, notification, useRetry, cancellationToken);

        if (usePipeline)
        {
            foreach (INotificationPipelineBehavior<TNotification> behavior in scope.ServiceProvider
                .GetServices<INotificationPipelineBehavior<TNotification>>()
                .Reverse())
            {
                Func<Task> next = handlerInvocation;
                handlerInvocation = () => behavior.HandleAsync(notification, next, cancellationToken);
            }
        }

        await handlerInvocation();

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("--> DispatchAsync<{NotificationType}> took {ElapsedMilliseconds} ms",
                typeof(TNotification).Name, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
    }

    /// <summary>
    /// Resolves all handlers for the notification generically and runs them concurrently.
    /// </summary>
    /// <remarks>When <paramref name="useRetry"/> is <see langword="true"/>, the resilience policy wraps each
    /// handler individually so a retry only re-executes the handler that failed, never those that already
    /// succeeded. Handlers are resolved by the static type of <typeparamref name="TNotification"/>, so notifications
    /// must be dispatched with their concrete type.</remarks>
    private async Task DispatchToHandlers<TNotification>(
        IServiceProvider serviceProvider,
        TNotification notification,
        bool useRetry,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        List<Task> tasks = new();
        foreach (INotificationHandler<TNotification> handler in serviceProvider
            .GetServices<INotificationHandler<TNotification>>())
        {
            tasks.Add(useRetry
                ? _resiliencePolicy.ExecuteAsync(token => handler.HandleAsync(notification, token), cancellationToken)
                : handler.HandleAsync(notification, cancellationToken));
        }

        // Execute all handler tasks concurrently
        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }
}
