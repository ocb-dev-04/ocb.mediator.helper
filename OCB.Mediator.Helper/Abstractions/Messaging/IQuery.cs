using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Sender;

namespace OCB.Mediator.Helper.Abstractions.Messaging;

/// <summary>
/// An <see cref="IQuery{TResponse}"/> to use for queries endpoints
/// </summary>
/// <remarks>Queries should inherit from <see cref="QueryBase{TSelf, TResponse}"/>, which implements
/// <see cref="Accept"/> and provides the reflection-free double dispatch used by the sender.</remarks>
public interface IQuery<TResponse>
{
    /// <summary>
    /// Routes this query to the dispatcher using its concrete type (double dispatch).
    /// </summary>
    /// <remarks>Implemented by <see cref="QueryBase{TSelf, TResponse}"/>; not intended to be called directly
    /// by application code — use <see cref="ISender.Send{TResponse}(IQuery{TResponse}, CancellationToken)"/>.</remarks>
    /// <param name="dispatcher">The dispatcher that resolves and executes the handler pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task<Result<TResponse>> Accept(ISenderDispatcher dispatcher, CancellationToken cancellationToken);
}

/// <summary>
/// Base type for queries. Recovers the concrete query type at compile time (curiously recurring template pattern)
/// so dispatch is fully generic — no reflection, no boxing.
/// </summary>
/// <typeparam name="TSelf">The concrete query type inheriting from this record.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the query.</typeparam>
/// <example>
/// <code>
/// public sealed record GetItemQuery(Guid Id) : QueryBase&lt;GetItemQuery, ItemResponse&gt;;
/// </code>
/// </example>
public abstract record QueryBase<TSelf, TResponse> : IQuery<TResponse>
    where TSelf : QueryBase<TSelf, TResponse>
{
    Task<Result<TResponse>> IQuery<TResponse>.Accept(ISenderDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchQuery<TSelf, TResponse>((TSelf)this, cancellationToken);
}
