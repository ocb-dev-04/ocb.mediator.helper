using Shared.Common.Helper.ErrorsHandler;

namespace MediatR.Cqrs.Helper.Abstractions.Messaging;

/// <summary>
/// An <see cref="IQuery"/> to use for queries endpoints
/// </summary>
public interface IQuery<TResponse> 
    : IRequest<Result<TResponse>>
{
}