using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.GetById;

/// <summary>
/// Represents a query to retrieve an entity by its unique identifier.
/// </summary>
/// <remarks>This query is used to request data for a specific entity identified by its <see cref="Id"/>. The
/// response type is <see cref="GetByIdResponse"/>, which contains the details of the requested entity.</remarks>
/// <param name="Id">The unique identifier of the entity to retrieve. Must be a valid <see cref="Guid"/>.</param>
public sealed record GetByIdQuery(Guid Id) : QueryBase<GetByIdQuery, GetByIdResponse>;
