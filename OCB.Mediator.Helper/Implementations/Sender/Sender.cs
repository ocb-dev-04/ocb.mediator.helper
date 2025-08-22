using System.Reflection;
using System.Collections.Concurrent;
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
/// commands. It resolves  the appropriate handler for each request type using dependency injection and executes the
/// handler, optionally applying  pipeline behaviors. This class supports caching of compiled handlers to improve
/// performance during repeated dispatches.</remarks>
public class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    // Cache para métodos compilados (más eficiente que delegates completos)
    private static readonly ConcurrentDictionary<Type, CompiledHandler> _compiledHandlers = new();
    private static readonly ConcurrentDictionary<Type, CompiledMethod> _handleMethods = new();
    private static readonly ConcurrentDictionary<Type, CompiledMethod> _behaviorMethods = new();

    public Sender(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        => DispatchOptimized<TResponse>(query, cancellationToken);

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => DispatchOptimized<TResponse>(command, cancellationToken);

    private Task<Result<TResponse>> DispatchOptimized<TResponse>(object request, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();

        // Cache del handler compilado (sin delegates pesados)
        CompiledHandler compiledHandler = _compiledHandlers.GetOrAdd(requestType, static type =>
        {
            Type handlerInterface;

            // Buscar interfaz IQuery<TResponse>
            Type? queryInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

            if (queryInterface != null)
            {
                Type queryType = queryInterface.GetGenericArguments()[0];
                handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(type, queryType);
            }
            else
            {
                // Buscar interfaz ICommand<TResponse>
                Type? commandInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

                if (commandInterface != null)
                {
                    Type commandType = commandInterface.GetGenericArguments()[0];
                    handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(type, commandType);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported request type: {type.Name}. Must implement IQuery<TResponse> or ICommand<TResponse>");
                }
            }

            // Obtener el response type para el behavior
            Type responseType = handlerInterface.GetGenericArguments()[1];
            Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(type, responseType);

            return new CompiledHandler(handlerInterface, behaviorType);
        });

        return InvokeOptimized<TResponse>(compiledHandler, request, cancellationToken);
    }

    private Task<Result<TResponse>> InvokeOptimized<TResponse>(CompiledHandler compiledHandler, object request, CancellationToken cancellationToken)
    {
        // Resolver handler
        object handler = _serviceProvider.GetRequiredService(compiledHandler.HandlerInterface);

        // Extraer el response type del handler interface
        Type responseType = compiledHandler.HandlerInterface.GetGenericArguments()[1];

        // Cache del método Handle
        CompiledMethod handleMethod = _handleMethods.GetOrAdd(compiledHandler.HandlerInterface, static type =>
        {
            MethodInfo? method = type.GetMethod("Handle");
            if (method == null)
                throw new InvalidOperationException($"Handle method not found in handler: {type.FullName}");
            return new CompiledMethod(method);
        });

        // Crear delegate del handler principal
        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            object? result = handleMethod.Method.Invoke(handler, new object[] { request, cancellationToken });
            if (result is not Task<Result<TResponse>> task)
                throw new InvalidOperationException($"Handle method did not return Task<Result<{responseType.Name}>>");
            return task;
        };

        // Aplicar behaviors (optimizado)
        var behaviors = _serviceProvider.GetServices(compiledHandler.BehaviorType);
        foreach (object behavior in behaviors.Reverse())
        {
            if (behavior == null) continue;

            // Cache del método Handle del behavior
            CompiledMethod behaviorMethod = _behaviorMethods.GetOrAdd(compiledHandler.BehaviorType, static type =>
            {
                MethodInfo? method = type.GetMethod("Handle");
                if (method == null)
                    throw new InvalidOperationException($"Handle method not found on behavior type {type.FullName}");
                return new CompiledMethod(method);
            });

            RequestHandlerDelegate<TResponse> currentDelegate = handlerDelegate;
            handlerDelegate = () =>
            {
                object? result = behaviorMethod.Method.Invoke(behavior, new object[] { request, cancellationToken, currentDelegate });
                if (result is not Task<Result<TResponse>> task)
                    throw new InvalidOperationException($"Handle method did not return Task<Result<{responseType.Name}>>");
                return task;
            };
        }

        return handlerDelegate();
    }

    // Structs ligeros para el cache (mucho más eficientes que delegates)
    private readonly record struct CompiledHandler(Type HandlerInterface, Type BehaviorType);
    private readonly record struct CompiledMethod(MethodInfo Method);
}

// Usar el delegate existente: OCB.Mediator.Helper.Abstractions.Pipelines.RequestHandlerDelegate<TResponse>