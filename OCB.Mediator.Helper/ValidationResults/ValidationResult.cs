using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.ValidationResults;

/// <summary>
/// Represents the result of a validation operation, indicating success or failure.
/// </summary>
/// <remarks>A <see cref="ValidationResult"/> encapsulates the outcome of a validation process, including any
/// associated error information. Use this class to represent validation results in scenarios where success or failure
/// needs to be communicated.</remarks>
public class ValidationResult
{
    /// <summary>
    /// Gets the validation error associated with the current operation.
    /// </summary>
    internal ValidationError Error { get; }

    /// <summary>
    /// <see cref="ValidationResult"/> internal constructor
    /// </summary>
    /// <param name="error">The validation error associated with the result. Cannot be <see langword="null"/>.</param>
    internal ValidationResult(ValidationError error)
        => Error = error;

    /// <summary>
    /// Creates a successful validation result containing the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated.</typeparam>
    /// <param name="value">The value associated with the successful validation result.</param>
    /// <returns>A <see cref="ValidationResult{TValue}"/> representing a successful validation, with no errors.</returns>
    internal static ValidationResult<TValue> Success<TValue>(TValue value)
        => new(ValidationError.None);

    /// <summary>
    /// Creates a failed validation result with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value associated with the validation result.</typeparam>
    /// <param name="error">The validation error that describes the failure. Cannot be null.</param>
    /// <returns>A <see cref="ValidationResult{TValue}"/> representing a failed validation result.</returns>
    internal static ValidationResult<TValue> Failure<TValue>(ValidationError error)
        => new(error);

    /// <summary>
    /// Creates a validation result that represents a failure with no associated validation error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated.</typeparam>
    /// <returns>A <see cref="ValidationResult{TValue}"/> instance indicating a failure with no validation error.</returns>
    internal static ValidationResult<TValue> Failure<TValue>()
        => new (ValidationError.None);

    /// <summary>
    /// Creates a <see cref="ValidationResult{TValue}"/> instance based on the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to validate.</typeparam>
    /// <param name="value">The value to validate. Can be null.</param>
    /// <returns>A <see cref="ValidationResult{TValue}"/> representing the validation outcome.  Returns a successful result if
    /// <paramref name="value"/> is not null; otherwise,  returns a failure result with a <see
    /// cref="ValidationError.NullValue"/> error.</returns>
    internal static ValidationResult<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(ValidationError.NullValue);
}
