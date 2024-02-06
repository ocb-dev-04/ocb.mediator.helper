using Shared.Common.Helper.ErrorsHandler;

namespace MediatR.Cqrs.Helper.Abstractions.Messaging;

/// <summary>
/// A <see cref="ICommandHandler{TCommand}"/> to use when endpoint doesn't return value
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<TCommand> 
    : IRequestHandler<TCommand, Result>
        where TCommand : ICommand
{
}

/// <summary>
/// A <see cref="ICommandHandler{TCommand, TResponse}"/> to use when endpoint return an object of type <typeparamref name="TResponse"/> 
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<TCommand, TResponse> 
    : IRequestHandler<TCommand, Result<TResponse>>
        where TCommand : ICommand<TResponse>
{
}