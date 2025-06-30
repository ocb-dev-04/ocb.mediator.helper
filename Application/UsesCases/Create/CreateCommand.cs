using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Create;

/// <summary>
/// Represents a command to create an entity with a specified name and description.
/// </summary>
/// <remarks>This command is typically used to initiate the creation of a new entity in a system. The <see
/// cref="Name"/> and <see cref="Description"/> properties provide the necessary details for the entity being
/// created.</remarks>
/// <param name="Name"></param>
/// <param name="Description"></param>
public sealed record CreateCommand(string Name, string Description) : ICommand<Guid>;
