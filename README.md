# Financial Planner

## Live Demo
The full application is available here: https://fin.ubivator.tech/

> Note: the database is hosted on a free tier, so it may go to sleep when idle. If the app/API doesn’t respond on the first try, open it once, wait ~30 seconds, and try again.

The backend portion of a personal finance management application, built with ASP.NET Core. This project is developed with a strong emphasis on clean code, scalability, and a modern layered architecture designed for maintainability, testability, and clear separation of concerns.

## Tech Stack
* **Framework:** .NET 9.0 / ASP.NET Core Web API
* **Database / ORM:** Entity Framework Core, SQL Server
* **Authentication:** JWT (Access & Refresh tokens), BCrypt
* **Tools & Libraries:** Mapster, FluentValidation, Serilog, Swashbuckle (Swagger)
* **Testing:** xUnit, Moq, FluentAssertions, EF Core In-Memory

## Architectural Decisions & Patterns

The backend is built around an N-tier architecture with a strict Separation of Concerns. The primary focus was to make the codebase highly testable, predictable, and easy to maintain.

* **Notification Pattern for Domain Logic**
  Instead of throwing exceptions to manage control flow during business logic errors, a `NotificationContext` is used. Domain validation issues are collected during the request and returned in a structured way, keeping the flow explicit and predictable.

* **Domain Services Isolation**
  Complex business logic that goes beyond simple CRUD operations is isolated into dedicated components within the `Domain` layer (for example, transaction processing and balance management services). This keeps application services thin and responsibilities well defined.

* **Global Exception Handling**
  Unhandled exceptions are processed centrally using the modern `IExceptionHandler` interface. Errors are safely logged and converted into standardized HTTP responses, improving observability and consistency.

* **User Context Encapsulation**
  An `ICurrentUserContext` abstraction is used to access authenticated user information. Services do not depend directly on `HttpContext`, which keeps them decoupled and easier to test in isolation.

* **Query Extensions for EF Core**
  Filtering, pagination, and sorting are implemented through reusable extension methods for `IQueryable` such as `FilterByDateRange` and `ApplySorting`. This keeps query logic reusable and application services focused on orchestration.

* **Decorators for Logging**
  The Decorator pattern is implemented through `Scrutor` to add cross-cutting concerns like logging without changing the core service implementations. This allows infrastructure behavior to stay separate from business logic.

* **Mapping & Validation**
  `Mapster` is used for efficient mapping between entities and DTOs. Input validation is handled by `FluentValidation`, integrated into the ASP.NET Core pipeline for clean request validation.

## Testing

The project includes a comprehensive suite of unit tests (the `APITest` project) to ensure reliability and prevent regressions.
* Business services and domain logic are tested in isolation using `Moq`.
* Database interaction logic is verified using the EF Core In-Memory Database provider.
* `FluentAssertions` is used to ensure test assertions are highly readable and expressive.
