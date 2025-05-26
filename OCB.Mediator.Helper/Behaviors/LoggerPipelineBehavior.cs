using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Common.Helper.ErrorsHandler;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace OCB.Mediator.Helper.Behaviors.Pipelines;

/// <summary>
/// Pipeline behavior that logs the handling of requests.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public sealed class LoggerPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : notnull
{
    private readonly ILogger<LoggerPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggerPipelineBehavior(ILogger<LoggerPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        string requestName = typeof(TRequest).Name;
        DateTime startTime = DateTime.UtcNow;
        Stopwatch stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("--> Handling {RequestName} at {TimestampUtc}", requestName, startTime);

        try
        {
            Result<TResponse> response = await next();

            stopwatch.Stop();
            _logger.LogInformation("--> Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "--> Error handling {RequestName} after {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
