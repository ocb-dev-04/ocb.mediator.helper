using FluentValidation;

namespace Application.UsesCases.GetById;

/// <summary>
/// Provides validation rules for the <see cref="GetByIdQuery"/> type.
/// </summary>
/// <remarks>This validator ensures that the <c>Id</c> property of a <see cref="GetByIdQuery"/> instance is not
/// empty and does not equal <see cref="Guid.Empty"/>.</remarks>
internal sealed class GetByIdQueryValidator
    : AbstractValidator<GetByIdQuery>
{
    /// <summary>
    /// <see cref="GetByIdQueryValidator"/> public constructor
    /// </summary>
    /// <remarks>This validator enforces that the <c>Id</c> property is not empty and is not equal to
    /// <c>Guid.Empty</c>. Use this validator to ensure that queries include a valid identifier before
    /// processing.</remarks>
    public GetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.")
            .NotEqual(Guid.Empty)
            .WithMessage("Id cannot be empty.");
    }
}