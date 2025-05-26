using OCB.Mediator.Helper.Abstractions.Notification;

namespace Application.Events;

internal sealed record TestEvent(string Name, string Description) : INotification;