# OCB.Mediator.Helper

A .NET library implementing CQRS (Command Query Responsibility Segregation) pattern with the mediator pattern for clean architecture applications.

## Installation

```bash
dotnet add package OCB.Mediator.Helper
```

## Quick Start

### 1. Register Services

In your `Program.cs`, register the mediator services: [1](#1-0) 

```csharp
builder.Services.AddApplicationServices();
```

This extension method automatically registers:
- The `ISender` interface and implementation [2](#1-1) 
- All command and query handlers from your assembly [3](#1-2) 
- Pipeline behaviors for cross-cutting concerns

### 2. Create Commands and Queries

Define your commands and queries by implementing the provided interfaces:

```csharp
// Query example
public record GetByIdQuery(Guid Id) : IQuery<GetByIdResponse>;

// Command example  
public record CreateCommand(string Name, string Description) : ICommand<Guid>;
```

### 3. Implement Handlers

Create handlers for your commands and queries:

```csharp
public class GetByIdHandler : IQueryHandler<GetByIdQuery, GetByIdResponse>
{
    public async Task<Result<GetByIdResponse>> Handle(GetByIdQuery query, CancellationToken cancellationToken)
    {
        // Your logic here
        return Result.Success(new GetByIdResponse());
    }
}
```

### 4. Use in Controllers/Endpoints

Inject `ISender` and send your commands/queries: [4](#1-3) 

```csharp
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
```

## Pipeline Behaviors

Add cross-cutting concerns using pipeline behaviors: [5](#1-4) 

```csharp
services.AddPipelineBehavior(typeof(ValidationPipelineBehavior<,>));
services.AddPipelineBehavior(typeof(LoggerPipelineBehavior<,>));
```

## Result Pattern

The library uses a `Result<T>` pattern for consistent error handling: [6](#1-5) 

```csharp
// Success
return Result.Success(value);

// Failure
return Result.Failure<T>(error);

// Check result
if (result.IsSuccess)
{
    var value = result.Value;
}
else
{
    var error = result.Error;
}
```

## Features

- **CQRS Implementation**: Separate commands and queries with dedicated handlers
- **Pipeline Behaviors**: Add validation, logging, and other cross-cutting concerns
- **Result Pattern**: Consistent error handling across all operations
- **Dependency Injection**: Automatic handler registration via assembly scanning
- **Cancellation Support**: Built-in `CancellationToken` support

## Notes

The library targets .NET 9.0 and automatically scans assemblies for handlers using Scrutor. All handlers are registered with scoped lifetime, and pipeline behaviors execute in registration order around the actual handler execution.

Wiki pages you might want to explore:
- [Architecture and Design Patterns (ocb-dev-04/ocb.mediator.helper)](/wiki/ocb-dev-04/ocb.mediator.helper#3)
- [Development and Deployment (ocb-dev-04/ocb.mediator.helper)](/wiki/ocb-dev-04/ocb.mediator.helper#6)