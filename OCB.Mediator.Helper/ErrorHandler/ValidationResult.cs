using OCB.Mediator.Helper.Abstractions.Validations;

namespace OCB.Mediator.Helper.ErrorHandler;

/// <summary>
/// Represents the result of a validation operation, including any validation errors encountered.
/// </summary>
/// <remarks>This class provides a way to encapsulate the outcome of a validation process. It includes a
/// collection  of validation errors that describe the issues found during validation. Use the <see cref="WithErrors"> 
/// method to create an instance of <see cref="ValidationResult"> with specific errors.</remarks>
public class ValidationResult : ValidationResults.ValidationResult, IValidationResult
{
    /// <summary>
    /// Represents the result of a validation operation, including any validation errors encountered.
    /// </summary>
    /// <remarks>This class encapsulates validation errors and provides access to them through the <see
    /// cref="Errors"/> property. Instances of this class are typically created internally during validation
    /// processes.</remarks>
    /// <param name="errors">An array of <see cref="ValidationError"/> objects representing the validation errors.</param>
    private ValidationResult(ValidationError[] errors)
        : base(IValidationResult.ValidationErrors)
        => Errors = errors;

    /// <summary>
    /// Gets the collection of validation errors encountered during the operation.
    /// </summary>
    public ValidationError[] Errors {get;}

    /// <summary>
    /// Creates a new <see cref="ValidationResult"/> instance containing the specified validation errors.
    /// </summary>
    /// <param name="errors">An array of <see cref="ValidationError"/> objects representing the validation errors to include in the result.
    /// Cannot be null.</param>
    /// <returns>A <see cref="ValidationResult"/> object initialized with the provided validation errors.</returns>
    public static ValidationResult WithErrors(ValidationError[] errors)
        => new(errors);
}