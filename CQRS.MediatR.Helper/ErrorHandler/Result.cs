namespace CQRS.MediatR.Helper.ErrorHandler;

/// <summary>
/// Base class to use Result design pattern
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ValidationError Errors { get; }

    /// <summary>
    /// Protected <see cref="Result"/> constructor
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    protected Result(bool isSuccess, ValidationError error)
    {
        if (isSuccess && !error.Equals(ValidationError.None))
            throw new InvalidOperationException();

        if (!isSuccess && error.Equals(ValidationError.None))
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Errors = error;
    }

    /// <summary>
    /// Return a <see cref="Result"/> as success
    /// </summary>
    /// <returns></returns>
    public static Result Success() => new(true, ValidationError.None);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with specific error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result Failure(ValidationError error)
        => new(false, error);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with none error
    /// </summary>
    /// <returns></returns>
    public static Result Failure() => new(false, ValidationError.None);

    /// <summary>
    /// Return a <see cref="Result"/> as success with response <see cref="{TValue}"/> value 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<TValue> Success<TValue>(TValue value)
        => new(value, true, ValidationError.None);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with <see cref="{Error}"/> value
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static Result<TValue> Failure<TValue>(ValidationError error)
        => new(default, false, error);

    /// <summary>
    /// Return a <see cref="Result"/> as failure with none error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static Result<TValue> Failure<TValue>()
        => new (default, false, ValidationError.None);

    /// <summary>
    /// Create a <see cref="Result{TValue}"/>, uf <typeparamref name="TValue"/> is not null then return <see cref="{TValue}"/> if it's null otherwise return <see cref="Error.NullValue"/>
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(ValidationError.NullValue);
}
