using Newtonsoft.Json;
using Application.Events;
using Microsoft.Extensions.Logging;
using OCB.Mediator.Helper.Abstractions.Notification;

namespace Application.EventsHandlers;

internal sealed class TestEventHandler
    : INotificationHandler<TestEvent>
{
    private readonly ILogger<TestEventHandler> _logger;

    public TestEventHandler(ILogger<TestEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TestEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("TestEventHandler: {Message}", JsonConvert.SerializeObject(notification));
        return Task.CompletedTask;
    }
}
