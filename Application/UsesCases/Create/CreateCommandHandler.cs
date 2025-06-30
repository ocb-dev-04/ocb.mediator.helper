using Application.Events;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;
using OCB.Mediator.Helper.Abstractions.Notification;

namespace Application.UsesCases.Create;

/// <summary>
/// Handles the execution of a <see cref="CreateCommand"/> and returns a unique identifier for the created entity.
/// </summary>
/// <remarks>This handler processes the <see cref="CreateCommand"/> by generating a new <see cref="Guid"/> and
/// dispatching a notification event through the provided <see cref="INotificationDispatcher"/>. The notification
/// pipeline is executed to handle the event.</remarks>
internal sealed class CreateCommandHandler 
    : ICommandHandler<CreateCommand, Guid>
{
    private readonly INotificationDispatcher _notificationDispacher;

    /// <summary>
    /// <see cref="CreateCommandHandler"/> public constructor
    /// </summary>
    /// <param name="notificationDispacher">The dispatcher used to send notifications. This parameter cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="notificationDispacher"/> is <see langword="null"/>.</exception>
    public CreateCommandHandler(INotificationDispatcher notificationDispacher)
        => _notificationDispacher = notificationDispacher ?? throw new ArgumentNullException(nameof(notificationDispacher));

    /// <summary>
    /// Handles the creation of a new entity and dispatches a notification event.
    /// </summary>
    /// <remarks>This method generates a new unique identifier for the entity, creates a notification event
    /// based on the provided details,  and dispatches the event through the notification pipeline. The notification
    /// pipeline execution behavior can be configured  to include or exclude unhandled notifications.</remarks>
    /// <param name="request">The command containing the details for the entity to be created, including its name and description.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{Guid}"/> containing the unique identifier of the newly created entity.</returns>
    public async Task<Result<Guid>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        Guid id = Guid.NewGuid();

        TestEvent testEvent = new (request.Name, request.Description);
        
        // all Notification Pipeline will be executed
        await _notificationDispacher.DispatchAsync(testEvent, true, cancellationToken);
        
        // ignore all Notification Pipelines
        //await _notificationDispacher.UnhandledDispatchAsync(testEvent, true, cancellationToken);

        return await Task.FromResult(id);
    }
}