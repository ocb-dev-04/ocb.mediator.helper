using Microsoft.Extensions.Logging;
using OCB.Mediator.Helper.Abstractions.Notification;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace OCB.Mediator.Helper.Behaviors.Notifications;

/// <summary>
/// Defines a pipeline behavior for handling exceptions during the processing of notifications.
/// </summary>
/// <typeparam name="TNotification"></typeparam>
public sealed class ExceptionHandlingNotificationPipelineBehavior<TNotification> 
    : INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
{
    private readonly ILogger<ExceptionHandlingNotificationPipelineBehavior<TNotification>> _logger;
    private readonly Func<Exception, Task>? _onException;

    /// <summary>
    /// <see cref="ExceptionHandlingNotificationPipelineBehavior{TNotification}"/> public constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="onException"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ExceptionHandlingNotificationPipelineBehavior(ILogger<ExceptionHandlingNotificationPipelineBehavior<TNotification>> logger, Func<Exception, Task>? onException = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _onException = onException;
    }

    public async Task HandleAsync(TNotification notification, CancellationToken cancellationToken, Func<Task> next)
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
