using System.Diagnostics.CodeAnalysis;

namespace OCB.Mediator.Helper.ResultPattern;

/// <summary>
/// Represents the outcome of an operation, encapsulating success or failure states.
/// </summary>
/// <remarks>The <see cref="Result"/> class is designed to provide a standardized way to represent the result of
/// an operation. It includes information about whether the operation succeeded or failed, and optionally an associated
/// error. Use <see cref="Success"/> or <see cref="Failure"/> methods to create instances of <see
/// cref="Result"/>.</remarks>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation resulted in a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the current operation, if any.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class, representing the outcome of an operation.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the operation was successful.  <see langword="true"/> if the operation succeeded;
    /// otherwise, <see langword="false"/>.</param>
    /// <param name="error">The error associated with the operation. Must be <see cref="Error.None"/> if <paramref name="isSuccess"/> is
    /// <see langword="true"/>,  and must not be <see cref="Error.None"/> if <paramref name="isSuccess"/> is <see
    /// langword="false"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="isSuccess"/> is <see langword="true"/> and <paramref name="error"/> is not <see
    /// cref="Error.None"/>,  or if <paramref name="isSuccess"/> is <see langword="false"/> and <paramref name="error"/>
    /// is <see cref="Error.None"/>.</exception>
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
    /// Creates a successful <see cref="Result"/> instance.
    /// </summary>
    /// <returns>A <see cref="Result"/> object representing a successful operation, with its  <see cref="Result.IsSuccess"/>
    /// property set to <see langword="true"/> and  <see cref="Result.Error"/> set to <see cref="Error.None"/>.</returns>
    public static Result Success() 
        => new(true, Error.None);
    
    /// <summary>
    /// Creates a failed <see cref="Result"/> instance with the specified error.
    /// </summary>
    /// <param name="error">The error describing the reason for the failure. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="Result"/> instance representing a failure, containing the provided error.</returns>
    public static Result Failure(Error error) 
        => new(false, error);

    /// <summary>
    /// Creates a <see cref="Result"/> instance representing a failed operation.
    /// </summary>
    /// <returns>A <see cref="Result"/> object with a failure state and no associated error.</returns>
    public static Result Failure() 
        => new(false, Error.None);

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
    /// <param name="value">The value to include in the successful result. Cannot be null.</param>
    /// <returns>A <see cref="Result{TValue}"/> instance representing a successful operation, containing the specified value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) 
        => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value associated with the result.</typeparam>
    /// <param name="error">The error describing the failure. Cannot be null.</param>
    /// <returns>A result object representing a failure, with no value and the specified error.</returns>
    public static Result<TValue> Failure<TValue>(Error error) 
        => new(default, false, error);

    /// <summary>
    /// Creates a failed result with no value and no error information.
    /// </summary>
    /// <typeparam name="TValue">The type of the value that the result would contain if successful.</typeparam>
    /// <returns>A <see cref="Result{TValue}"/> instance representing a failure, with a default value and no associated error.</returns>
    public static Result<TValue> Failure<TValue>() 
        => new(default, false, Error.None);

    /// <summary>
    /// Creates a <see cref="Result{TValue}"/> instance based on the provided value.
    /// </summary>
    /// <remarks>Use this method to create a result object that encapsulates either a successful value or an
    /// error indicating that the value was null. This is useful for scenarios where null values need to be explicitly
    /// handled as errors.</remarks>
    /// <typeparam name="TValue">The type of the value to be encapsulated in the result.</typeparam>
    /// <param name="value">The value to be evaluated and encapsulated. Can be null.</param>
    /// <returns>A <see cref="Result{TValue}"/> representing success if <paramref name="value"/> is not null; otherwise, a
    /// failure result containing an error indicating a null value.</returns>
    public static Result<TValue> Create<TValue>(TValue? value) 
        => value is not null 
            ? Success(value) 
            : Failure<TValue>(Error.NullValue);
}

/// <summary>
/// Represents the result of an operation, encapsulating a value of type <typeparamref name="TValue"/>  along with
/// success or failure state and an associated error, if applicable.
/// </summary>
/// <remarks>This class is a generic extension of the <see cref="Result"/> base type, providing additional
/// functionality  for operations that return a value. It includes mechanisms to access the value when the operation
/// succeeds  and ensures proper handling of failure cases.</remarks>
/// <typeparam name="TValue">The type of the value encapsulated by the result.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// <see cref="Result{TValue}"/> public constructor
    /// </summary>
    /// <param name="value">The value associated with the result. Can be <see langword="null"/> if the result does not contain a value.</param>
    /// <param name="isSuccess">A value indicating whether the operation was successful. <see langword="true"/> if successful; otherwise, <see
    /// langword="false"/>.</param>
    /// <param name="error">The error information associated with the result. Must be provided if <paramref name="isSuccess"/> is <see
    /// langword="false"/>.</param>
    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
        => _value = value;

    /// <summary>
    /// Gets the value associated with a successful result.
    /// </summary>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="TValue"/> to a <see cref="Result{TValue}"/> instance.
    /// </summary>
    /// <param name="value">The value to be converted. Can be <see langword="null"/>.</param>
    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}