using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.GetById;

public sealed record GetByIdQuery(Guid Id) : IQuery<GetByIdResponse>;
