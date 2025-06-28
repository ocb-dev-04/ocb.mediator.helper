namespace OCB.Mediator.Helper.Abstractions.Notification;

/// <summary>
/// Contract to use for notification dispatchers.
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// Dispatches a notification to all registered handlers asynchronously.
    /// </summary>
    /// <typeparam name="TNotification"></typeparam>
    /// <param name="notification"></param>
    /// <param name="useRetry">Set is need retry just in case of exception</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
