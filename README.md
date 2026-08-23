# Financial Planner

## Live Demo
The full application is available here: https://fin.ubivator.tech/

The backend portion of a personal finance management application, built with ASP.NET Core. This project is developed with a strong emphasis on clean code, scalability, and a Clean Architecture layout organized around vertical feature slices.

## Tech Stack
* **Framework:** .NET 10.0 / ASP.NET Core Web API
* **Database / ORM:** Entity Framework Core, PostgreSQL (Npgsql)
* **Authentication:** JWT (Access & Refresh tokens), BCrypt
* **Tools & Libraries:** Mapster, FluentValidation, Serilog, Swashbuckle (Swagger), Scrutor
* **Testing:** xUnit, Moq, FluentAssertions, EF Core In-Memory

## Architectural Decisions & Patterns

The backend is split into four projects — `Domain`, `Application`, `Infrastructure`, `Api` — following Clean Architecture dependency rules. Inside `Application`, code is organized by feature (`Features/Transactions`, `Features/Aims`, etc.), each with its own `Commands`/`Queries` and a dedicated handler class, rather than a shared service per entity.

* **Result Pattern for Domain Logic**
  Instead of throwing exceptions to manage control flow during business logic errors, handlers return a `Result` / `Result<T>` (`Domain/Common`) carrying a typed `Error` (`NotFound`, `Validation`, `Conflict`, `Unauthorized`, `Forbidden`, ...). `BaseApiController` translates the error type into the matching HTTP status and a `ProblemDetails` response, so failures never rely on exceptions for control flow.

* **Vertical Slice Commands & Queries**
  Each use case lives in its own folder under `Application/Features/<Feature>/{Commands,Queries}/<UseCase>` with a `Command`/`Query` record, a `...Handler` class, and (for commands) a `FluentValidation` validator. Handlers are plain classes — there's no mediator library; they're discovered and registered automatically by scanning the assembly for types ending in `Handler` (`Infrastructure/DependencyInjection.cs`, via `Scrutor`'s `services.Scan`) and are injected directly into controllers.

* **Domain Services Isolation**
  Complex business logic that goes beyond simple CRUD operations is isolated into dedicated components within the `Domain` layer (`Domain/Services`) — e.g. `BalanceManager` for applying transactions to source balances and `AimProgressCalculator` for aim progress — keeping application handlers thin and focused.

* **Global Exception Handling**
  Implemented using the modern `IExceptionHandler` interface (`Api/Utils/GlobalExceptionHandler.cs`). Unhandled exceptions are caught in a single central location, logged via Serilog, and return a standardized 500 `ProblemDetails` response to the client.

* **User Context Encapsulation**
  An `ICurrentUserContext` abstraction (`Application/Abstractions`, implemented in `Api/Security/CurrentUserContext.cs`) exposes the authenticated user's id. Application-layer code depends only on this abstraction, not on `HttpContext`, keeping handlers decoupled and unit-testable.

* **Mapping & Validation**
  `Mapster` handles mapping between entities and DTOs (including a custom `IPatchMapper` for partial updates). Input validation is handled by `FluentValidation` validators that are invoked explicitly at the start of each command handler, returning a `ValidationError` result rather than throwing or relying on pipeline middleware.

## Testing

The project includes a suite of unit tests (the `Application.Tests` project, mirroring the `Features` folder structure) to ensure reliability and prevent regressions.
* Command/query handlers and domain logic are tested in isolation using `Moq`.
* Database interaction logic is verified using the EF Core In-Memory Database provider.
* `FluentAssertions` is used to ensure test assertions are highly readable and expressive.
