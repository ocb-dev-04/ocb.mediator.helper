using FluentValidation;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace OCB.Mediator.Helper.Behaviors.Pipelines;

/// <summary>
/// Implements a pipeline behavior that performs validation on a request before passing it to the next handler.
/// </summary>
/// <remarks>This behavior uses a collection of validators to validate the incoming request. If validation errors
/// are found,  a <see cref="Exceptions.ValidationException"/> is thrown containing the validation errors. If no
/// validators are  configured or no validation errors are found, the request is passed to the next handler in the
/// pipeline.</remarks>
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
    /// validation. If validation errors are found, a <see cref="Exceptions.ValidationException"/> is thrown containing
    /// the validation errors.</remarks>
    /// <param name="request">The request object to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response from the next handler.</returns>
    /// <exception cref="Exceptions.ValidationException">Thrown when one or more validation errors are detected in the request.</exception>
    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        // If no validators, skip validation
        if (!_validators.Any())
            return await next();

        // Execute all validators asynchronously and await them properly
        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(request, cancellationToken))
        );

        // Group errors by property name
        IDictionary<string, string[]> errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => string.Concat(char.ToLowerInvariant(group.Key[0]), group.Key[1..]),
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray()
            );

        // If there are validation errors, throw exception
        if (errors.Any())
            throw new Exceptions.ValidationException(errors);

        return await next();
    }
}
