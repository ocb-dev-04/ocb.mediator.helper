using Microsoft.Extensions.DependencyInjection;
using OCB.Mediator.Helper.Abstractions.Notification;
using System.Reflection;

namespace OCB.Mediator.Helper.Implementations.Notification;

/// <summary>
/// <see cref="INotificationDispatcher"/> implementation that dispatches notifications to all registered handlers.
/// </summary>
internal sealed class NotificationDispatcher
    : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// <see cref="NotificationDispatcher"/> public constructor that accepts an <see cref="IServiceProvider"/> to resolve dependencies.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public NotificationDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task DispatchAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Type notificationType = notification.GetType();
        Type handlerInterface = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        object?[] handlers = scope.ServiceProvider.GetServices(handlerInterface).Reverse().ToArray();
        if (!handlers.Any()) return;

        Task[] tasks = handlers
            .Select(handler =>
            {
                MethodInfo? method = handlerInterface.GetMethod("HandleAsync");
                if (method is null)
                    return Task.CompletedTask;

                return (Task)method.Invoke(handler, new object[] { notification, cancellationToken })!;
            })
            .ToArray();

        await Task.WhenAll(tasks);
    }
}
