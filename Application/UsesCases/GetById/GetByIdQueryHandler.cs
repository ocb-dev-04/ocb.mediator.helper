using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.GetById;

/// <summary>
/// Handles queries to retrieve an entity by its unique identifier.
/// </summary>
/// <remarks>This handler processes a <see cref="GetByIdQuery"/> and returns a <see cref="GetByIdResponse"/> 
/// containing the entity's details. The response includes the entity's ID, name, description,  creation date, and last
/// updated date.</remarks>
internal sealed class GetByIdQueryHandler 
    : IQueryHandler<GetByIdQuery, GetByIdResponse>
{
    /// <summary>
    /// Handles the specified query to retrieve an item by its identifier.
    /// </summary>
    /// <param name="request">The query containing the identifier of the item to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/> wrapping a
    /// <see cref="GetByIdResponse"/> with the details of the requested item.</returns>
    public async Task<Result<GetByIdResponse>> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        GetByIdResponse response = new(
            request.Id,
            "Sample Name",
            "Sample Description",
            DateTime.Now.AddDays(-23),
            DateTime.Now);

        return await Task.FromResult(response);
    }
}