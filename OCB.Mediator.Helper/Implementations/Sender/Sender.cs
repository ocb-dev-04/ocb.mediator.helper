using System.Reflection;
using System.Collections.Concurrent;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Sender;
using Microsoft.Extensions.DependencyInjection;
using OCB.Mediator.Helper.Abstractions.Messaging;
using OCB.Mediator.Helper.Abstractions.Pipelines;

namespace OCB.Mediator.Helper.Implementations.Sender;

/// <summary>
/// <see cref="ISender"/> implementation.
/// </summary>
public class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

    private static readonly ConcurrentDictionary<Type, Delegate> _compiledHandlers = new();

    /// <summary>
    /// <see cref="Sender"/> public constructor.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public Sender(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        => Dispatch<TResponse>(query.GetType(), query, cancellationToken);

    /// <inheritdoc/>
    public Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
        => Dispatch(command.GetType(), command, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => Dispatch<TResponse>(command.GetType(), command, cancellationToken);

    #region Dispatch for caching

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

    private Task<Result> Dispatch(Type requestType, object request, CancellationToken cancellationToken)
    {
        Func<IServiceProvider, object, CancellationToken, Task<Result>>? func = (Func<IServiceProvider, object, CancellationToken, Task<Result>>)
            _compiledHandlers.GetOrAdd(requestType, static type =>
            {
                return new Func<IServiceProvider, object, CancellationToken, Task<Result>>(
                    (sp, req, ct) =>
                    {
                        var sender = (Sender)sp.GetRequiredService(typeof(Sender));
                        return sender.InvokeWithPipeline((ICommand)req, ct);
                    });
            });

        return func(_serviceProvider, request, cancellationToken);
    }

    #endregion

    #region Invoke methods

    private Task<Result<TResponse>> InvokeWithPipeline<TResponse>(Type concreteType, object request, CancellationToken cancellationToken)
    {
        Type handlerInterface;
        if (typeof(IQuery<TResponse>).IsAssignableFrom(concreteType))
        {
            handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(concreteType, typeof(TResponse));
        }
        else if (typeof(ICommand<TResponse>).IsAssignableFrom(concreteType))
        {
            handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(concreteType, typeof(TResponse));
        }
        else
        {
            throw new InvalidOperationException($"Unsupported request type: {concreteType.Name}");
        }

        if (handlerInterface is null)
            throw new InvalidOperationException($"Handler interface not found for request type: {concreteType.FullName}");

        object handler = _serviceProvider.GetRequiredService(handlerInterface);
        MethodInfo? method = handlerInterface.GetMethod("Handle");

        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            object task = method.Invoke(handler, new object[] { request, cancellationToken })!;
            return (Task<Result<TResponse>>)task;
        };

        // Get behaviors using the concrete type instead of the interface type
        Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(concreteType, typeof(TResponse));
        IEnumerable<object?> behaviors = _serviceProvider
            .GetServices(behaviorType)
            .Reverse();

        foreach (object? behavior in behaviors)
        {
            if (behavior is null) continue;

            RequestHandlerDelegate<TResponse> currentDelegate = handlerDelegate;
            handlerDelegate = () =>
            {
                object typedBehavior = behavior;
                MethodInfo? handleMethod = behaviorType.GetMethod("Handle");
                if (handleMethod == null)
                    throw new InvalidOperationException($"Handle method not found on behavior type {behaviorType.FullName}");

                return (Task<Result<TResponse>>)handleMethod.Invoke(
                    typedBehavior,
                    new object[] { request, cancellationToken, currentDelegate })!;
            };
        }

        return handlerDelegate();
    }

    private async Task<Result> InvokeWithPipeline(ICommand command, CancellationToken cancellationToken)
    {
        Type concreteType = command.GetType();
        Type handlerInterface = typeof(ICommandHandler<>).MakeGenericType(concreteType);
        object handler = _serviceProvider.GetRequiredService(handlerInterface);

        MethodInfo? method = handlerInterface.GetMethod("Handle");
        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        RequestHandlerDelegate<Result> handlerDelegate = async () =>
        {
            object task = method.Invoke(handler, new object[] { command, cancellationToken })!;
            return await (Task<Result>)task;
        };

        Type pipelineInterfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(concreteType, typeof(Result));
        IEnumerable<object?> behaviors = _serviceProvider.GetServices(pipelineInterfaceType).Reverse();
        foreach (object? behavior in behaviors)
        {
            if (behavior is null) continue;

            RequestHandlerDelegate<Result> next = handlerDelegate;
            handlerDelegate = async () =>
            {
                MethodInfo? behaviorHandle = pipelineInterfaceType.GetMethod("Handle");
                if (behaviorHandle is null)
                    throw new InvalidOperationException($"Handle method not found in behavior {pipelineInterfaceType.FullName}");

                object pipelineTask = behaviorHandle.Invoke(behavior, new object[] { command, cancellationToken, next })!;
                dynamic awaited = await (dynamic)pipelineTask;
                return (Result)awaited;
            };
        }

        return await handlerDelegate();
    }

    #endregion
}
