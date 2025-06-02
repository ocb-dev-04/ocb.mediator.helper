using System.Diagnostics.CodeAnalysis;

namespace OCB.Mediator.Helper.ResultPattern;


/// <summary>
/// Base class to use Result design pattern
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    /// <summary>
    /// Protected <see cref="Result"/> constructor
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && !error.Equals(Error.None))
            throw new InvalidOperationException();

        if (!isSuccess && error.Equals(Error.None))
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Return a <see cref="Result"/> as success
    /// </summary>
    /// <returns></returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with specific error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with none error
    /// </summary>
    /// <returns></returns>
    public static Result Failure() => new(false, Error.None);

    /// <summary>
    /// Return a <see cref="Result"/> as success with response <see cref="{TValue}"/> value 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with <see cref="{Error}"/> value
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with none error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static Result<TValue> Failure<TValue>() => new(default, false, Error.None);

    /// <summary>
    /// Create a <see cref="Result{TValue}"/>, uf <typeparamref name="TValue"/> is not null then return <see cref="{TValue}"/> if it's null otherwise return <see cref="Error.NullValue"/>
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

/// <summary>
/// <see cref="Result"/> expansion
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Protected <see cref="Result{TValue}"/> constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}