using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OCB.Mediator.Helper.Abstractions.Notification;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace OCB.Mediator.Helper.Implementations.Notification;

/// <summary>
/// <see cref="INotificationDispatcher"/> implementation that dispatches notifications to all registered handlers.
/// </summary>
internal sealed class NotificationDispatcher
    : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider; 
    private readonly ILogger<NotificationDispatcher> _logger;
    
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, INotification, CancellationToken, Task[]>> _cachedDispatchers = new();

    /// <summary>
    /// <see cref="NotificationDispatcher"/> public constructor that accepts an <see cref="IServiceProvider"/> to resolve dependencies.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public NotificationDispatcher(
        IServiceProvider serviceProvider, 
        ILogger<NotificationDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task DispatchAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        Func<IServiceProvider, INotification, CancellationToken, Task[]> dispatcher = _cachedDispatchers.GetOrAdd(notification.GetType(), CreateDispatcher);
        Task[] tasks = dispatcher(_serviceProvider, notification, cancellationToken);
        await Task.WhenAll(tasks);

        stopwatch.Stop();
        _logger.LogInformation("--> DispatchAsync<{NotificationType}> took {ElapsedMilliseconds} ms",
            typeof(TNotification).Name, stopwatch.ElapsedMilliseconds);
    }

    private static Func<IServiceProvider, INotification, CancellationToken, Task[]> CreateDispatcher(Type notificationType)
    {
        Type handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        return (serviceProvider, notification, cancellationToken) =>
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            Type notificationType = notification.GetType();
            Type handlerInterface = typeof(INotificationHandler<>).MakeGenericType(notificationType);
            Task[] tasks = Enumerable.Empty<Task>().ToArray();

            object?[] handlers = scope.ServiceProvider.GetServices(handlerInterface).Reverse().ToArray();
            if (!handlers.Any()) return tasks;

            tasks = handlers
                .Select(handler =>
                {
                    MethodInfo? method = handlerInterface.GetMethod("HandleAsync");
                    if (method is null)
                        return Task.CompletedTask;

                    return (Task)method.Invoke(handler, new object[] { notification, cancellationToken })!;
                })
                .ToArray();

            return tasks;
        };
    }
}
