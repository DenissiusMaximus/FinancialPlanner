# Architecture overview

This project follows Clean Architecture: four projects arranged so dependencies only point inward — `Api` → `Infrastructure` → `Application` → `Domain`. `Domain` has no dependency on anything else in the solution; it doesn't even reference Entity Framework.

```
src/
  Domain/           entities, Result/Error, repository interfaces, domain services
  Application/      CQRS commands/queries, handlers, validators, DTOs, mapping
  Infrastructure/    EF Core DbContext, repositories, JWT/password implementations, DI wiring
  Api/               controllers, Program.cs, HTTP-specific concerns
tests/
  Application.Tests/ handler and domain-service tests (xUnit + Moq + EF InMemory)
```

## 1. Layers

### Domain (`src/Domain`)
The innermost layer. Contains:
- **Entities** (`Entities/`) — plain classes mapped to the database, no EF attributes or behavior.
- **`Common/Result.cs`, `Common/Error.cs`** — a `Result`/`Result<T>` type used instead of exceptions or nulls for expected failures. Every handler returns a `Result`.
- **`Errors/`** — one static class per aggregate (`AimErrors`, `TransactionErrors`, `UserErrors`, …), each method building a named `Error` with a stable `Code` and an `ErrorType` (`NotFound`, `Validation`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`).
- **`Repositories/`** — repository interfaces and `IUnitOfWork`. Domain defines the contracts; `Infrastructure` implements them. Repositories return entities, never DTOs.
- **`Services/`** — pure domain logic with no I/O: `IBalanceManager` (applies/reverts a transaction's effect on source balances) and `IAimProgressCalculator` (computes aim funding progress across shared sources by priority).

### Application (`src/Application`)
CQRS: every use case is a `Command` or `Query` record plus a matching `...Handler` class with a single `HandleAsync` method returning `Result` or `Result<T>`. Organized by feature under `Features/<Feature>/{Commands,Queries}/<Operation>/`.

A handler's shape is always the same:
1. Validate the command via an injected `IValidator<TCommand>` (FluentValidation); on failure, return `Result.Failure` with a `ValidationError`.
2. Load what's needed through repository interfaces.
3. Apply the change (mutate a tracked entity, or construct a new one) and call `IUnitOfWork.SaveChangesAsync`.
4. Map the result to a DTO with Mapster (`IMapper`, injected) and return it.

`Common/Mapping` holds the Mapster `IRegister` configurations (e.g. flattening `Aim.SourceAims` into a `Sources` list) and the null-ignoring `IPatchMapper` used for PATCH-style updates.

### Infrastructure (`src/Infrastructure`)
Implements everything Domain and Application only declared as interfaces:
- `Database/ApplicationDbContext.cs` + `Database/Configurations/*Configuration.cs` (one `IEntityTypeConfiguration<T>` per entity, applied via `ApplyConfigurationsFromAssembly`).
- `Database/Repositories/` — one repository per aggregate; each owns its own filtering/sorting/paging and eager-loads exactly the navigation properties its callers need.
- `Security/` — `PasswordHasher` (BCrypt) and `JwtProvider` (token issuance and validation; blacklist-checking is orchestrated by the Application handlers that call it, not by the provider itself).
- `DependencyInjection.cs` — a single `AddInfrastructure(configuration)` entry point wiring the DbContext, FluentValidation, Mapster, repositories, domain services, and all Application handlers (the handlers are registered by convention — every class whose name ends in `Handler` — instead of one line per handler).

### Api (`src/Api`)
Thin controllers. Each action builds a command/query, calls its handler, and passes the `Result` to `BaseApiController.HandleResult(...)`, which maps `ErrorType` to an HTTP status and a `ProblemDetails` body in one place. `Security/CurrentUserContext.cs` implements `ICurrentUserContext` over `IHttpContextAccessor` — this is HTTP-specific, so it lives here rather than in Infrastructure.

## 2. Key patterns

### Result instead of exceptions for expected failures
Domain and Application never throw for "not found" or "validation failed" — they return `Result.Failure(SomeErrors.X(...))`. `GlobalExceptionHandler` (in `Api/Utils`) exists only for genuinely unexpected exceptions and always returns a generic 500; it is not part of normal control flow.

### CQRS with explicit handlers
There is no mediator/pipeline library. A controller calls exactly one handler by constructor injection. This keeps the call stack for any given endpoint to two hops (`Controller` → `Handler`) and makes each use case's dependencies explicit in its constructor.

### Repository + Unit of Work
Repositories never call `SaveChanges`; only `IUnitOfWork.SaveChangesAsync` does. This lets a handler compose multiple repository calls (e.g. reverting a transfer's source and destination balances) inside one `IUnitOfWork.BeginTransactionAsync` scope and commit once.

### Domain services stay pure
`BalanceManager` and `AimProgressCalculator` take already-loaded entities as parameters and never touch a `DbContext`. Handlers are responsible for loading data through repositories first. This is what makes them unit-testable without any database.

### Patch mapping
Update commands carry nullable fields for every patchable property. `IPatchMapper.PatchInto(command, entity)` copies only the non-null fields onto the tracked entity, using a Mapster config with `IgnoreNullValues(true)` that is separate from the read-mapping config but scans the same `IRegister` classes.

## 3. Testing

`tests/Application.Tests` exercises handlers directly against EF Core's InMemory provider through the real repository implementations — no mocking of the database. External concerns (`IPasswordHasher`, `IJwtProvider`) are mocked with Moq where a handler's own logic, not the collaborator's, is under test. Domain services (`BalanceManager`, `AimProgressCalculator`) are tested with no infrastructure at all — they're plain constructors and method calls.
