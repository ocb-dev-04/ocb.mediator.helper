using Shared.Common.Helper.ErrorsHandler;

namespace MediatR.Cqrs.Helper.Abstractions.Messaging;

public interface IQuery<TResponse> 
    : IRequest<Result<TResponse>>
{
}