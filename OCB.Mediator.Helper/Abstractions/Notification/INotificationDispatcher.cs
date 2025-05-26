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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DispatchAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
