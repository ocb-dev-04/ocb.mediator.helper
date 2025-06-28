using Application.Events;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;
using OCB.Mediator.Helper.Abstractions.Notification;

namespace Application.UsesCases.Create;

internal sealed class CreateCommandHandler : ICommandHandler<CreateCommand, Guid>
{
    private readonly INotificationDispatcher _notificationDispacher;
    public CreateCommandHandler(INotificationDispatcher notificationDispacher)
    {
        _notificationDispacher = notificationDispacher;
    }

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