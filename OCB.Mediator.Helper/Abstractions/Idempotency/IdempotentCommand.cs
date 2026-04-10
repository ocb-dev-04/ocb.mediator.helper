using OCB.Mediator.Helper.Abstractions.Messaging;

namespace OCB.Mediator.Helper.Abstractions.Idempotency;

/// <summary>
/// Represents a command that ensures idempotent behavior, guaranteeing that repeated executions with the same <see
/// </summary>
/// <remarks>Idempotent commands are useful in scenarios where duplicate requests may occur, such as distributed
/// systems or retry mechanisms.
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
public abstract record IdempotentCommand<TResponse>
    : ICommand<TResponse>;

