using System.Reflection;
using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Sender;
using Microsoft.Extensions.DependencyInjection;
using CQRS.MediatR.Helper.Abstractions.Messaging;

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
    {
        Type handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        object handler = _serviceProvider.GetRequiredService(handlerInterface);

        MethodInfo? method = handlerInterface.GetMethod("Handle");
        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        return (Task<Result<TResponse>>)method.Invoke(handler, new object[] { query, cancellationToken })!;
    }

    /// <inheritdoc/>
    public Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        Type handlerInterface = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        object handler = _serviceProvider.GetRequiredService(handlerInterface);

        MethodInfo? method = handlerInterface.GetMethod("Handle");
        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        return (Task<Result>)method.Invoke(handler, new object[] { command, cancellationToken })!;
    }

    /// <inheritdoc/>
    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        Type handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        object handler = _serviceProvider.GetRequiredService(handlerInterface);

        MethodInfo? method = handlerInterface.GetMethod("Handle");
        if (method is null)
            throw new InvalidOperationException($"Handle method not found in handler: {handlerInterface.FullName}");

        return (Task<Result<TResponse>>)method.Invoke(handler, new object[] { command, cancellationToken })!;
    }
}
