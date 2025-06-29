namespace OCB.Mediator.Helper.Abstractions.Notification;

/// <summary>
/// Defines a contract for dispatching notifications to registered handlers asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for invoking all registered handlers for a given
/// notification type. Notifications are dispatched asynchronously, and the execution order of handlers is not
/// guaranteed.</remarks>
public interface INotificationDispatcher
{
    /// <summary>
    /// Dispatches the specified notification to all registered handlers asynchronously.
    /// </summary>
    /// <remarks>This method ensures that all registered handlers for the specified notification type are
    /// invoked. If <paramref name="useRetry"/> is <see langword="true"/>, failed handler invocations may be retried
    /// based on the configured retry policy.</remarks>
    /// <typeparam name="TNotification">The type of the notification to dispatch. Must implement <see cref="INotification"/>.</typeparam>
    /// <param name="notification">The notification instance to be dispatched. Cannot be <see langword="null"/>.</param>
    /// <param name="useRetry">A value indicating whether retry logic should be applied if a handler fails. <see langword="true"/> to enable
    /// retry logic; otherwise, <see langword="false"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when all handlers have processed the
    /// notification.</returns>
    Task DispatchAsync<TNotification>(TNotification notification, bool useRetry = true, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Dispatches a notification to all registered handlers that can process the specified notification type.
    /// It is good to use with the exception/error events because it avoids the exception loop.
    /// </summary>
    /// <remarks>This method ensures that all handlers registered for the specified notification type are
    /// invoked. Handlers are executed asynchronously, and their execution order is not guaranteed.</remarks>
    /// <typeparam name="TNotification">The type of the notification to dispatch. Must implement <see cref="INotification"/>.</typeparam>
    /// <param name="notification">The notification instance to be dispatched. Cannot be <see langword="null"/>.</param>
    /// <param name="useRetry">Set is need retry just in case of exception</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when all handlers have processed the
    /// notification.</returns>
    Task UnhandledDispatchAsync<TNotification>(TNotification notification, bool useRetry = true, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
