using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace Application.Behaviors.HandlerPipelines;

/// <summary>
/// Implements pipeline behavior for logging the execution of requests and their responses.
/// </summary>
/// <remarks>This behavior logs the start and end of request handling, including the request name, timestamp,  and
/// elapsed time. If an exception occurs during request handling, it logs the error details  along with the elapsed time
/// before rethrowing the exception.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. Must be non-null.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler. Must be non-null.</typeparam>
public sealed class LoggerPipelineBehavior<TRequest, TResponse>
    : IRequestPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : notnull
{
    private readonly ILogger<LoggerPipelineBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// <see cref="LoggerPipelineBehavior{TRequest, TResponse}"/> public constructor
    /// </summary>
    /// <param name="logger">The logger instance used to log information about the request and response processing. Cannot be <see
    /// langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is <see langword="null"/>.</exception>
    public LoggerPipelineBehavior(ILogger<LoggerPipelineBehavior<TRequest, TResponse>> logger)
        => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Handles the processing of a request by invoking the next handler in the pipeline.
    /// </summary>
    /// <remarks>This method logs the start and end of the request handling process, including timing
    /// information.  If an exception occurs during processing, it logs the error and rethrows the exception.</remarks>
    /// <param name="request">The request object to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/> 
    /// representing the outcome of the request processing.</returns>
    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        string requestName = typeof(TRequest).Name;
        long startTimestamp = Stopwatch.GetTimestamp();

        _logger.LogInformation("--> Handling {RequestName} at {TimestampUtc}", requestName, DateTime.UtcNow);

        try
        {
            Result<TResponse> response = await next();

            _logger.LogInformation("--> Handled {RequestName} in {ElapsedMs}ms",
                requestName, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "--> Error handling {RequestName} after {ElapsedMs}ms",
                requestName, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
            throw;
        }
    }
}
