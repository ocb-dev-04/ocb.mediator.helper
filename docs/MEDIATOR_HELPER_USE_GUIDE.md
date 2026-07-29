# OCB.Mediator.Helper — Usage Guide

Custom lightweight mediator for CQRS. No MediatR dependency. Built on FluentValidation, Polly, and Scrutor. Targets **.NET 10** (`net10.0`).

Dispatch is **reflection-free**: requests inherit from `CommandBase<TSelf, TResponse>` / `QueryBase<TSelf, TResponse>`, which recover the concrete type at compile time (double dispatch). No `MethodInfo.Invoke`, no boxing, AOT/trimming friendly.

---

## ⚠️ Breaking changes (v2)

| Before (1.x) | Now (2.x) |
|---|---|
| `record X(...) : ICommand<T>;` | `record X(...) : CommandBase<X, T>;` |
| `record X(...) : IQuery<T>;` | `record X(...) : QueryBase<X, T>;` |
| `record X(...) : IdempotentCommand<T>;` | `record X(...) : IdempotentCommand<X, T>;` |
| Validation failure **throws** `ValidationException` | Validation failure **returns** `Result.Failure` with a `ValidationError` (no exception). `ValidationException` is `[Obsolete]` and will be removed. |
| `Match(Func<object, T> success, ...)` | `Match(Func<TValue, T> success, ...)` — the success callback receives the typed value (no boxing). |
| `INotificationDispatcher` was scoped; retry wrapped **all** handlers together | Singleton; the resilience policy wraps **each handler individually** (a retry never re-executes handlers that already succeeded). |
| Notification timeout was pessimistic | Timeout is **optimistic**: notification handlers must honor the `CancellationToken` they receive, or the 30s timeout will not fire. |
| `Sender` was registered as itself + `ISender` | Only `ISender` is registered. Inject `ISender`, never the concrete `Sender`. |
| `ErrorHandler.*`, `ValidationResults.*`, `IValidationResult`, `RequestResultStatus` | Removed (dead code). |
| Depended on `Microsoft.AspNetCore.Http.Abstractions` | Dependency removed — `Error` uses plain int status codes. |
| Targeted `net9.0` | Targets `net10.0` — consuming projects must be on .NET 10. |

`ISender.Send(...)` call sites do **not** change — only the request type declarations do.

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

`AddMediatorHelperServices` registers:

- `ISender` — **scoped**. Handlers (`ICommandHandler<,>`, `IQueryHandler<,>`, `INotificationHandler<>`) — **scoped**, registered against their handler interfaces only (other interfaces a handler implements are not registered).
- `INotificationDispatcher` — **singleton**, so its Polly circuit breaker and bulkhead accumulate state across the whole application. It creates its own DI scope per dispatch to resolve handlers.

Multiple assemblies (`params`): `AddMediatorHelperServices(assembly1, assembly2)`.

---

## 2. Commands

Commands inherit from `CommandBase<TSelf, TResponse>`, where `TSelf` is the command's own type:

```csharp
// Define
sealed record CreateItemCommand(string Name, string Description)
    : CommandBase<CreateItemCommand, Guid>;

// Void return — use Unit
sealed record DeleteItemCommand(Guid Id)
    : CommandBase<DeleteItemCommand, Unit>;

// Idempotent command — no RequestId on the record itself
sealed record ProcessPaymentCommand(decimal Amount)
    : IdempotentCommand<ProcessPaymentCommand, bool>;
```

> **Why `TSelf`?** The base record implements the dispatch plumbing generically (curiously recurring template pattern), which is what makes the mediator reflection-free. Passing the wrong type as `TSelf` (e.g. `CommandBase<OtherCommand, Guid>`) fails at compile time in normal usage.

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

### IdempotentCommand and RequestId

`IdempotentCommand<TSelf, TResponse>` is a marker base — it does **not** implement idempotency by itself; pair it with a pipeline behavior. It does not carry a `RequestId` property: the recommended approach is to resolve the idempotency key via a **scoped provider** that reads the idempotency header (e.g., `Idempotency-Key`) once and exposes it as a `Guid`.

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

