# OCB.Mediator.Helper — Usage Guide

Custom lightweight mediator for CQRS. No MediatR dependency. Built on FluentValidation, Polly, and Scrutor.

---

## 1. DI Registration

```csharp
// Register mediator (auto-scans handlers via Scrutor)
services.AddMediatorHelperServices(typeof(YourAssemblyMarker).Assembly);

// Register FluentValidation validators
services.AddValidators(typeof(YourAssemblyMarker).Assembly, includeInternalTypes: true);

// Register request pipeline behaviors (order = execution order)
services
    .AddRequestPipelineBehavior(typeof(LoggerPipelineBehavior<,>))
    .AddRequestPipelineBehavior(typeof(ValidationPipelineBehavior<,>)); // built-in

// Register notification pipeline behaviors
services.AddNotificationPipelineBehavior(typeof(ExceptionHandlingNotificationPipelineBehavior<>));
```

`AddMediatorHelperServices` registers `ISender`, `INotificationDispatcher`, and all `ICommandHandler<,>`, `IQueryHandler<,>`, `INotificationHandler<>` found in the assembly (scoped, including internal types).

Multiple assemblies: `AddMediatorHelperServices(assembly1, assembly2)`.

---

## 2. Commands

```csharp
// Define
sealed record CreateItemCommand(string Name, string Description) : ICommand<Guid>;

// Void return — use Unit
sealed record DeleteItemCommand(Guid Id) : ICommand<Unit>;

// Idempotent command — no RequestId on the record itself
sealed record ProcessPaymentCommand(decimal Amount)
    : IdempotentCommand<bool>();
```

### IdempotentCommand and RequestId

`IdempotentCommand<TResponse>` does **not** carry a `RequestId` property. The recommended approach is to resolve the idempotency key via a **scoped `RequiredHeaderProvider`** — a service registered per-request that reads the idempotency header (e.g., `Idempotency-Key`) once and exposes it as a `Guid`.

```csharp
// Provider interface
public interface IIdempotencyKeyProvider
{
    Guid RequestId { get; }
}

// Implementation — reads the header on first access (scoped)
internal sealed class IdempotencyKeyProvider : IIdempotencyKeyProvider
{
    private readonly IHttpContextAccessor _http;

    public IdempotencyKeyProvider(IHttpContextAccessor http) => _http = http;

    public Guid RequestId
    {
        get
        {
            var raw = _http.HttpContext?.Request.Headers["Idempotency-Key"].FirstOrDefault();
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }
}
```

```csharp
// Register as scoped so each request gets its own instance
services.AddHttpContextAccessor();
services.AddScoped<IIdempotencyKeyProvider, IdempotencyKeyProvider>();
```

Then inject it inside the `IdempotencyPipelineBehavior` instead of relying on a property on the command:

```csharp
internal sealed class IdempotencyPipelineBehavior<TRequest, TResponse>
    : IRequestPipelineBehavior<TRequest, TResponse>
    where TRequest : IdempotentCommand<TResponse>
{
    private readonly IIdempotencyKeyProvider _keyProvider;
    private readonly IIdempotencyStore _store;          // your store abstraction

    public IdempotencyPipelineBehavior(
        IIdempotencyKeyProvider keyProvider,
        IIdempotencyStore store)
    {
        _keyProvider = keyProvider;
        _store = store;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        var requestId = _keyProvider.RequestId;

        if (await _store.ExistsAsync(requestId, ct))
            return await _store.GetAsync<TResponse>(requestId, ct);

        var result = await next();

        if (result.IsSuccess)
            await _store.SaveAsync(requestId, result, ct);

        return result;
    }
}
```

Register the behavior with the request pipeline:

```csharp
services.AddRequestPipelineBehavior(typeof(IdempotencyPipelineBehavior<,>));
```

```csharp
// Implement handler (can be internal sealed)
internal sealed class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, Guid>
{
    public Task<Result<Guid>> Handle(CreateItemCommand command, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        return Task.FromResult(Result.Success(id));
    }
}

// Void-return handler
internal sealed class DeleteItemCommandHandler : ICommandHandler<DeleteItemCommand, Unit>
{
    public Task<Result<Unit>> Handle(DeleteItemCommand command, CancellationToken ct)
        => Task.FromResult(Result.Success(Unit.Value));
}
```

---

## 3. Queries

```csharp
// Define
sealed record GetItemQuery(Guid Id) : IQuery<ItemResponse>;
sealed record ItemResponse(Guid Id, string Name);

// Implement handler
internal sealed class GetItemQueryHandler : IQueryHandler<GetItemQuery, ItemResponse>
{
    public Task<Result<ItemResponse>> Handle(GetItemQuery query, CancellationToken ct)
    {
        var response = new ItemResponse(query.Id, "Sample");
        return Task.FromResult(Result.Success(response));
    }
}
```

---

## 4. Result Pattern

```csharp
// Result<T> — success
Result<Guid> ok = Result.Success(Guid.NewGuid());

// Result<T> — failure
Result<Guid> fail = Result.Failure<Guid>(Error.NotFound());

// Result (no value)
Result done = Result.Success();
Result err  = Result.Failure(Error.BadRequest());

// Nullable value — Success if not null, Failure<NullValue> if null
Result<string> r = Result.Create(maybeNullString);

// Read value (throws if failure)
Guid id = ok.Value;
bool isOk = ok.IsSuccess;
bool isFail = ok.IsFailure;
Error e = fail.Error;
```

### Error factory methods

