using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Delete;

/// <summary>
/// Represents a command to delete an entity identified by its unique identifier.
/// </summary>
/// <remarks>This command is typically used in CQRS patterns to request the deletion of an entity. The entity to
/// be deleted is identified by the <see cref="Id"/> property.</remarks>
/// <param name="Id">The unique identifier of the entity to be deleted. Must not be <see langword="default"/>.</param>
public sealed record DeleteCommand(Guid Id) : CommandBase<DeleteCommand, Unit>;
