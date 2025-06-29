using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.Abstractions.Validations;

/// <summary>
/// Represents the result of a validation operation, including any validation errors encountered.
/// </summary>
/// <remarks>This interface provides access to validation errors that occurred during a validation process.
/// Implementations of this interface should define how validation errors are collected and exposed.</remarks>
public interface IValidationResult
{
    /// <summary>
    /// Represents a predefined validation error indicating that a validation problem occurred.
    /// </summary>
    /// <remarks>This static readonly field provides a reusable instance of the <see cref="ValidationError"/>
    /// class with a predefined error code and message. It can be used to represent general validation issues in
    /// applications.</remarks>
    public static readonly ValidationError ValidationErrors = new(
        "ValidationError",
        "A validation problem ocurred");

    /// <summary>
    /// Gets the collection of validation errors associated with the current operation or object.
    /// </summary>
    ValidationError[] Errors { get; }
}