using OCB.Mediator.Helper.ResultPattern;

namespace OCB.Mediator.Helper.Abstractions.Pipelines;

/// <summary>
/// Defines a behavior in the processing pipeline for handling requests and responses.
/// </summary>
/// <remarks>Implementations of this interface can perform additional processing before or after the request is
/// handled by the next handler in the pipeline. This allows for cross-cutting concerns such as logging, validation, or
/// exception handling to be applied consistently across requests.</remarks>
/// <typeparam name="TRequest">The type of the request being processed.</typeparam>
/// <typeparam name="TResponse">The type of the response returned after processing the request.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Processes the specified request and invokes the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request to be processed. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/>
    /// object representing the outcome of the request processing.</returns>
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next);
}

/// <summary>
/// Represents a delegate that handles a request and returns a task containing a result of the specified response type.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the delegate.</typeparam>
/// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/> object
/// representing the outcome of the request.</returns>
public delegate Task<Result<TResponse>> RequestHandlerDelegate<TResponse>();