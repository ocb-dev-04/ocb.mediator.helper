namespace OCB.Mediator.Helper.Abstractions.Notification;

/// <summary>
/// Defines a contract for handling notifications asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for processing notifications of type <typeparamref
/// name="TNotification"/>. This is typically used in scenarios where notifications or events need to be handled in an
/// asynchronous manner.</remarks>
/// <typeparam name="TNotification">The type of notification to be handled. Must implement the <see cref="INotification"/> interface.</typeparam>
public interface INotificationHandler<in TNotification> 
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <remarks>This method processes the provided notification and performs the necessary actions
    /// asynchronously. Ensure that the <paramref name="notification"/> parameter is not <see langword="null"/> before
    /// calling this method.</remarks>
    /// <param name="notification">The notification to be processed. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}