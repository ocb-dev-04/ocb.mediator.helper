using OCB.Mediator.Helper.Abstractions.Notification;

namespace OCB.Mediator.Helper.Abstractions.Pipelines;

/// <summary>
/// Defines a behavior in the notification pipeline that processes a notification and optionally performs actions before
/// or after the next behavior in the pipeline.
/// </summary>
/// <typeparam name="TNotification"></typeparam>
public interface INotificationPipelineBehavior<TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the notification in the pipeline.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    Task HandleAsync(
        TNotification notification,
        CancellationToken cancellationToken,
        Func<Task> next);
}
