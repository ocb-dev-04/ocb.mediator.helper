using Microsoft.Extensions.Logging;
using OCB.Mediator.Helper.Abstractions.Notification;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace Application.Behaviors.EventPipelines;

/// <summary>
/// Provides a pipeline behavior for handling exceptions that occur during the processing of notifications.
/// </summary>
/// <remarks>This behavior intercepts exceptions thrown during the execution of the notification pipeline and logs
/// the error. If an exception handler delegate is provided, it will be invoked with the caught exception. If no handler
/// is specified, the exception will be rethrown.</remarks>
/// <typeparam name="TNotification">The type of notification being processed. Must implement <see cref="INotification"/>.</typeparam>
public sealed class ExceptionHandlingNotificationPipelineBehavior<TNotification> 
    : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
{
    private readonly ILogger<ExceptionHandlingNotificationPipelineBehavior<TNotification>> _logger;
    private readonly Func<Exception, Task>? _onException;

    /// <summary>
    /// <see cref="ExceptionHandlingNotificationPipelineBehavior{TNotification}"/> public constructor
    /// </summary>
    /// <param name="logger">The logger instance used to log exception details. This parameter cannot be <see langword="null"/>.</param>
    /// <param name="onException">An optional callback function that is invoked when an exception occurs. If provided, the callback is executed
    /// with the exception as its parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is <see langword="null"/>.</exception>
    public ExceptionHandlingNotificationPipelineBehavior(ILogger<ExceptionHandlingNotificationPipelineBehavior<TNotification>> logger, Func<Exception, Task>? onException = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _onException = onException;
    }

    /// <summary>
    /// Handles the processing of a notification within a pipeline, allowing for exception handling and logging.
    /// </summary>
    /// <remarks>If an exception occurs during the execution of the pipeline, the exception is logged. If an
    /// exception handler is configured, it will be invoked; otherwise, the exception will be rethrown.</remarks>
    /// <param name="notification">The notification object being processed. Represents the data or event to be handled.</param>
    /// <param name="next">A delegate representing the next step in the pipeline. This must be invoked to continue processing.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(TNotification notification, Func<Task> next, CancellationToken cancellationToken)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "--> Exception caught in notification handling pipeline");

            if (_onException is not null)
                await _onException(ex);
            else
                throw;
        }
    }
}
