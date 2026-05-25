# Financial Planner

## Live Demo
The full application is available here: https://fin.ubivator.tech/

> Note: the database is hosted on a free tier, so it may go to sleep when idle. If the app/API doesn’t respond on the first try, open it once, wait ~30 seconds, and try again.

The backend portion of a personal finance management application, built with ASP.NET Core. This project is developed with a strong emphasis on clean code, scalability, and the implementation of mod[...]

## Tech Stack
* **Framework:** .NET 9.0 / ASP.NET Core Web API
* **Database / ORM:** Entity Framework Core, SQL Server
* **Authentication:** JWT (Access & Refresh tokens), BCrypt
* **Tools & Libraries:** Mapster, FluentValidation, Serilog, Swashbuckle (Swagger)
* **Testing:** xUnit, Moq, FluentAssertions, EF Core In-Memory

## Architectural Decisions & Patterns

The project is built around an N-tier architecture with a strict Separation of Concerns. The primary focus was to make the codebase highly testable, predictable, and easy to maintain.

* **Notification Pattern for Domain Logic**
  Instead of throwing exceptions to manage control flow during business logic errors, a `NotificationContext` is utilized. All domain validation errors are collected during the request and handled[...]

* **Domain Services Isolation**
  Complex business logic that goes beyond simple CRUD operations is isolated into dedicated components within the `Domain` layer (e.g., `BalanceManagementService` for transaction and balance proce[...]

* **Global Exception Handling**
  Implemented using the modern `IExceptionHandler` interface. All unhandled exceptions are caught in a single central location, safely logged, and return a standardized 500 Internal Server Error t[...]

* **User Context Encapsulation**
  An `ICurrentUserContext` abstraction was created to handle the authenticated user's state. Services do not depend directly on `HttpContext`, making them completely decoupled and suitable for iso[...]

* **Query Extensions for EF Core**
  Filtering, pagination, and sorting logic is encapsulated into extension methods for `IQueryable` (e.g., `FilterByDateRange`, `ApplySorting`). This keeps the application services clean and promot[...]

* **Decorators for Logging**
  The Decorator pattern is implemented (via `Scrutor`) to seamlessly add cross-cutting concerns like logging to services (e.g., `UserLoggingService`, `JwtLoggingService`) without modifying their c[...]

* **Mapping & Validation**
  `Mapster` is used for fast and efficient object mapping between Entities and DTOs. Input validation is handled by `FluentValidation`, which is natively integrated into the ASP.NET Core pipeline [...]

## Testing

The project includes a comprehensive suite of unit tests (the `APITest` project) to ensure reliability and prevent regressions.
* Business services and domain logic are tested in isolation using `Moq`.
* Database interaction logic is verified using the EF Core In-Memory Database provider.
* `FluentAssertions` is used to ensure test assertions are highly readable and expressive.
