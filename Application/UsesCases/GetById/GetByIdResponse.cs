namespace Application.UsesCases.GetById;

public sealed record GetByIdResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);