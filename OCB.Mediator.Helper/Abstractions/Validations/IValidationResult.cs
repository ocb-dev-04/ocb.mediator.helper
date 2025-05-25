using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.Abstractions.Validations;

public interface IValidationResult
{
    public static readonly ValidationError ValidationErrors = new(
        "ValidationError",
        "A validation problem ocurred");

    ValidationError[] Errors { get; }
}