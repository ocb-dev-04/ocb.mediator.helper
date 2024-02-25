using CQRS.MediatR.Helper.Models;

namespace CQRS.MediatR.Helper.Exceptions;

/// <summary>
/// An <see cref="Exception"/> to throw when some validation  fails in a CQRS request pipeline
/// </summary>
public sealed class ValidationException 
    : Exception
{
    public ValidationException(IEnumerable<ValidationError> errors)
    {
        Errors = errors;
    }

    public IEnumerable<ValidationError> Errors { get; }
}