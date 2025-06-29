using OCB.Mediator.Helper.ResultPattern;

namespace OCB.Mediator.Helper.Abstractions.Pipelines;

/// <summary>
/// Defines a behavior in a request processing pipeline, allowing pre- and post-processing of requests and responses.
/// </summary>
/// <remarks>Implementations of this interface can be used to add cross-cutting concerns, such as logging,
/// validation, or caching, to the request handling pipeline. The behavior is invoked before and/or after the next
/// handler in the pipeline.</remarks>
/// <typeparam name="TRequest">The type of the request being processed.</typeparam>
public interface IPipelineBehavior<TRequest>
{
    /// <summary>
    /// Processes the specified request and invokes the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request object containing the data to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline. Cannot be null.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The result contains the outcome of the
    /// request processing.</returns>
    Task<Result> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate next);
}

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
/// Represents a delegate that handles a request and returns a <see cref="Result"/> asynchronously.
/// </summary>
/// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation,  where the result is a <see cref="Result"/>
/// object containing the outcome of the request.</returns>
public delegate Task<Result> RequestHandlerDelegate();

/// <summary>
/// Represents a delegate that handles a request and returns a task containing a result of the specified response type.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the delegate.</typeparam>
/// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/> object
/// representing the outcome of the request.</returns>
public delegate Task<Result<TResponse>> RequestHandlerDelegate<TResponse>();