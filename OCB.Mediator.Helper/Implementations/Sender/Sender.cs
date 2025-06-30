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
    private static readonly ConcurrentDictionary<Type, Delegate> _compiledHandlers
        = new();

    /// <summary>
    /// <see cref="Sender"/> public constructor.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public Sender(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <inheritdoc/>
    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        => Dispatch<TResponse>(query.GetType(), query, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => Dispatch<TResponse>(command.GetType(), command, cancellationToken);

    #region Dispatch for caching

    /// <summary>
    /// Dispatches a request to the appropriate handler and executes it, returning the result.
    /// </summary>
    /// <remarks>This method uses a caching mechanism to store compiled handlers for request types, improving
    /// performance by avoiding repeated handler resolution. The handler is invoked with a pipeline, which may include
    /// additional processing steps such as validation or logging.</remarks>
    /// <typeparam name="TResponse">The type of the response expected from the handler.</typeparam>
    /// <param name="requestType">The type of the request to be dispatched. This is used to locate the appropriate handler.</param>
    /// <param name="request">The request object to be processed by the handler. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TResponse}"/>
    /// object representing the outcome of the request processing.</returns>
    private Task<Result<TResponse>> Dispatch<TResponse>(Type requestType, object request, CancellationToken cancellationToken)
    {
        Func<IServiceProvider, object, CancellationToken, Task<Result<TResponse>>>? func = (Func<IServiceProvider, object, CancellationToken, Task<Result<TResponse>>>)
            _compiledHandlers.GetOrAdd(requestType, static type =>
            {
                return new Func<IServiceProvider, object, CancellationToken, Task<Result<TResponse>>>(
                    (sp, req, ct) =>
                    {
                        Sender sender = (Sender)sp.GetRequiredService(typeof(Sender));
                        return sender.InvokeWithPipeline<TResponse>(type, req, ct);
                    });
            });

        return func(_serviceProvider, request, cancellationToken);
    }

    #endregion

    #region Invoke methods

    /// <summary>
    /// Invokes a request handler with an optional pipeline of behaviors, processing the specified request and returning
    /// a result.
    /// </summary>
    /// <remarks>This method dynamically resolves the appropriate handler and pipeline behaviors for the given
    /// request type using dependency injection. It ensures that the handler and behaviors conform to the expected
    /// interfaces and return valid results.</remarks>
    /// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
    /// <param name="concreteType">The concrete type of the request being processed. Must implement either <see cref="IQuery{TResponse}"/> or <see
    /// cref="ICommand{TResponse}"/>.</param>
    /// <param name="request">The request object to be handled. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the request type is unsupported, the handler interface or method cannot be resolved, or the handler or
    /// pipeline behavior returns an invalid result.</exception>
    private Task<Result<TResponse>> InvokeWithPipeline<TResponse>(Type concreteType, object request, CancellationToken cancellationToken)
    {
        Type? handlerInterface;
        if (typeof(IQuery<TResponse>).IsAssignableFrom(concreteType))
            handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(concreteType, typeof(TResponse));
        else if (typeof(ICommand<TResponse>).IsAssignableFrom(concreteType))
            handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(concreteType, typeof(TResponse));
        else
            throw new InvalidOperationException($"Unsupported request type: {concreteType.Name}");

        if (handlerInterface is null)
            throw new InvalidOperationException($"Handler interface not found for request type: {concreteType.FullName}");

        object handler = _serviceProvider.GetRequiredService(handlerInterface);
        MethodInfo? method = handlerInterface.GetMethod("Handle");
        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            object? result = method.Invoke(handler, new object[] { request, cancellationToken })!;
            if (result is null)
                throw new InvalidOperationException($"Handle method returned null for handler type {handlerInterface.FullName}");

            if (result is not Task<Result<TResponse>> task)
                throw new InvalidOperationException($"Handle method did not return Task<Result<{typeof(TResponse).Name}>> for handler type {handlerInterface.FullName}");

            return task;
        };

        // Get behaviors using the concrete type instead of the interface type
        Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(concreteType, typeof(TResponse));
        IEnumerable<object?> behaviors = _serviceProvider.GetServices(behaviorType).Reverse();
        foreach (object? behavior in behaviors)
        {
            if (behavior is null)
                continue;

            RequestHandlerDelegate<TResponse> currentDelegate = handlerDelegate;
            handlerDelegate = () =>
            {
                MethodInfo? handleMethod = behaviorType.GetMethod("Handle");
                if (handleMethod == null)
                    throw new InvalidOperationException($"Handle method not found on behavior type {behaviorType.FullName}");

                object? result = handleMethod.Invoke(behavior, new object[] { request, cancellationToken, currentDelegate });
                if (result is null)
                    throw new InvalidOperationException($"Handle method returned null for behavior type {behaviorType.FullName}");

                if (result is not Task<Result<TResponse>> task)
                    throw new InvalidOperationException($"Handle method did not return Task<Result<{typeof(TResponse).Name}>> for behavior type {behaviorType.FullName}");

                return task;
            };
        }

        return handlerDelegate();
    }

    #endregion
}
