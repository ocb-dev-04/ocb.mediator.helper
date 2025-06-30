using FluentValidation;

namespace Application.UsesCases.Create;

/// <summary>
/// Provides validation rules for the <see cref="CreateCommand"/> type.
/// </summary>
/// <remarks>This validator ensures that the <see cref="CreateCommand.Name"/> and <see
/// cref="CreateCommand.Description"/>  properties meet specific requirements, such as being non-empty and adhering to
/// maximum length constraints.</remarks>
internal sealed class CreateCommandValidator
    : AbstractValidator<CreateCommand>
{
    /// <summary>
    /// <see cref="CreateCommandValidator"/> public constructor
    /// </summary>
    /// <remarks>This validator enforces rules for the <c>Name</c> and <c>Description</c> properties of a
    /// command. The <c>Name</c> property must be non-empty and have a maximum length of 50 characters. The
    /// <c>Description</c> property must be non-empty and have a maximum length of 200 characters.</remarks>
    public CreateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(50)
            .WithMessage("Name must be less than 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description must be less than 200 characters");
    }
}