using System.Reflection;
using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Sender;
using Microsoft.Extensions.DependencyInjection;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using CQRS.MediatR.Helper.Abstractions.Pipelines;

namespace CQRS.MediatR.Helper.Implementations.Sender;

/// <summary>
/// <see cref="ISender"/> implementation.
/// </summary>
public class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;

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
        => InvokeWithPipeline<IQuery<TResponse>, TResponse>(query, cancellationToken);

    /// <inheritdoc/>
    public Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
        => InvokeWithPipeline(command, cancellationToken);

    /// <inheritdoc/>
    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => InvokeWithPipeline<ICommand<TResponse>, TResponse>(command, cancellationToken);

    #region Pipeline to command with returns

    /// <summary>
    /// Handle the pipelines and invoke the handler.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private Task<Result<TResponse>> InvokeWithPipeline<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : notnull
    {
        Type requestType = request.GetType();
        Type handlerInterface = request switch
        {
            TRequest r when typeof(IQuery<TResponse>).IsAssignableFrom(r.GetType()) =>
                typeof(IQueryHandler<,>).MakeGenericType(r.GetType(), typeof(TResponse)),

            TRequest r when typeof(ICommand<TResponse>).IsAssignableFrom(r.GetType()) =>
                typeof(ICommandHandler<,>).MakeGenericType(r.GetType(), typeof(TResponse)),

            TRequest r when typeof(ICommand).IsAssignableFrom(r.GetType()) =>
                typeof(ICommandHandler<>).MakeGenericType(r.GetType()),

            _ => throw new InvalidOperationException($"Unsupported request type: {requestType.Name}")
        };

        if (handlerInterface is null)
            throw new InvalidOperationException($"Handler interface not found for request type: {typeof(TRequest).FullName}");

        object handler = _serviceProvider.GetRequiredService(handlerInterface);
        MethodInfo? method = handlerInterface.GetMethod("Handle");

        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            object task = method.Invoke(handler, new object[] { request, cancellationToken })!;
            return (Task<Result<TResponse>>)task;
        };

        IEnumerable<dynamic>? behaviors = _serviceProvider
            .GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(typeof(TRequest), typeof(TResponse)))
            .Cast<dynamic>()
            .Reverse();

        foreach (var behaviorObj in behaviors)
            handlerDelegate = WrapWithBehavior<TRequest, TResponse>(behaviorObj, handlerDelegate, request, cancellationToken);

        return handlerDelegate();
    }

    private static RequestHandlerDelegate<TResponse> WrapWithBehavior<TRequest, TResponse>(
        object behaviorObj,
        RequestHandlerDelegate<TResponse> next,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : notnull
        where TResponse : notnull
    {
        IPipelineBehavior<TRequest, TResponse> behavior = (IPipelineBehavior<TRequest, TResponse>)behaviorObj;
        return () => behavior.Handle(request, cancellationToken, next);
    }

    #endregion

    #region Pipeline to Command with no returns

    /// <summary>
    /// Maneja comandos sin retorno con pipeline.
    /// </summary>
    private Task<Result> InvokeWithPipeline(ICommand command, CancellationToken cancellationToken)
    {
        Type requestType = command.GetType();
        Type handlerInterface = typeof(ICommandHandler<>).MakeGenericType(requestType);
        object handler = _serviceProvider.GetRequiredService(handlerInterface);

        MethodInfo? method = handlerInterface.GetMethod("Handle");
        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        RequestHandlerDelegate handlerDelegate = () =>
        {
            object task = method.Invoke(handler, new object[] { command, cancellationToken })!;
            return (Task<Result>)task;
        };

        Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(Result));
        IEnumerable<object> behaviors = _serviceProvider
            .GetServices(behaviorType)
            .Cast<object>()
            .Reverse();

        foreach (var behaviorObj in behaviors)
            handlerDelegate = WrapWithBehavior(command, handlerDelegate, behaviorObj, cancellationToken);

        return handlerDelegate();
    }

    private static RequestHandlerDelegate WrapWithBehavior(
        ICommand request,
        RequestHandlerDelegate next,
        object behaviorObj,
        CancellationToken cancellationToken)
    {
        Type behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(request.GetType());
        MethodInfo? handleMethod = behaviorType.GetMethod("Handle");

        return () => (Task<Result>)handleMethod!.Invoke(behaviorObj, new object[] { request, cancellationToken, next })!;
    }


    #endregion
}
