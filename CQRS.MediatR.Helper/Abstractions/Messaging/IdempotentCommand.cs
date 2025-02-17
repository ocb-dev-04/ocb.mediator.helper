namespace CQRS.MediatR.Helper.Abstractions.Messaging;

/// <summary>
/// An <see cref="IdempotentCommand"/> to use when endpoint doesn't return value
/// </summary>
public interface IdempotentCommand 
    : ICommand;

/// <summary>
/// An <see cref="IdempotentCommand{TResponse}"/> to use when endpoint return value
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IdempotentCommand<TResponse> 
    : ICommand<TResponse>;