# Architecture overview

This project is a layered ASP.NET Core API with a clean separation between controllers, services, domain logic, and technical infrastructure.

## 1. Layered structure

### Presentation layer
Controllers are intentionally thin. They only accept input, call a service, and return HTTP responses.

- [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11)
- [API/Controllers/UserController.cs](API/Controllers/UserController.cs#L11)
- [API/Controllers/TransactionController.cs](API/Controllers/TransactionController.cs#L13)

This keeps HTTP concerns out of business logic and makes controllers easy to test.

### Application layer
Most business rules live in services, not in controllers.

- [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L161)
- [API/Services/Category/CategoryService.cs](API/Services/Category/CategoryService.cs#L12-L80)
- [API/Domain/BalanceManagement/BalanceManagementService.cs](API/Domain/BalanceManagement/BalanceManagementService.cs#L8-L56)

The service layer coordinates database access, validation decisions, current user checks, and domain notifications.

### Domain / utility layer
Reusable technical concerns are isolated into focused components.

- current user context: [API/Utils/UserContext/CurrentUserProvider.cs](API/Utils/UserContext/CurrentUserProvider.cs#L5-L9)
- notification storage: [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13)
- global exception handler: [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15)
- mapping configuration: [API/Utils/Mapping/MapConfig.cs](API/Utils/Mapping/MapConfig.cs#L6-L14)
- mapping helper: [API/Extensions/MappingExtensions.cs](API/Extensions/MappingExtensions.cs#L5-L7)

## 2. Patterns used

### Thin controller pattern
Controllers are small orchestration points. They do not contain business rules, database logic, or cross-cutting concerns.

Example locations:

- [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11)
- [API/Controllers/UserController.cs](API/Controllers/UserController.cs#L11)
- [API/Controllers/TransactionController.cs](API/Controllers/TransactionController.cs#L13)

### Notification pattern
The project uses a notification context to collect multiple domain errors without throwing exceptions for every validation failure.

- [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13)
- [API/Filters/NotificationFilter.cs](API/Filters/NotificationFilter.cs#L7-L32)
- [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L35)
- [API/Services/Category/CategoryService.cs](API/Services/Category/CategoryService.cs#L12-L40)
- [API/Domain/BalanceManagement/BalanceManagementService.cs](API/Domain/BalanceManagement/BalanceManagementService.cs#L8-L24)

Why this is useful:

- several validation issues can be accumulated in one request
- the API can return one structured `ValidationProblemDetails` response
- business rules remain explicit in services
- the code avoids many small generic exceptions for expected domain failures

### Global request-to-response handling
The project uses an exception handler for unexpected failures and a request logger for consistent telemetry.

- [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15)
- [API/Program.cs](API/Program.cs#L112-L141)

The flow is:

1. unexpected exception is caught globally
2. it is logged once
3. the API returns a safe 500 response
4. every request is logged by Serilog request logging

### Decorator pattern
The project decorates selected services with logging wrappers.

- [API/Program.cs](API/Program.cs#L64-L67)
- [API/Services/Logging/JwtLoggingService.cs](API/Services/Logging/JwtLoggingService.cs#L5-L27)
- [API/Services/Logging/UserLoggingService.cs](API/Services/Logging/UserLoggingService.cs#L5-L42)

This is a clean way to add logging around existing services without changing their core logic.

#### Why `builder.Services.Decorate` is better than manual decoration

`builder.Services.Decorate` replaces the repetitive manual pattern where you register the inner service, resolve it yourself, and then wrap it.

Manual decoration usually looks like this:

```csharp
builder.Services.AddScoped<IJwtService>(sp =>
{
    var inner = ActivatorUtilities.CreateInstance<JwtService>(sp);
    return new JwtLoggingService(
        inner,
        sp.GetRequiredService<ILogger<JwtLoggingService>>(),
        sp.GetRequiredService<IHttpContextAccessor>());
});
```

With decoration, the registration stays simpler:

- base implementation: [API/Program.cs](API/Program.cs#L64-L64)
- wrapper: [API/Program.cs](API/Program.cs#L65-L67)

This is easier to read, easier to extend, and avoids duplicated dependency resolution code.

### Singleton pattern
`PasswordHasher` is registered as a singleton because it is stateless and safe to reuse across the application.

- [API/Program.cs](API/Program.cs#L61-L61)
- [API/Utils/PasswordHasher/PasswordHasher.cs](API/Utils/PasswordHasher/PasswordHasher.cs#L3-L13)

This is a good fit because the class only performs hashing and verification and does not keep per-request state.

### Chain of responsibility style pipeline
The request pipeline is an ordered chain of handlers. This is not a classic hand-written GoF chain class, but the architecture clearly follows the same idea.

Relevant steps:

- [API/Program.cs](API/Program.cs#L139-L141)
- [API/Filters/NotificationFilter.cs](API/Filters/NotificationFilter.cs#L7-L32)

The order matters:

1. exception handling
2. request logging
3. authentication
4. authorization
5. controller action filters
6. controller action execution

### Current-user abstraction
The current user is not read directly from controllers or services. It is wrapped behind a dedicated abstraction.

- interface: [API/Utils/UserContext/ICurrentUserProvider.cs](API/Utils/UserContext/ICurrentUserProvider.cs#L3-L6)
- implementation: [API/Utils/UserContext/CurrentUserProvider.cs](API/Utils/UserContext/CurrentUserProvider.cs#L5-L9)

This reduces repeated `HttpContext` access and keeps user ID lookup in one place.

### Query composition helpers
Filtering and sorting logic is extracted into extension methods instead of being duplicated in service methods.

- aims: [API/Extensions/EF/AimQueryExtensions.cs](API/Extensions/EF/AimQueryExtensions.cs#L8-L60)
- transactions: [API/Extensions/EF/TransactionQueryExtensions.cs](API/Extensions/EF/TransactionQueryExtensions.cs#L6-L28)
- planned transactions: [API/Extensions/EF/PlannedTransactionExtensions.cs](API/Extensions/EF/PlannedTransactionExtensions.cs#L5-L18)

Benefits:

- smaller service methods
- reusable query rules
- easier testing of query behavior
- better readability for complex filtering and sorting

### Patch mapping and null-safe updates
Patch/update operations use a reusable Mapster configuration that ignores null values.

- [API/Utils/Mapping/MapConfig.cs](API/Utils/Mapping/MapConfig.cs#L8-L14)
- [API/Extensions/MappingExtensions.cs](API/Extensions/MappingExtensions.cs#L5-L7)
- usage example: [API/Services/Category/CategoryService.cs](API/Services/Category/CategoryService.cs#L57-L66)

This avoids boilerplate manual property-by-property updates for patch endpoints.

### Validation layer
Validation is centralized through FluentValidation and automatic model validation.

- [API/Program.cs](API/Program.cs#L83-L85)
- validators are under [API/Validators](API/Validators)

This keeps input validation separate from business rules and prevents controllers from becoming validation-heavy.

## 3. Good architectural decisions in the code

- Controllers are thin and declarative.
- Business logic is in services.
- Domain errors are accumulated through notifications.
- Logging is added through decorators instead of copy-paste code.
- Global exception handling protects the API from leaking internal details.
- A current-user abstraction removes repeated `HttpContext` access.
- Query logic is composed with extension methods.
- Patch mapping is centralized and null-safe.
- Request logging is global and consistent.
- The password hasher is a stateless singleton.

## 4. Short conclusion

This codebase is a solid example of practical layered architecture in ASP.NET Core. The strongest parts are the small controllers, notification-based domain feedback, decorator-based logging, centralized user context, and the global request/exception pipeline.