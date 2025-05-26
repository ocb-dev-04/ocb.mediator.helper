namespace OCB.Mediator.Helper.Abstractions.Notification;

/// <summary>
/// Contract to use for notification handlers.
/// </summary>
/// <typeparam name="TNotification"></typeparam>
public interface INotificationHandler<in TNotification> 
    where TNotification : INotification
{
    /// <summary>
    /// Method to handle a notification.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}