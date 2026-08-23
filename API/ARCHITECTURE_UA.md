# Огляд архітектури

Цей проєкт побудований за принципами Clean Architecture: чотири проєкти розташовані так, що залежності спрямовані лише всередину — `Api` → `Infrastructure` → `Application` → `Domain`. `Domain` не залежить від жодного іншого шару в рішенні — він навіть не посилається на Entity Framework.

```
src/
  Domain/           сутності, Result/Error, інтерфейси репозиторіїв, доменні сервіси
  Application/      CQRS-команди/запити, хендлери, валідатори, DTO, мапінг
  Infrastructure/    EF Core DbContext, репозиторії, реалізації JWT/хешування паролів, DI
  Api/               контролери, Program.cs, HTTP-специфічні речі
tests/
  Application.Tests/ тести хендлерів і доменних сервісів (xUnit + Moq + EF InMemory)
```

## 1. Шари

### Domain (`src/Domain`)
Найвнутрішніший шар. Містить:
- **Entities** (`Entities/`) — прості класи, що мапляться на таблиці БД, без атрибутів EF чи бізнес-логіки.
- **`Common/Result.cs`, `Common/Error.cs`** — тип `Result`/`Result<T>`, який використовується замість винятків чи `null` для очікуваних відмов. Кожен хендлер повертає `Result`.
- **`Errors/`** — по одному статичному класу на агрегат (`AimErrors`, `TransactionErrors`, `UserErrors`, …), кожен метод будує іменований `Error` зі стабільним `Code` та `ErrorType` (`NotFound`, `Validation`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`).
- **`Repositories/`** — інтерфейси репозиторіїв та `IUnitOfWork`. Domain визначає контракти; `Infrastructure` їх реалізує. Репозиторії повертають сутності, ніколи DTO.
- **`Services/`** — чиста доменна логіка без будь-якого I/O: `IBalanceManager` (застосовує/відкочує вплив транзакції на баланси джерел) та `IAimProgressCalculator` (обчислює прогрес накопичення цілей по спільних джерелах з урахуванням пріоритету).

### Application (`src/Application`)
CQRS: кожен сценарій використання — це запис `Command` або `Query` разом із відповідним класом `...Handler` з єдиним методом `HandleAsync`, що повертає `Result` або `Result<T>`. Організовано за фічами під `Features/<Фіча>/{Commands,Queries}/<Операція>/`.

Форма хендлера завжди однакова:
1. Валідація команди через інжектований `IValidator<TCommand>` (FluentValidation); у разі помилки — повернути `Result.Failure` з `ValidationError`.
2. Завантаження потрібних даних через інтерфейси репозиторіїв.
3. Застосування зміни (мутація відстежуваної сутності або створення нової) та виклик `IUnitOfWork.SaveChangesAsync`.
4. Мапінг результату в DTO через Mapster (`IMapper`, інжектований) та повернення.

`Common/Mapping` містить конфігурації Mapster `IRegister` (наприклад, розгортання `Aim.SourceAims` у список `Sources`) та null-безпечний `IPatchMapper`, що використовується для PATCH-оновлень.

### Infrastructure (`src/Infrastructure`)
Реалізує все, що Domain та Application лише оголосили як інтерфейси:
- `Database/ApplicationDbContext.cs` + `Database/Configurations/*Configuration.cs` (по одному `IEntityTypeConfiguration<T>` на сутність, застосовуються через `ApplyConfigurationsFromAssembly`).
- `Database/Repositories/` — по одному репозиторію на агрегат; кожен відповідає за власну фільтрацію/сортування/пагінацію та завантажує (`Include`) лише ті навігаційні властивості, які потрібні його викликачам.
- `Security/` — `PasswordHasher` (BCrypt) та `JwtProvider` (видача та валідація токенів; перевірку чорного списку оркеструють хендлери Application, які його викликають, а не сам провайдер).
- `DependencyInjection.cs` — єдина точка входу `AddInfrastructure(configuration)`, що реєструє DbContext, FluentValidation, Mapster, репозиторії, доменні сервіси та всі хендлери Application (хендлери реєструються за угодою — кожен клас, чиє ім'я закінчується на `Handler` — замість одного рядка на хендлер).

### Api (`src/Api`)
Тонкі контролери. Кожна дія будує команду/запит, викликає свій хендлер і передає `Result` у `BaseApiController.HandleResult(...)`, який в одному місці мапить `ErrorType` на HTTP-статус і тіло `ProblemDetails`. `Security/CurrentUserContext.cs` реалізує `ICurrentUserContext` поверх `IHttpContextAccessor` — це HTTP-специфічна річ, тому вона живе тут, а не в Infrastructure.

## 2. Ключові патерни

### Result замість винятків для очікуваних відмов
Domain та Application ніколи не кидають винятки для "не знайдено" чи "помилка валідації" — вони повертають `Result.Failure(SomeErrors.X(...))`. `GlobalExceptionHandler` (в `Api/Utils`) існує лише для справді неочікуваних винятків і завжди повертає загальний 500; він не є частиною звичайного потоку керування.

### CQRS з явними хендлерами
Немає бібліотеки-медіатора чи пайплайну. Контролер викликає рівно один хендлер через ін'єкцію в конструктор. Це утримує стек викликів для будь-якого ендпоінту в межах двох переходів (`Controller` → `Handler`) і робить залежності кожного сценарію явними в його конструкторі.

### Repository + Unit of Work
Репозиторії ніколи не викликають `SaveChanges`; лише `IUnitOfWork.SaveChangesAsync` це робить. Це дозволяє хендлеру комбінувати кілька викликів репозиторіїв (наприклад, відкат балансів джерела й призначення при переказі) всередині одного `IUnitOfWork.BeginTransactionAsync` та закомітити один раз.

### Доменні сервіси лишаються чистими
`BalanceManager` та `AimProgressCalculator` приймають уже завантажені сутності як параметри і ніколи не торкаються `DbContext`. Відповідальність за завантаження даних через репозиторії лежить на хендлерах. Саме це робить їх тестованими без будь-якої бази даних.

### Patch-мапінг
Команди оновлення несуть nullable-поля для кожної властивості, яку можна змінити частково. `IPatchMapper.PatchInto(command, entity)` копіює лише не-null поля на відстежувану сутність, використовуючи конфігурацію Mapster з `IgnoreNullValues(true)`, окрему від конфігурації читання, але яка сканує ті самі класи `IRegister`.

## 3. Тестування

`tests/Application.Tests` перевіряє хендлери напряму поверх InMemory-провайдера EF Core через реальні реалізації репозиторіїв — без мокання бази даних. Зовнішні залежності (`IPasswordHasher`, `IJwtProvider`) мокаються через Moq лише там, де перевіряється логіка самого хендлера, а не колаборатора. Доменні сервіси (`BalanceManager`, `AimProgressCalculator`) тестуються взагалі без інфраструктури — це прості конструктори й виклики методів.
