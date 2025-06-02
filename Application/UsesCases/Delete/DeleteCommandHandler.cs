using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Delete;

internal sealed class DeleteCommandHandler : ICommandHandler<DeleteCommand>
{
    public async Task<Result> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        Result result = Result.Success();
        return await Task.FromResult(result);
    }
}