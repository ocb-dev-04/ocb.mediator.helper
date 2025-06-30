using FluentValidation;

namespace Application.UsesCases.Delete;

/// <summary>
/// Validates the properties of a <see cref="DeleteCommand"/> to ensure they meet required conditions.
/// </summary>
/// <remarks>This validator enforces the following rules for the <c>Id</c> property of <see
/// cref="DeleteCommand"/>: <list type="bullet"> <item><description>The <c>Id</c> must not be
/// empty.</description></item> <item><description>The <c>Id</c> must not be null.</description></item>
/// <item><description>The <c>Id</c> must not be the default value of <see cref="Guid"/>.</description></item> </list>
/// If any of these conditions are violated, a validation error will be generated with an appropriate message.</remarks>
internal sealed class DeleteCommandValidator 
    : AbstractValidator<DeleteCommand>
{
    /// <summary>
    /// <see cref="DeleteCommand"/> public constructor
    /// </summary>
    /// <remarks>This validator enforces the following rules for the <c>Id</c> property of the command: <list
    /// type="bullet"> <item><description>The <c>Id</c> must not be empty.</description></item> <item><description>The
    /// <c>Id</c> must not be null.</description></item> <item><description>The <c>Id</c> must not be the default value
    /// of <see cref="Guid"/>.</description></item> </list> Use this validator to ensure that a <see
    /// cref="DeleteCommand"/> is properly constructed before processing.</remarks>
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