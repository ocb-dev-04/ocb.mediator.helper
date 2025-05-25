using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Delete;

public sealed record DeleteCommand(Guid Id) : ICommand;