Then inject it inside the `IdempotencyPipelineBehavior` instead of relying on a property on the command. Note the CRTP constraint:

```csharp
internal sealed class IdempotencyPipelineBehavior<TRequest, TResponse>
    : IRequestPipelineBehavior<TRequest, TResponse>
    where TRequest : IdempotentCommand<TRequest, TResponse>
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

Register the behavior with the request pipeline (the DI container only applies it to requests that satisfy the constraint):

```csharp
services.AddRequestPipelineBehavior(typeof(IdempotencyPipelineBehavior<,>));
```

---

## 3. Queries

Queries inherit from `QueryBase<TSelf, TResponse>`:

```csharp
// Define
sealed record GetItemQuery(Guid Id) : QueryBase<GetItemQuery, ItemResponse>;
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
| `new ValidationError(errors)` | 400, carries `IReadOnlyDictionary<string, string[]> Errors` |

Custom error: `new Error(StatusCode, "TRANSLATION_KEY", "Human description")`.

### Match extension

The success callback receives the **typed** value (`Func<TValue, TReturn>`) — no boxing, no `object`:

```csharp
// Result<T>
Result<ItemResponse> result = await _sender.Send(query, ct);
return result.Match(
    success: value => Ok(value),          // value is ItemResponse
    error:   err   => HandleErrorResults(err)
);

// Result (no value)
Result result = await _sender.Send(command, ct);
return result.Match(
    success: () => Ok(),
    error:   err => HandleErrorResults(err)
);
```

---

## 4a. Optional Pattern

`Optional<T>` explicitly represents whether a response **section** carries a value, replacing a
semantic `null` (which forces every consumer to guess "not provided" vs. "intentionally empty" vs.
"hidden"). It mirrors the `Result` pattern's API shape: `Optional.Some(...)` / `Optional.None()`,
never a public constructor.

```csharp
// Wrapping a value
Optional<BirthInfo> present = Optional.Some(birthInfo);   // non-generic factory, T inferred
Optional<BirthInfo> present2 = Optional<BirthInfo>.Some(birthInfo); // generic factory, explicit T

// Absence of a value — Optional.None() infers T from the assignment target
Optional<BirthInfo> absent = Optional.None();
Optional<BirthInfo> absent2 = Optional<BirthInfo>.None();

// Implicit conversion from T
Optional<BirthInfo> implicitPresent = birthInfo;

// Read — HasValue is always the source of truth; never inspect Value first
if (present.HasValue)
{
    BirthInfo value = present.Value; // NotNull when HasValue is true
}
```

`Optional.Some(null)` throws `ArgumentNullException` — an "empty section" must always be `None()`,
never a `Some` wrapping a null.

### Where to use it

Use `Optional<T>` for **optional response sections** — complex nested objects on a `*Response`/`*Dto`
whose absence has business meaning (`BirthInfo`, `Address`, `EmergencyContact`, `Employment`…).

Do **not** use it for primitive nullable properties (`string?`, `int?`, `DateTime?`, `decimal?`,
`bool?`) that don't represent a whole business section — those keep their existing nullable
semantics.

### Serialization

`Optional<T>` has no custom `System.Text.Json` converter — its two public properties (`HasValue`,
`Value`) serialize with whatever `JsonSerializerOptions` the consuming API already uses (e.g.
camelCase policy), producing:

```json
{ "hasValue": true, "value": { } }
{ "hasValue": false, "value": null }
```

