using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Delete;

internal sealed class DeleteCommandHandler : ICommandHandler<DeleteCommand>
{
    public async Task<Result> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
        return Result.Success();
    }
}