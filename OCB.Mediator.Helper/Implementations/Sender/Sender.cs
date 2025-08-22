using System.Reflection;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Sender;
using Microsoft.Extensions.DependencyInjection;
using OCB.Mediator.Helper.Abstractions.Messaging;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace OCB.Mediator.Helper.Implementations.Sender;

/// <summary>
/// <see cref="ISender"/> implementation
/// </summary>
/// <remarks>The <see cref="Sender"/> class acts as a mediator for processing requests such as queries and
/// commands. It resolves the appropriate handler for each request type using dependency injection and executes the
/// handler, optionally applying pipeline behaviors. This implementation uses direct reflection without caching
/// for minimal memory footprint.</remarks>
public class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        => DispatchDirect<TResponse>(query, cancellationToken);

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => DispatchDirect<TResponse>(command, cancellationToken);

    /// <summary>
    /// Dispatches a request directly to its handler using reflection without caching.
    /// </summary>
    /// <remarks>This method performs type resolution and method discovery on each call,
    /// trading performance for reduced memory usage and avoiding potential cache-related memory leaks.</remarks>
    private Task<Result<TResponse>> DispatchDirect<TResponse>(object request, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();

        // Resolve handler interface through reflection (no caching)
        Type handlerInterface = ResolveHandlerInterface<TResponse>(requestType);

        // Get handler instance from DI container
        object handler = _serviceProvider.GetRequiredService(handlerInterface);

        // Get Handle method via reflection
        MethodInfo handleMethod = handlerInterface.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        // Create base handler delegate
        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            object? result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
            if (result is not Task<Result<TResponse>> task)
                throw new InvalidOperationException($"Handle method did not return Task<Result<{typeof(TResponse).Name}>>");
            return task;
        };

        // Apply pipeline behaviors (resolved fresh each time)
        Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _serviceProvider.GetServices(behaviorType);

        foreach (object behavior in behaviors.Reverse())
        {
            if (behavior == null) continue;

            // Get behavior Handle method via reflection
            MethodInfo behaviorMethod = behaviorType.GetMethod("Handle")
                ?? throw new InvalidOperationException($"Handle method not found on behavior type {behaviorType.FullName}");

            RequestHandlerDelegate<TResponse> currentDelegate = handlerDelegate;
            handlerDelegate = () =>
            {
                object? result = behaviorMethod.Invoke(behavior, new object[] { request, cancellationToken, currentDelegate });
                if (result is not Task<Result<TResponse>> task)
                    throw new InvalidOperationException($"Handle method did not return Task<Result<{typeof(TResponse).Name}>>");
                return task;
            };
        }

        return handlerDelegate();
    }

    /// <summary>
    /// Resolves the appropriate handler interface for the given request type.
    /// </summary>
    /// <remarks>This method uses reflection to determine whether the request implements
    /// IQuery&lt;TResponse&gt; or ICommand&lt;TResponse&gt; and returns the corresponding handler interface.</remarks>
    private static Type ResolveHandlerInterface<TResponse>(Type requestType)
    {
        // Check for IQuery<TResponse> interface
        Type? queryInterface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

        if (queryInterface != null)
        {
            Type responseType = queryInterface.GetGenericArguments()[0];
            return typeof(IQueryHandler<,>).MakeGenericType(requestType, responseType);
        }

        // Check for ICommand<TResponse> interface
        Type? commandInterface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        if (commandInterface != null)
        {
            Type responseType = commandInterface.GetGenericArguments()[0];
            return typeof(ICommandHandler<,>).MakeGenericType(requestType, responseType);
        }

        throw new InvalidOperationException(
            $"Unsupported request type: {requestType.Name}. Must implement IQuery<TResponse> or ICommand<TResponse>");
    }
}