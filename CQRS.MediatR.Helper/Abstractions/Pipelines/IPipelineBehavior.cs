using Shared.Common.Helper.ErrorsHandler;

namespace CQRS.MediatR.Helper.Abstractions.Pipelines;

/// <summary>
/// IPipelineBehavior interface defines a behavior that can be applied to requests in the CQRS pattern.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface IPipelineBehavior<TRequest>
{
    Task<Result> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate next);
}

/// <summary>
/// IPipelineBehavior interface defines a behavior that can be applied to requests in the CQRS pattern.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Handles the request and allows for additional processing before or after the request is handled by the next handler in the pipeline.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next);
}

/// <summary>
/// Delegate to use when handler does not return a result.
/// </summary>
/// <returns></returns>
public delegate Task<Result> RequestHandlerDelegate();

/// <summary>
/// Delegate to use when handler returns a result.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <returns></returns>
public delegate Task<Result<TResponse>> RequestHandlerDelegate<TResponse>();