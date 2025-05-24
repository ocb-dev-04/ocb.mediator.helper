using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Application.UsesCases.GetById;

public sealed record GetByIdQuery(Guid Id) : IQuery<Result<GetByIdResponse>>;