Because the constructor is private, deserializing `Optional<T>` back from JSON (System.Text.Json or
Newtonsoft) requires a custom converter in the consuming project — this package intentionally ships
none, so it has zero serializer dependencies. Add the converter where the response is actually
(de)serialized (e.g. the API's JSON options, or a cache serialization layer), not here.

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

The built-in `ValidationPipelineBehavior<,>` runs all validators in parallel before the handler. On failure it **returns** `Result.Failure<TResponse>` carrying a `ValidationError` — it does **not** throw. `ValidationError.Errors` is an `IReadOnlyDictionary<string, string[]>` (camelCase property name → distinct messages). The handler is never invoked when validation fails. Register it as a request pipeline behavior (see section 1).

```csharp
// Typical HTTP mapping (see BaseController below)
if (result.Error is ValidationError validation)
    return BadRequest(new { validation.Translation, validation.Description, validation.Errors });
```

Example 400 response body:

```json
{
  "translation": "validationError",
  "description": "One or more validation errors occurred",
  "errors": { "name": ["Name is required"], "description": ["Description is required"] }
}
```

> `ValidationException` still exists but is `[Obsolete]` and is no longer thrown by the library. If you had exception-handling middleware catching it, replace it with `ValidationError` handling at the result level.

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
        var start = Stopwatch.GetTimestamp();
        var result = await next();
        _logger.LogInformation("{Request} completed in {Ms}ms",
            typeof(TRequest).Name, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
        return result;
    }
}
```

`RequestHandlerDelegate<TResponse>` is `Func<Task<Result<TResponse>>>`.

### Notification pipeline

Executes around every notification dispatch (around the *whole* handler group, **outside** the per-handler resilience policy).

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
        // handle the event — honor ct: the 30s timeout policy is optimistic (cancellation-based)
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

        // With notification pipeline behaviors + per-handler Polly resilience
        // (signature: DispatchAsync(notification, bool useRetry = true, CancellationToken ct = default))
        await _dispatcher.DispatchAsync(new ItemCreatedEvent(id, cmd.Name), cancellationToken: ct);

        // Bypasses pipeline behaviors (use for error/exception events to avoid infinite loops)
        // await _dispatcher.UnhandledDispatchAsync(new ItemCreatedEvent(id, cmd.Name), cancellationToken: ct);

        return Result.Success(id);
    }
}
```

All `INotificationHandler<T>` implementations for the same event run concurrently (`Task.WhenAll`). Each dispatch creates a new DI scope.

> **Dispatch with the concrete type.** Handlers are resolved by the static generic type of the notification. Dispatching through an `INotification`-typed variable resolves zero handlers — always pass the concrete event type (the normal case with `new SomeEvent(...)`).

---

## 8. Built-in Polly Resilience (Notifications only)

`INotificationDispatcher` is a **singleton**, so these policies keep their state across the whole application (a scoped circuit breaker would never accumulate failures). The policy stack wraps **each handler individually** — if one handler out of three fails, only that one is retried; the others are never re-executed.

| Policy | Config |
|--------|--------|
| Timeout | 30s **optimistic** — handlers must honor their `CancellationToken` |
| Bulkhead | 100 parallel handler executions, 50 queued (global, per app) |
| Circuit Breaker | opens after 5 exceptions, breaks 30s |
| Retry | 3 attempts, exponential backoff (250ms × 2^attempt) |

No configuration required. Pass `useRetry: false` to skip the policy stack for a dispatch. Notification pipeline behaviors run **outside** the resilience policy (they execute once per dispatch, not per retry). Commands and queries do **not** have built-in Polly policies.

---

## 9. Controller Integration

Inherit from `BaseController` to get `ISender` and `HandleErrorResults`. It only needs `ISender` (no `IHttpContextAccessor`).

```csharp
[ApiController]
[Route("api/items")]
public class ItemsController : BaseController
{
    public ItemsController(ISender sender) : base(sender) { }

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

`HandleErrorResults(Error)` maps the error → HTTP response:

| Error | Response |
|-----------|----------|
| `ValidationError` | `BadRequest({ translation, description, errors })` |
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
      CreateCommand.cs            // : CommandBase<CreateCommand, TResponse>
      CreateCommandHandler.cs     // : ICommandHandler<,>
      CreateCommandValidator.cs   // : AbstractValidator<>
    GetById/
      GetByIdQuery.cs             // : QueryBase<GetByIdQuery, TResponse>
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
