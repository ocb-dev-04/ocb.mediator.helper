using Shared.Common.Helper.ErrorsHandler;

namespace MediatR.Cqrs.Helper.Abstractions.Messaging;

/// <summary>
/// a <see cref="IQueryHandler{TQuery, TResponse}"/> to define and  handle queries that return an object of type <typeparamref name="TResponse"/> 
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IQueryHandler<TQuery, TResponse> 
    : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>
{
}