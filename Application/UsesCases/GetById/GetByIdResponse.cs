namespace Application.UsesCases.GetById;

/// <summary>
/// Represents the response returned when retrieving an entity by its unique identifier.
/// </summary>
/// <remarks>This record encapsulates the details of an entity, including its identifier, name, description,  and
/// timestamps for creation and last update. It is typically used in scenarios where an entity  is fetched from a data
/// source by its ID.</remarks>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public sealed record GetByIdResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);