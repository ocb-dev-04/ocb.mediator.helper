namespace OCB.Mediator.Helper.Abstractions.Messaging;

/// <summary>
/// An <see cref="ICommand"/> to use when endpoint return value
/// </summary>
public interface ICommand<TReponse> : IBaseCommand;

/// <summary>
/// Base interface to define a command structure
/// </summary>
public interface IBaseCommand;