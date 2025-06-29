using OCB.Mediator.Helper.Abstractions.Validations;

namespace OCB.Mediator.Helper.ErrorHandler;

/// <summary>
/// Represents the result of a validation operation, including any validation errors.
/// </summary>
/// <remarks>This class encapsulates the outcome of a validation process, providing access to any validation
/// errors that occurred. Use the <see cref="WithErrors(ValidationError[])"/> method to create an instance with specific
/// errors.</remarks>
/// <typeparam name="TValue">The type of the value being validated.</typeparam>
public sealed class ValidationResult<TValue> : ValidationResults.ValidationResult<TValue>, IValidationResult
{
    /// <summary>
    /// Represents the result of a validation operation, including any validation errors encountered.
    /// </summary>
    /// <param name="errors">An array of <see cref="ValidationError"/> objects representing the validation errors. Cannot be null.</param>
    public ValidationResult(ValidationError[] errors)
        : base(IValidationResult.ValidationErrors)
        => Errors = errors;

    /// <summary>
    /// Gets the collection of validation errors encountered during the operation.
    /// </summary>
    public ValidationError[] Errors {get;}

    /// <summary>
    /// Creates a new <see cref="ValidationResult{TValue}"/> instance containing the specified validation errors.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static ValidationResult<TValue> WithErrors(ValidationError[] errors)
        => new(errors);
}