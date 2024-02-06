using Shared.Common.Helper.ErrorsHandler;

namespace MediatR.Cqrs.Helper.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> 
    : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>
{
}