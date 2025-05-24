using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Create;

internal sealed class CreateCommandHandler : ICommandHandler<CreateCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        Guid id = Guid.NewGuid();
        return await Task.FromResult(id);
    }
}