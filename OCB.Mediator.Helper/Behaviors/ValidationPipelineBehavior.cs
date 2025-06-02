using FluentValidation;
using OCB.Mediator.Helper.ErrorHandler;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace OCB.Mediator.Helper.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators, nameof(validators));

        _validators = validators;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (!_validators.Any())
            return await next();

        ValidationError[] errors = _validators
            .Select(async validator
                => await validator.ValidateAsync(request))
            .SelectMany(validationResult
                => validationResult.Result.Errors)
            .Where(validationFailure
                => validationFailure is not null)
            .Select(failure
                => new ValidationError(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Any())
            throw new Exceptions.ValidationException(errors);

        return await next();
    }
}
