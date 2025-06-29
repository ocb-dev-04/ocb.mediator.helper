using OCB.Mediator.Helper.ResultPattern;

namespace OCB.Mediator.Helper.Abstractions.Messaging;

/// <summary>
/// Defines a handler for processing queries of type <typeparamref name="TQuery"/> and returning a result of type
/// <typeparamref name="TResponse"/>.
/// </summary>
/// <remarks>Implementations of this interface are responsible for executing the logic required to process the
/// query and produce a response. This interface is typically used in CQRS (Command Query Responsibility Segregation)
/// patterns to decouple query processing from other application logic.</remarks>
/// <typeparam name="TQuery">The type of the query to be handled. Must implement <see cref="IQuery{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the query handler.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> 
        where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the specified query and returns the result asynchronously.
    /// </summary>
    /// <param name="request">The query object containing the data required for processing. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/>
    /// object representing the outcome of the query processing.</returns>
    Task<Result<TResponse>> Handle(TQuery request, CancellationToken cancellationToken);
}