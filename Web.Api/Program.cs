using Application;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Application.UsesCases.Create;
using Application.UsesCases.Delete;
using Application.UsesCases.GetById;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Sender;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddApplicationServices();


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("todos", async (
    Guid Id,
    [FromServices] ISender sender,
    CancellationToken cancellationToken) =>
{
    GetByIdQuery query = new GetByIdQuery(Id);
    Result<GetByIdResponse> result = await sender.Send(query, cancellationToken);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound();
});

app.MapPost("todos", async (
    CreateRequest request,
    [FromServices] ISender sender,
    CancellationToken cancellationToken) =>
{
    CreateCommand command = new(request.Name, request.Description);
    Result<Guid> result = await sender.Send(command, cancellationToken);
    
    return result.IsSuccess 
        ? Results.Created($"/todos/{result.Value}", result.Value) 
        : Results.BadRequest(result.Error.Description);
});

app.MapDelete("todos", async (
    Guid id,
    [FromServices] ISender sender,
    CancellationToken cancellationToken) =>
{
    DeleteCommand command = new(id);
    Result result = await sender.Send(command, cancellationToken);
    
    return result.IsSuccess 
        ? Results.Ok()
        : Results.BadRequest(result.Error.Description);
});

app.Run();

/// <summary>
/// Represents a request to create a new entity with a name and description.
/// </summary>
/// <remarks>This record is typically used to encapsulate the data required for creating a new entity. Both <see
/// cref="Name"/> and <see cref="Description"/> are required to provide meaningful context for the entity being
/// created.</remarks>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record CreateRequest(string Name, string Description);