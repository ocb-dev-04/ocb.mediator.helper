using Microsoft.AspNetCore.Mvc;
using Application.UsesCases.Create;
using Application.UsesCases.Delete;
using Application.UsesCases.GetById;
using OCB.Mediator.Helper.Extensions;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Sender;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/todos")]
[Produces("application/json")]
public sealed class TodosController : BaseController
{
    public TodosController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        GetByIdQuery query = new GetByIdQuery(id);
        Result<GetByIdResponse> result = await _sender.Send(query, cancellationToken);

        return result.Match(Ok, HandleErrorResults);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateRequest request,
        CancellationToken cancellationToken)
    {
        CreateCommand command = new(request.Name, request.Description);
        Result<Guid> result = await _sender.Send(command, cancellationToken);

        return result.Match(
            value => Created(string.Empty, value),
            HandleErrorResults);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        DeleteCommand command = new(id);
        Result result = await _sender.Send(command, cancellationToken);

        return result.Match(Ok, HandleErrorResults);
    }
}

/// <summary>
/// Represents a request to create a new entity with a name and description.
/// </summary>
/// <remarks>This record is typically used to encapsulate the data required for creating a new entity. Both <see
/// cref="Name"/> and <see cref="Description"/> are required to provide meaningful context for the entity being
/// created.</remarks>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record CreateRequest(string Name, string Description);