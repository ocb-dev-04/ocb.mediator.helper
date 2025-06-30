using OCB.Mediator.Helper.Abstractions.Notification;

namespace OCB.Mediator.Helper.Abstractions.Pipelines;

/// <summary>
/// Defines a behavior in the notification processing pipeline.
/// </summary>
/// <remarks>Implementations of this interface can be used to add custom logic or modify the behavior of
/// notification handling, such as logging, validation, or other cross-cutting concerns.</remarks>
/// <typeparam name="TNotification">The type of notification being processed. Must implement <see cref="INotification"/>.</typeparam>
public interface INotificationPipelineBehavior<TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification and invokes the next delegate in the processing pipeline.
    /// </summary>
    /// <remarks>This method is typically used in a notification handling pipeline to process a notification
    /// and optionally pass control to the next handler. Implementations should ensure proper handling of the <paramref
    /// name="cancellationToken"/> to support cancellation scenarios.</remarks>
    /// <param name="notification">The notification to be handled. This parameter cannot be null.</param>
    /// <param name="next">A delegate representing the next action in the pipeline. This parameter cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TNotification notification, Func<Task> next, CancellationToken cancellationToken);
}
