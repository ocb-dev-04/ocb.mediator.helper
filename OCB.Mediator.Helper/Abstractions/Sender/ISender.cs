using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;

namespace OCB.Mediator.Helper.Abstractions.Sender;

/// <summary>
/// Defines a contract for sending queries and commands to their respective handlers for processing.
/// </summary>
/// <remarks>The <see cref="ISender"/> interface provides methods for dispatching queries and commands
/// asynchronously. Queries are used to retrieve data or perform read-only operations, while commands are used to
/// perform actions or modify state. Each method returns a <see cref="Task"/> representing the asynchronous operation,
/// with the result encapsulated in a <see cref="Result"/> or <see cref="Result{T}"/> object.</remarks>
public interface ISender
{
    /// <summary>
    /// Sends the specified query to be processed and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response expected from the query.</typeparam>
    /// <param name="query">The query to be sent for processing. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed result of type
    /// <typeparamref name="TResponse"/>.</returns>
    Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command to the appropriate handler and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response expected from the command.</typeparam>
    /// <param name="command">The command to be sent. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response of type <typeparamref
    /// name="TResponse"/>.</returns>
    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}