using Application;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Application.UsesCases.Create;
using Application.UsesCases.GetById;
using Shared.Common.Helper.ErrorsHandler;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using CQRS.MediatR.Helper.Abstractions.Sender;

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

//app.MapPost("todos", async (
//    CreateRequest request,
//    [FromServices] ICommandHandler<CreateCommand, Guid> commandHandler,
//    CancellationToken cancellationToken) =>
//{
//    CreateCommand command = new(request.Name);
//    Result<Guid> result = await commandHandler.Handle(command, cancellationToken);
    
//    return result.IsSuccess 
//        ? Results.Created($"/todos/{result.Value}", result.Value) 
//        : Results.BadRequest(result.Error.Description);
//});

app.MapPost("todos", async (
    CreateRequest request,
    [FromServices] ISender sender,
    CancellationToken cancellationToken) =>
{
    CreateCommand command = new(request.Name);
    Result<Guid> result = await sender.Send(command, cancellationToken);
    
    return result.IsSuccess 
        ? Results.Created($"/todos/{result.Value}", result.Value) 
        : Results.BadRequest(result.Error.Description);
});

app.Run();

public record CreateRequest(string Name, string Description);