using CQRS.MediatR.Helper.ErrorHandler;

namespace CQRS.MediatR.Helper.Abstractions.Validations;

public interface IValidationResult
{
    public static readonly ValidationError ValidationErrors = new(
        "ValidationError",
        "A validation problem ocurred");

    ValidationError[] Errors { get; }
}