using FluentValidation;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace OCB.Mediator.Helper.Behaviors.Pipelines;

/// <summary>
/// Implements a pipeline behavior that performs validation on a request before passing it to the next handler.
/// </summary>
/// <remarks>This behavior uses a collection of validators to validate the incoming request. If validation errors
/// are found, a failed <see cref="Result{TResponse}"/> carrying a <see cref="ValidationError"/> is returned — no
/// exception is thrown, keeping validation failures on the (cheap) result path. If no validators are configured or no
/// validation errors are found, the request is passed to the next handler in the pipeline.</remarks>
/// <typeparam name="TRequest">The type of the request being processed. Must be non-null.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler. Must be non-null.</typeparam>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IRequestPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// <see cref="ValidationPipelineBehavior{TRequest, TResponse}"/> public constructor
    /// </summary>
    /// <param name="validators"></param>
    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators, nameof(validators));

        _validators = validators;
    }

    /// <summary>
    /// Processes a request by validating it using the configured validators and then invoking the next handler in the
    /// pipeline.
    /// </summary>
    /// <remarks>If no validators are configured, the request is passed directly to the next handler without
    /// validation. If validation errors are found, a failed result containing a <see cref="ValidationError"/> is
    /// returned and the handler is not invoked. The error dictionary is only built on the failure path, so the happy
    /// path allocates nothing beyond the validation results themselves.</remarks>
    /// <param name="request">The request object to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response from the next handler,
    /// or a failed result with a <see cref="ValidationError"/> when validation fails.</returns>
    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        // If no validators, skip validation
        if (!_validators.Any())
            return await next();

        // Execute all validators asynchronously and await them properly
        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(request, cancellationToken))
        );

        bool isValid = true;
        foreach (FluentValidation.Results.ValidationResult validationResult in validationResults)
        {
            if (!validationResult.IsValid)
            {
                isValid = false;
                break;
            }
        }

        if (isValid)
            return await next();

        // Failure path: group errors by property name (camelCase)
        Dictionary<string, string[]> errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => ToCamelCase(group.Key),
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray()
            );

        return Result.Failure<TResponse>(new ValidationError(errors));
    }

    private static string ToCamelCase(string propertyName)
    {
        if (propertyName.Length == 0 || char.IsLower(propertyName[0]))
            return propertyName;

        return string.Create(propertyName.Length, propertyName, static (chars, source) =>
        {
            source.CopyTo(chars);
            chars[0] = char.ToLowerInvariant(chars[0]);
        });
    }
}
