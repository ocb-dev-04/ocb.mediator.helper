using FluentValidation;

namespace Application.UsesCases.Delete;

internal sealed class DeleteCommandValidator 
    : AbstractValidator<DeleteCommand>
{
    public DeleteCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Id must not be empty.")
            .NotNull()
            .WithMessage("Id must not be null.")
            .NotEqual(Guid.Empty)
            .WithMessage("Id must not be the default value of Guid.");
    }
}