using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Create;

public sealed record CreateCommand(string Name, string Description) : ICommand<Guid>;
