using Newtonsoft.Json;
using Application.Events;
using Microsoft.Extensions.Logging;
using OCB.Mediator.Helper.Abstractions.Notification;

namespace Application.EventsHandlers;

/// <summary>
/// Handles notifications of type <see cref="TestEvent"/>.
/// </summary>
/// <remarks>This class processes incoming <see cref="TestEvent"/> notifications by logging their details. It is
/// designed to be used as part of a notification handling pipeline.</remarks>
internal sealed class TestEventHandler
    : INotificationHandler<TestEvent>
{
    private readonly ILogger<TestEventHandler> _logger;

    /// <summary>
    /// <see cref="TestEventHandler"/> public constructor
    /// </summary>
    /// <param name="logger">The logger instance used to log events and diagnostic information.</param>
    public TestEventHandler(ILogger<TestEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the specified <see cref="TestEvent"/> notification asynchronously.
    /// </summary>
    /// <remarks>This method logs the details of the <see cref="TestEvent"/> notification.  It is designed to
    /// complete immediately and does not perform any additional processing.</remarks>
    /// <param name="notification">The event notification to process. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task HandleAsync(TestEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("TestEventHandler: {Message}", JsonConvert.SerializeObject(notification));
        
        // uncomment the following line to test the ExceptionHandlingNotificationPipelineBehavior
        //if(notification.Name.Length > 1)
        //    throw new Exception("Exception only for testing the ExceptionHandlingNotificationPipelineBehavior");

        return Task.CompletedTask;
    }
}
