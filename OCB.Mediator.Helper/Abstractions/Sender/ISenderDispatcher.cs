using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;

namespace OCB.Mediator.Helper.Abstractions.Sender;

/// <summary>
/// Defines the strongly-typed dispatch contract used by requests to route themselves to their handler.
/// </summary>
/// <remarks>This interface is the second half of the double-dispatch mechanism: <see cref="ISender"/> receives a
/// request through its interface type (<see cref="IQuery{TResponse}"/> or <see cref="ICommand{TResponse}"/>) and the
/// request calls back into this dispatcher with its own concrete type as <c>TQuery</c>/<c>TCommand</c>. This keeps the
/// whole dispatch path fully generic — no reflection, no boxing — and compatible with AOT/trimming. Consumers normally
/// never call these methods directly; they are invoked by <see cref="Messaging.QueryBase{TSelf, TResponse}"/> and
/// <see cref="Messaging.CommandBase{TSelf, TResponse}"/>.</remarks>
public interface ISenderDispatcher
{
    /// <summary>
    /// Dispatches a query to its <see cref="IQueryHandler{TQuery, TResponse}"/> through the request pipeline.
    /// </summary>
    /// <typeparam name="TQuery">The concrete query type.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by the query.</typeparam>
    /// <param name="query">The query instance to dispatch. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/>
    /// representing the outcome of the query.</returns>
    Task<Result<TResponse>> DispatchQuery<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken)
        where TQuery : class, IQuery<TResponse>;

    /// <summary>
    /// Dispatches a command to its <see cref="ICommandHandler{TCommand, TResponse}"/> through the request pipeline.
    /// </summary>
    /// <typeparam name="TCommand">The concrete command type.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by the command.</typeparam>
    /// <param name="command">The command instance to dispatch. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/>
    /// representing the outcome of the command.</returns>
    Task<Result<TResponse>> DispatchCommand<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
        where TCommand : class, ICommand<TResponse>;
}
