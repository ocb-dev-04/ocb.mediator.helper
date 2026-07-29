using OCB.Mediator.Helper.Abstractions.Messaging;

namespace OCB.Mediator.Helper.Abstractions.Idempotency;

/// <summary>
/// Marker base type for commands that should be executed idempotently.
/// </summary>
/// <remarks>This type does not implement idempotency by itself: pair it with an idempotency pipeline behavior
/// (an <see cref="Pipelines.IRequestPipelineBehavior{TRequest, TResponse}"/> constrained to this type) that checks a
/// store for a previously processed request id before invoking the handler. Useful in scenarios where duplicate
/// requests may occur, such as distributed systems or retry mechanisms.</remarks>
/// <typeparam name="TSelf">The concrete command type inheriting from this record.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
public abstract record IdempotentCommand<TSelf, TResponse>
    : CommandBase<TSelf, TResponse>
        where TSelf : IdempotentCommand<TSelf, TResponse>;