| Method | StatusCode |
|--------|-----------|
| `Error.None` | — (no error sentinel) |
| `Error.NullValue` | — (null-value sentinel) |
| `Error.NotModified()` | 304 |
| `Error.BadRequest()` | 400 |
| `Error.Unauthorized()` | 401 |
| `Error.NotFound()` | 404 |
| `Error.TooManyRequest()` | 429 |
| `Error.InternalServerError()` | 500 |
| `Error.Exception()` | 500 |

Custom error: `new Error(StatusCode, "TRANSLATION_KEY", "Human description")`.

### Match extension

```csharp
// Result<T>
IActionResult result = await _sender.Send(query, ct);
return result.Match(
    success: value => Ok(value),
    error:   err   => HandleErrorResults(err)
);

// Result (no value)
IActionResult result = await _sender.Send(command, ct);
return result.Match(
    success: () => Ok(),
    error:   err => HandleErrorResults(err)
);
```

---

## 5. Validation

Create a FluentValidation validator for each command/query. Auto-registered via `AddValidators`.

```csharp
internal sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
    }
}
```

The built-in `ValidationPipelineBehavior<,>` runs all validators in parallel before the handler. On failure it throws `ValidationException` with a `IDictionary<string, string[]> Errors` (property name → messages). Register it as a request pipeline behavior (see section 1).

---

## 6. Pipeline Behaviors

### Request pipeline

Executes in registration order around every command/query handler.

```csharp
internal sealed class LoggerPipelineBehavior<TRequest, TResponse>
    : IRequestPipelineBehavior<TRequest, TResponse>
{
    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        var sw = Stopwatch.StartNew();
        var result = await next();
        _logger.LogInformation("{Request} completed in {Ms}ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
        return result;
    }
}
```

`RequestHandlerDelegate<TResponse>` is `Func<Task<Result<TResponse>>>`.

### Notification pipeline

Executes around every notification dispatch.

```csharp
internal sealed class ExceptionHandlingNotificationPipelineBehavior<TNotification>
    : INotificationPipelineBehavior<TNotification>
    where TNotification : INotification
{
    public async Task HandleAsync(TNotification notification, Func<Task> next, CancellationToken ct)
    {
        try { await next(); }
        catch (Exception ex) { _logger.LogError(ex, "Notification error"); }
    }
}
```

---

## 7. Notifications (Events)

```csharp
// Define event
sealed record ItemCreatedEvent(Guid Id, string Name) : INotification;

// Implement handler (can be internal)
internal sealed class ItemCreatedEventHandler : INotificationHandler<ItemCreatedEvent>
{
    public Task HandleAsync(ItemCreatedEvent notification, CancellationToken ct)
    {
        // handle the event
        return Task.CompletedTask;
    }
}
```

```csharp
// Dispatch from a command handler
public class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, Guid>
{
    private readonly INotificationDispatcher _dispatcher;

    public async Task<Result<Guid>> Handle(CreateItemCommand cmd, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        // With notification pipeline behaviors + Polly resilience
        await _dispatcher.DispatchAsync(new ItemCreatedEvent(id, cmd.Name), ct);

        // Bypasses pipeline behaviors (use for error/exception events to avoid infinite loops)
        // await _dispatcher.UnhandledDispatchAsync(new ItemCreatedEvent(id, cmd.Name), ct);

        return Result.Success(id);
    }
}
```

All `INotificationHandler<T>` implementations for the same event run concurrently (`Task.WhenAll`). Each dispatch creates a new DI scope.

---

## 8. Built-in Polly Resilience (Notifications only)

`INotificationDispatcher` applies this policy stack automatically on every dispatch:

| Policy | Config |
|--------|--------|
| Timeout | 30s pessimistic |
| Bulkhead | 100 parallel, 50 queued |
| Circuit Breaker | opens after 5 exceptions, breaks 30s |
| Retry | 3 attempts, exponential backoff (250ms × 2^attempt) |

No configuration required. Commands and queries do **not** have built-in Polly policies.

---

## 9. Controller Integration

Inherit from `BaseController` to get `ISender` and `HandleErrorResults`.

```csharp
[ApiController]
[Route("api/items")]
public class ItemsController : BaseController
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetItemQuery(id), ct);
        return result.Match(Ok, HandleErrorResults);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.Match(value => Created(string.Empty, value), HandleErrorResults);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteItemCommand(id), ct);
        return result.Match(Ok, HandleErrorResults);
    }
}
```

`HandleErrorResults(Error)` maps `Error.StatusCode` → HTTP response:

| StatusCode | Response |
|-----------|----------|
| 304 | `StatusCode(304, { translation, description })` |
| 400 | `BadRequest({ translation, description })` |
| 401 | `Unauthorized()` |
| 404 | `NotFound({ translation, description })` |
| other | `StatusCode(n)` |

---

## 10. Folder Convention

```
Application/
  UsesCases/
    Create/
      CreateCommand.cs            // : ICommand<TResponse>
      CreateCommandHandler.cs     // : ICommandHandler<,>
      CreateCommandValidator.cs   // : AbstractValidator<>
    GetById/
      GetByIdQuery.cs             // : IQuery<TResponse>
      GetByIdQueryHandler.cs      // : IQueryHandler<,>
      GetByIdQueryValidator.cs
      GetByIdResponse.cs          // sealed record
  Events/
    ItemCreatedEvent.cs           // : INotification
  EventsHandlers/
    ItemCreatedEventHandler.cs    // : INotificationHandler<>
  Behaviours/
    HandlerPipelines/
      LoggerPipelineBehavior.cs
    EventPipelines/
      ExceptionHandlingNotificationPipelineBehavior.cs
  ApplicationServices.cs          // IServiceCollection extensions
```
