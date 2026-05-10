# Огляд архітектури

Цей проєкт є багаторівневим ASP.NET Core API з чітким розділенням між контролерами, сервісами, доменною логікою та технічною інфраструктурою, що видно в [API/Program.cs](API/Program.cs#L61-L141) і файлах нижче.

## 1. Структура шарів

### Шар презентації
Контролери навмисно зроблені тонкими. Вони лише приймають вхідні дані, викликають сервіс і повертають HTTP-відповідь, як у [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11-L56), [API/Controllers/UserController.cs](API/Controllers/UserController.cs#L11-L57) та [API/Controllers/TransactionController.cs](API/Controllers/TransactionController.cs#L13-L43).

- [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11)
- [API/Controllers/UserController.cs](API/Controllers/UserController.cs#L11)
- [API/Controllers/TransactionController.cs](API/Controllers/TransactionController.cs#L13)

Це прибирає HTTP-логіку з бізнес-правил і робить контролери простими для тестування.

### Шар застосунку
Більшість бізнес-правил знаходиться в сервісах, а не в контролерах, наприклад у [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L161), [API/Services/Category/CategoryService.cs](API/Services/Category/CategoryService.cs#L12-L80) та [API/Domain/BalanceManagement/BalanceManagementService.cs](API/Domain/BalanceManagement/BalanceManagementService.cs#L8-L56).

- [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L161)
- [API/Services/Category/CategoryService.cs](API/Services/Category/CategoryService.cs#L12-L80)
- [API/Domain/BalanceManagement/BalanceManagementService.cs](API/Domain/BalanceManagement/BalanceManagementService.cs#L8-L56)

Шар сервісів координує доступ до бази даних, перевірки, визначення поточного користувача та доменні нотифікації, щоб HTTP-рівень залишався мінімальним.

### Доменний та утилітарний шар
Повторно використовувані технічні речі винесені в окремі компактні компоненти, такі як [API/Utils/UserContext/CurrentUserProvider.cs](API/Utils/UserContext/CurrentUserProvider.cs#L5-L9), [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13) і [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15).

- контекст поточного користувача: [API/Utils/UserContext/CurrentUserProvider.cs](API/Utils/UserContext/CurrentUserProvider.cs#L5-L9)
- збереження нотифікацій: [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13)
- глобальний обробник винятків: [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15)
- конфігурація мапінгу: [API/Utils/Mapping/MapConfig.cs](API/Utils/Mapping/MapConfig.cs#L6-L14)
- допоміжний мапінг-хелпер: [API/Extensions/MappingExtensions.cs](API/Extensions/MappingExtensions.cs#L5-L7)

## 2. Використані патерни

### Патерн тонкого контролера
Контролери є маленькими точками прийому і передачі даних. Вони не містять бізнес-правил, роботи з базою даних або крос-секційних аспектів, саме тому [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11-L56) та інші контролери залишаються короткими.

Приклади:

- [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11)
- [API/Controllers/UserController.cs](API/Controllers/UserController.cs#L11)
- [API/Controllers/TransactionController.cs](API/Controllers/TransactionController.cs#L13)

### Патерн нотифікацій
У проєкті використовується контекст нотифікацій, щоб збирати кілька доменних помилок без кидання винятків на кожну очікувану проблему валідації. Основні частини — [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13) і [API/Filters/NotificationFilter.cs](API/Filters/NotificationFilter.cs#L7-L32).

- [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13)
- [API/Filters/NotificationFilter.cs](API/Filters/NotificationFilter.cs#L7-L32)
- [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L35)
- [API/Services/Category/CategoryService.cs](API/Services/Category/CategoryService.cs#L12-L40)
- [API/Domain/BalanceManagement/BalanceManagementService.cs](API/Domain/BalanceManagement/BalanceManagementService.cs#L8-L24)

Чому це корисно:

- можна накопичувати кілька помилок за один запит
- API може повернути один структурований `ValidationProblemDetails`
- бізнес-правила залишаються явними в сервісах
- код уникає багатьох дрібних загальних винятків для очікуваних доменних помилок

### Глобальна обробка запитів і відповідей
Проєкт використовує глобальний обробник винятків для непередбачених помилок і глобальне логування запитів для стабільної телеметрії, налаштоване в [API/Program.cs](API/Program.cs#L112-L141) та реалізоване в [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15).

- [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15)
- [API/Program.cs](API/Program.cs#L112-L141)

Потік роботи такий:

1. непередбачений виняток перехоплюється глобально
2. він логуються один раз
3. API повертає безпечну відповідь 500
4. кожен запит логуються через Serilog request logging

### Патерн декоратора
Для окремих сервісів використані декоратори з логуванням, зареєстровані в [API/Program.cs](API/Program.cs#L64-L67) і реалізовані в [API/Services/Logging/JwtLoggingService.cs](API/Services/Logging/JwtLoggingService.cs#L5-L27) та [API/Services/Logging/UserLoggingService.cs](API/Services/Logging/UserLoggingService.cs#L5-L42).

- [API/Program.cs](API/Program.cs#L64-L67)
- [API/Services/Logging/JwtLoggingService.cs](API/Services/Logging/JwtLoggingService.cs#L5-L27)
- [API/Services/Logging/UserLoggingService.cs](API/Services/Logging/UserLoggingService.cs#L5-L42)

Це зручний спосіб додати логування навколо існуючих сервісів без зміни їхньої основної логіки.

#### `builder.Services.Decorate` 

`builder.Services.Decorate` замінює повторюваний ручний підхід, коли потрібно окремо реєструвати внутрішній сервіс, вручну його резолвити і потім загортати в обгортку.

Ручний варіант зазвичай виглядає так:

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

З `Decorate` реєстрація стає простішою:

- базова реалізація: [API/Program.cs](API/Program.cs#L64-L64)
- обгортка: [API/Program.cs](API/Program.cs#L65-L67)

Це легше читати, простіше розширювати і не дублює код резолву залежностей порівняно з ручним варіантом вище.

### Singleton
`PasswordHasher` зареєстрований як singleton, тому що він не має стану і безпечно перевикористовується в межах усього застосунку, як налаштовано в [API/Program.cs](API/Program.cs#L61-L61) і реалізовано в [API/Utils/PasswordHasher/PasswordHasher.cs](API/Utils/PasswordHasher/PasswordHasher.cs#L3-L13).

- [API/Program.cs](API/Program.cs#L61-L61)
- [API/Utils/PasswordHasher/PasswordHasher.cs](API/Utils/PasswordHasher/PasswordHasher.cs#L3-L13)

Це хороший вибір, тому що клас лише виконує хешування і перевірку, не зберігаючи per-request стан.

### Сhain of responsibility у pipeline
Запит проходить через впорядкований ланцюг обробників у [API/Program.cs](API/Program.cs#L139-L141) та [API/Filters/NotificationFilter.cs](API/Filters/NotificationFilter.cs#L7-L32).

Важливі кроки:

- [API/Program.cs](API/Program.cs#L139-L141)
- [API/Filters/NotificationFilter.cs](API/Filters/NotificationFilter.cs#L7-L32)

Порядок має значення:

1. обробка винятків
2. логування запитів
3. аутентифікація
4. авторизація
5. action filters контролера
6. виконання action методу

### Абстракція поточного користувача
Поточний користувач не читається напряму в контролерах або сервісах. Для цього є окрема абстракція в [API/Utils/UserContext/ICurrentUserProvider.cs](API/Utils/UserContext/ICurrentUserProvider.cs#L3-L6) та [API/Utils/UserContext/CurrentUserProvider.cs](API/Utils/UserContext/CurrentUserProvider.cs#L5-L9).

- інтерфейс: [API/Utils/UserContext/ICurrentUserProvider.cs](API/Utils/UserContext/ICurrentUserProvider.cs#L3-L6)
- реалізація: [API/Utils/UserContext/CurrentUserProvider.cs](API/Utils/UserContext/CurrentUserProvider.cs#L5-L9)

Це зменшує кількість прямого доступу до `HttpContext` і тримає логіку отримання ID користувача в одному місці.

### Допоміжні методи композиції запитів
Логіка фільтрації і сортування винесена в extension methods замість дублювання в сервісах, головним чином у [API/Extensions/EF/AimQueryExtensions.cs](API/Extensions/EF/AimQueryExtensions.cs#L8-L60), [API/Extensions/EF/TransactionQueryExtensions.cs](API/Extensions/EF/TransactionQueryExtensions.cs#L6-L28) та [API/Extensions/EF/PlannedTransactionExtensions.cs](API/Extensions/EF/PlannedTransactionExtensions.cs#L5-L18).

- для aim: [API/Extensions/EF/AimQueryExtensions.cs](API/Extensions/EF/AimQueryExtensions.cs#L8-L60)
- для transactions: [API/Extensions/EF/TransactionQueryExtensions.cs](API/Extensions/EF/TransactionQueryExtensions.cs#L6-L28)
- для planned transactions: [API/Extensions/EF/PlannedTransactionExtensions.cs](API/Extensions/EF/PlannedTransactionExtensions.cs#L5-L18)

Переваги:

- менші методи сервісів
- повторно використовувані правила запитів
- простіше тестування поведінки фільтрації
- краща читабельність для складних умов сортування і фільтрації

### Patch-мапінг і оновлення без null
Операції patch/update використовують повторно використовувану конфігурацію Mapster, яка ігнорує null-значення, визначену в [API/Utils/Mapping/MapConfig.cs](API/Utils/Mapping/MapConfig.cs#L8-L14) і застосовану через [API/Extensions/MappingExtensions.cs](API/Extensions/MappingExtensions.cs#L5-L7).

- [API/Utils/Mapping/MapConfig.cs](API/Utils/Mapping/MapConfig.cs#L8-L14)
- [API/Extensions/MappingExtensions.cs](API/Extensions/MappingExtensions.cs#L5-L7)
- приклад використання: [API/Services/Category/CategoryService.cs](API/Services/Category/CategoryService.cs#L57-L66)

Це прибирає шаблонний ручний код, де потрібно оновлювати властивості по одній.

### Шар валідації
Валідація централізована через FluentValidation та автоматичну model validation, підключену в [API/Program.cs](API/Program.cs#L83-L85), а валідатори знаходяться в [API/Validators](API/Validators).

- [API/Program.cs](API/Program.cs#L83-L85)
- валідатори знаходяться в [API/Validators](API/Validators)

Це відділяє валідацію від бізнес-логіки та не дозволяє контролерам перетворюватися на великий блок перевірок.

## 3. Хороші архітектурні рішення в коді

- Контролери короткі та декларативні, наприклад [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11-L56).
- Бізнес-логіка знаходиться в сервісах, наприклад [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L161).
- Доменні помилки накопичуються через нотифікації в [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13).
- Логування додане через декоратори, а не через копіювання коду, у [API/Services/Logging/JwtLoggingService.cs](API/Services/Logging/JwtLoggingService.cs#L5-L27) та [API/Services/Logging/UserLoggingService.cs](API/Services/Logging/UserLoggingService.cs#L5-L42).
- Глобальна обробка винятків захищає API від витоку внутрішніх деталей у [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15).
- Абстракція поточного користувача прибирає повторюваний доступ до `HttpContext` у [API/Utils/UserContext/CurrentUserProvider.cs](API/Utils/UserContext/CurrentUserProvider.cs#L5-L9).
- Логіка запитів композиційно зібрана через extension methods у [API/Extensions/EF/AimQueryExtensions.cs](API/Extensions/EF/AimQueryExtensions.cs#L8-L60).
- Patch-мапінг централізований і null-safe у [API/Utils/Mapping/MapConfig.cs](API/Utils/Mapping/MapConfig.cs#L8-L14).
- Глобальне логування запитів є стабільним і послідовним завдяки [API/Program.cs](API/Program.cs#L139-L141).
- Password hasher є stateless singleton у [API/Program.cs](API/Program.cs#L61-L61).

## 5. Принципи чистого коду

### KISS
Keep It Simple, Stupid. Код дотримується цього принципу через тонкі контролери та винесення логіки у вузькі сервіси.

- тонкі контролери: [API/Controllers/AimController.cs](API/Controllers/AimController.cs#L11-L56)
- логіка сервісу: [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L161)
- налаштування pipeline: [API/Program.cs](API/Program.cs#L61-L141)

### DRY
Don't Repeat Yourself. Повторювану логіку винесено в повторно використовувані компоненти замість копіювання.

- зберігання та обробка нотифікацій: [API/Utils/Notification/NotificationContext.cs](API/Utils/Notification/NotificationContext.cs#L3-L13) і [API/Filters/NotificationFilter.cs](API/Filters/NotificationFilter.cs#L7-L32)
- допоміжні методи для запитів: [API/Extensions/EF/AimQueryExtensions.cs](API/Extensions/EF/AimQueryExtensions.cs#L8-L60), [API/Extensions/EF/TransactionQueryExtensions.cs](API/Extensions/EF/TransactionQueryExtensions.cs#L6-L28), [API/Extensions/EF/PlannedTransactionExtensions.cs](API/Extensions/EF/PlannedTransactionExtensions.cs#L5-L18)
- хелпер для patch-мапінгу: [API/Extensions/MappingExtensions.cs](API/Extensions/MappingExtensions.cs#L5-L7)

### SOLID

- **S — Single Responsibility Principle**: кожен клас має одну основну причину для змін. Наприклад, [API/Services/Logging/JwtLoggingService.cs](API/Services/Logging/JwtLoggingService.cs#L5-L27) додає лише логування навколо JWT-логіки, а [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15) займається тільки глобальними винятками.
- **O — Open/Closed Principle**: поведінку можна розширювати без зміни споживачів, наприклад через [API/Services/Logging/UserLoggingService.cs](API/Services/Logging/UserLoggingService.cs#L5-L42) і декорування в [API/Program.cs](API/Program.cs#L64-L67).
- **L — Liskov Substitution Principle**: сервіси використовуються через інтерфейси, наприклад [API/Services/Jwt/IJwtService.cs](API/Services/Jwt/IJwtService.cs) і [API/Services/User/IUserService.cs](API/Services/User/IUserService.cs), тому реалізації можна замінювати без поломки викликів.
- **I — Interface Segregation Principle**: використовуються малі сфокусовані інтерфейси, як [API/Utils/UserContext/ICurrentUserProvider.cs](API/Utils/UserContext/ICurrentUserProvider.cs#L3-L6), [API/Utils/PasswordHasher/IPasswordHasher.cs](API/Utils/PasswordHasher/IPasswordHasher.cs#L3-L6) і інтерфейси сервісів у папці Services.
- **D — Dependency Inversion Principle**: високорівневий код залежить від абстракцій, а не від конкретних реалізацій. Це видно в [API/Program.cs](API/Program.cs#L61-L88) і в конструкторах на кшталт [API/Services/Aim/AimService.cs](API/Services/Aim/AimService.cs#L12-L12).

### Інші корисні принципи

- **Composition over inheritance**: логіка зібрана з маленьких сервісів, декораторів і extension methods, а не з глибоких ієрархій.
- **Separation of concerns**: HTTP-логіка, валідація, доменні нотифікації, логування і доступ до даних розділені по різних файлах.
- **Fail fast для непередбачених помилок**: необроблені винятки ловить глобальний обробник у [API/Utils/ExceptionHandler/GlobalExceptionHandler.cs](API/Utils/ExceptionHandler/GlobalExceptionHandler.cs#L5-L15).

