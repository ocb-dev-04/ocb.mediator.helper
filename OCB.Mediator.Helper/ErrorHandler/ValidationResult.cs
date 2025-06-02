using OCB.Mediator.Helper.Abstractions.Validations;

namespace OCB.Mediator.Helper.ErrorHandler;

/// <summary>
/// Base class to use ValidationResult design pattern
/// </summary>
public class ValidationResult : ValidationResults.ValidationResult, IValidationResult
{
    /// <summary>
    /// Protected <see cref="Results.ValidationResult"/> constructor
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    private ValidationResult(ValidationError[] errors)
        : base(IValidationResult.ValidationErrors)
        => Errors = errors;

    public ValidationError[] Errors {get;}

    public static ValidationResult WithErrors(ValidationError[] errors)
        => new(errors);
}