using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.Exceptions;

/// <summary>
/// Represents an exception that occurs when validation of input data fails.
/// </summary>
/// <remarks>This exception is typically thrown to indicate that one or more validation errors have occurred. The
/// <see cref="Errors"/> property provides detailed information about the specific validation errors.</remarks>
public sealed class ValidationException 
    : Exception
{
    /// <summary>
    /// <see cref="ValidationException"/> public constructor
    /// </summary>
    /// <param name="errors"></param>
    public ValidationException(IEnumerable<ValidationError> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets a collection of validation errors associated with the current operation or object.
    /// </summary>
    public IEnumerable<ValidationError> Errors { get; }
}