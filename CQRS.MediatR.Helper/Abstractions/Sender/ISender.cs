using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace CQRS.MediatR.Helper.Abstractions.Sender;

/// <summary>
/// The ISender interface defines methods for sending commands and queries.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends a query and returns a response.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> Send(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command and returns a response.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}