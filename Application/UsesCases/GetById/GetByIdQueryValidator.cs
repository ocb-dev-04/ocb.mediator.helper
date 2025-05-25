using FluentValidation;

namespace Application.UsesCases.GetById;

internal sealed class GetByIdQueryValidator
    : AbstractValidator<GetByIdQuery>
{
    public GetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("Id cannot be empty.");
    }
}