using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Application.UsesCases.GetById;

internal sealed class GetByIdQueryHandler : IQueryHandler<GetByIdQuery, GetByIdResponse>
{
    public async Task<Result<GetByIdResponse>> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        GetByIdResponse response = new (
            request.Id, 
            "Sample Name", 
            "Sample Description",
            DateTime.Now.AddDays(-23),
            DateTime.Now);

        return await Task.FromResult(response);
    }
}