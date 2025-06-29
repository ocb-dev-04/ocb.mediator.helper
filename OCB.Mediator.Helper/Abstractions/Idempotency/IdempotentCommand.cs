using OCB.Mediator.Helper.Abstractions.Messaging;

namespace OCB.Mediator.Helper.Abstractions.Idempotency;

/// <summary>
/// Represents a command that ensures idempotent behavior by associating a unique request identifier.
/// </summary>
/// <remarks>Idempotent commands are designed to prevent unintended side effects when executed multiple times. The
/// <see cref="RequestId"/> property uniquely identifies the request, allowing the system to track and ensure that the
/// command is processed only once.</remarks>
/// <param name="RequestId"></param>
public abstract record IdempotentCommand(Guid RequestId)
    : ICommand;

/// <summary>
/// Represents a command that ensures idempotent behavior, guaranteeing that repeated executions with the same <see
/// cref="RequestId"/> produce the same result.
/// </summary>
/// <remarks>Idempotent commands are useful in scenarios where duplicate requests may occur, such as distributed
/// systems or retry mechanisms. The <see cref="RequestId"/> uniquely identifies the command instance, allowing the
/// system to recognize and handle duplicate requests appropriately.</remarks>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
/// <param name="RequestId"></param>
public abstract record IdempotentCommand<TResponse>(Guid RequestId)
    : ICommand<TResponse>;