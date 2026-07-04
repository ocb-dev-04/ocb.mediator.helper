using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Sender;

namespace OCB.Mediator.Helper.Abstractions.Messaging;

/// <summary>
/// An <see cref="ICommand{TReponse}"/> to use when endpoint return value
/// </summary>
/// <remarks>Commands should inherit from <see cref="CommandBase{TSelf, TResponse}"/>, which implements
/// <see cref="Accept"/> and provides the reflection-free double dispatch used by the sender.</remarks>
public interface ICommand<TResponse> : IBaseCommand
{
    /// <summary>
    /// Routes this command to the dispatcher using its concrete type (double dispatch).
    /// </summary>
    /// <remarks>Implemented by <see cref="CommandBase{TSelf, TResponse}"/>; not intended to be called directly
    /// by application code — use <see cref="ISender.Send{TResponse}(ICommand{TResponse}, CancellationToken)"/>.</remarks>
    /// <param name="dispatcher">The dispatcher that resolves and executes the handler pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task<Result<TResponse>> Accept(ISenderDispatcher dispatcher, CancellationToken cancellationToken);
}

/// <summary>
/// Base interface to define a command structure
/// </summary>
public interface IBaseCommand;

/// <summary>
/// Base type for commands. Recovers the concrete command type at compile time (curiously recurring template pattern)
/// so dispatch is fully generic — no reflection, no boxing.
/// </summary>
/// <typeparam name="TSelf">The concrete command type inheriting from this record.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the command.</typeparam>
/// <example>
/// <code>
/// public sealed record CreateItemCommand(string Name) : CommandBase&lt;CreateItemCommand, Guid&gt;;
/// </code>
/// </example>
public abstract record CommandBase<TSelf, TResponse> : ICommand<TResponse>
    where TSelf : CommandBase<TSelf, TResponse>
{
    Task<Result<TResponse>> ICommand<TResponse>.Accept(ISenderDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchCommand<TSelf, TResponse>((TSelf)this, cancellationToken);
}
