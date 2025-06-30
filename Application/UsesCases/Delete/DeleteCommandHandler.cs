using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Messaging;

namespace Application.UsesCases.Delete;

/// <summary>
/// Handles the execution of a delete command.
/// </summary>
/// <remarks>This handler processes a <see cref="DeleteCommand"/> and returns a result indicating the success or
/// failure of the operation. The result is wrapped in a <see cref="Task"/> to support asynchronous execution.</remarks>
internal sealed class DeleteCommandHandler 
    : ICommandHandler<DeleteCommand, Unit>
{
    /// <summary>
    /// Handles the deletion command and returns the result of the operation.
    /// </summary>
    /// <param name="request">The <see cref="DeleteCommand"/> containing the details of the deletion operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.      The result contains a <see
    /// cref="Result{T}"/> object indicating the success or failure of the deletion operation.</returns>
    public async Task<Result<Unit>> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        Result<Unit> result = Result.Success(Unit.Value);

        // Uncomment the following line to simulate a failure case
        //Result<Unit> result = Result.Failure<Unit>(Error.NullValue);

        return await Task.FromResult(result);
    }
}