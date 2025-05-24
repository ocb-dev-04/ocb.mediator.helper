using Shared.Common.Helper.ErrorsHandler;

namespace CQRS.MediatR.Helper.Abstractions.Messaging;

/// <summary>
/// A <see cref="ICommandHandler{TCommand}"/> to use when endpoint doesn't return value
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<in TCommand> 
        where TCommand : ICommand
{
    /// <summary>
    /// Handle the <see cref="TCommand"/>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Handle(TCommand request, CancellationToken cancellationToken);
}

/// <summary>
/// A <see cref="ICommandHandler{TCommand, TResponse}"/> to use when endpoint return an object of type <typeparamref name="TResponse"/> 
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<in TCommand, TResponse> 
        where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handle the <see cref="TCommand"/> and returns an object of type <typeparamref name="TResponse"/>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
}