using OCB.Mediator.Helper.ResultPattern;

namespace OCB.Mediator.Helper.Abstractions.Messaging;

/// <summary>
/// Defines a contract for handling commands of type <typeparamref name="TCommand"/> and producing a response of type
/// <typeparamref name="TResponse"/>.
/// </summary>
/// <remarks>Implementations of this interface are responsible for processing commands and returning a result
/// encapsulating the response. This interface is typically used in command-based architectures to decouple command
/// handling logic from other parts of the application.</remarks>
/// <typeparam name="TCommand">The type of the command to be handled. Must implement <see cref="ICommand{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by handling the command.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> 
        where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles the specified command and returns the result of the operation.
    /// </summary>
    /// <param name="request">The command to be handled. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/>
    /// indicating the outcome of the operation, including the response data if successful.</returns>
    Task<Result<TResponse>> Handle(TCommand request, CancellationToken cancellationToken);
}