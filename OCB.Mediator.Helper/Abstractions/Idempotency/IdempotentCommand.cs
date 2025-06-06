﻿using OCB.Mediator.Helper.Abstractions.Messaging;

namespace OCB.Mediator.Helper.Abstractions.Idempotency;

/// <summary>
/// An <see cref="IdempotentCommand"/> to use when endpoint doesn't return value
/// </summary>
public abstract record IdempotentCommand(Guid RequestId)
    : ICommand;

/// <summary>
/// An <see cref="IdempotentCommand{TResponse}"/> to use when endpoint return value
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public abstract record IdempotentCommand<TResponse>(Guid RequestId)
    : ICommand<TResponse>;