using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.Results;

/// <summary>
/// Base class to use Result design pattern
/// </summary>
public class Result
{
    internal ValidationError Error { get; }

    /// <summary>
    /// Protected <see cref="Result"/> constructor
    /// </summary>
    /// <param name="error"></param>
    internal Result(ValidationError error)
        => Error = error;

    /// <summary>
    /// Return a <see cref="Result"/> as success with response <see cref="{TValue}"/> value 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static Result<TValue> Success<TValue>(TValue value)
        => new(ValidationError.None);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with <see cref="{Error}"/> value
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    internal static Result<TValue> Failure<TValue>(ValidationError error)
        => new(error);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with none error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    internal static Result<TValue> Failure<TValue>()
        => new (ValidationError.None);

    /// <summary>
    /// Create a <see cref="Result{TValue}"/>, uf <typeparamref name="TValue"/> is not null then return <see cref="{TValue}"/> if it's null otherwise return <see cref="Error.NullValue"/>
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    internal static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(ValidationError.NullValue);
}
