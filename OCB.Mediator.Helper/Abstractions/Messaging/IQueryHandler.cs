using OCB.Mediator.Helper.ResultPattern;

namespace OCB.Mediator.Helper.Abstractions.Messaging;

/// <summary>
/// a <see cref="IQueryHandler{TQuery, TResponse}"/> to define and  handle queries that return an object of type <typeparamref name="TResponse"/> 
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IQueryHandler<in TQuery, TResponse> 
        where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handle the <see cref="TQuery"/> and returns an object of type <typeparamref name="TResponse"/>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<TResponse>> Handle(TQuery request, CancellationToken cancellationToken);
}