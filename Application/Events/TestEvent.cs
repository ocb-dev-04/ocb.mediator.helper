using OCB.Mediator.Helper.Abstractions.Notification;

namespace Application.Events;

/// <summary>
/// Represents an event with a name and description that can be used for notifications.
/// </summary>
/// <remarks>This type is immutable and is intended to be used as a lightweight data structure for passing  event
/// information within a notification system.</remarks>
/// <param name="Name">The name of the event. This value cannot be null or empty.</param>
/// <param name="Description">A brief description of the event. This value cannot be null or empty.</param>
internal sealed record TestEvent(string Name, string Description) : INotification;