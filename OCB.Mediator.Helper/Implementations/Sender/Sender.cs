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
/// commands. Requests route themselves back into this class through <see cref="ISenderDispatcher"/> (double
/// dispatch), which recovers their concrete type at compile time. The whole dispatch path is therefore fully
/// generic: no reflection, no boxing, and compatible with AOT/trimming.</remarks>
public sealed class Sender : ISender, ISenderDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Sender(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Accept(this, cancellationToken);
    }

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return command.Accept(this, cancellationToken);
    }

    Task<Result<TResponse>> ISenderDispatcher.DispatchQuery<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken)
    {
        IQueryHandler<TQuery, TResponse> handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();

        return ExecutePipeline(query, () => handler.Handle(query, cancellationToken), cancellationToken);
    }

    Task<Result<TResponse>> ISenderDispatcher.DispatchCommand<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
    {
        ICommandHandler<TCommand, TResponse> handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();

        return ExecutePipeline(command, () => handler.Handle(command, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Wraps the handler invocation with the registered <see cref="IRequestPipelineBehavior{TRequest, TResponse}"/>
    /// instances, preserving registration order (first registered runs outermost).
    /// </summary>
    private Task<Result<TResponse>> ExecutePipeline<TRequest, TResponse>(
        TRequest request,
        RequestHandlerDelegate<TResponse> handlerDelegate,
        CancellationToken cancellationToken)
            where TRequest : notnull
    {
        foreach (IRequestPipelineBehavior<TRequest, TResponse> behavior in _serviceProvider
            .GetServices<IRequestPipelineBehavior<TRequest, TResponse>>()
            .Reverse())
        {
            RequestHandlerDelegate<TResponse> next = handlerDelegate;
            handlerDelegate = () => behavior.Handle(request, cancellationToken, next);
        }

        return handlerDelegate();
    }
}
