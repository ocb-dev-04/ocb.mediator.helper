using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.ValidationResults;

/// <summary>
/// Represents the result of a validation operation, including the validated value, success status, and any associated
/// error.
/// </summary>
/// <remarks>This class extends <see cref="ValidationResult"/> to include a strongly-typed value. It is typically
/// used to encapsulate the outcome of a validation process, providing both the validated value and information about
/// whether the validation succeeded or failed.</remarks>
/// <typeparam name="TValue">The type of the value being validated.</typeparam>
public class ValidationResult<TValue> : ValidationResult
{
    /// <summary>
    /// <see cref="ValidationResult{TValue}"/> internal constructor
    /// </summary>
    /// <remarks>This class encapsulates a validation error and provides a base for handling validation
    /// results. It is intended for internal use and is not exposed directly to external consumers.</remarks>
    /// <param name="error">The validation error associated with the result. Cannot be null.</param>
    internal ValidationResult(ValidationError error)
        : base(error)
    {
    }
}