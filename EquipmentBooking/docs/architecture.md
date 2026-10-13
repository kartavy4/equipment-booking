# Архитектура backend

## Зависимости

```text
Api ────────────────→ Application ──→ Domain
 │                        ↑
 └──→ Infrastructure ─────┘
```

Каждый слой — отдельный .csproj. Domain не ссылается на ASP.NET Core, EF Core или инфраструктуру. Application не ссылается на Api/Infrastructure. Infrastructure реализует интерфейсы Application. Api содержит HTTP-адаптеры и собирает зависимости.

## Структура

```text
src/
  EquipmentBooking.Domain/
    Bookings/          Booking, BookingRules
    Equipment/         EquipmentItem
    Users/             AppUser, Roles
    Common/            DomainRuleException
  EquipmentBooking.Application/
    Abstractions/      IApplicationDbContext, ICurrentUser, IPasswordService
    Auth/              IAuthService, AuthService, Contracts/
    Bookings/          IBookingService, BookingService, Contracts/
    Equipment/         IEquipmentService, EquipmentService, Contracts/
    Common/Errors/     AppException, ErrorType
    DependencyInjection.cs
  EquipmentBooking.Infrastructure/
    Persistence/
      Configurations/  отдельная конфигурация каждой сущности
      Migrations/      Initial, designer, model snapshot
      BookingDbContext, BookingDbContextFactory
      DatabaseInitializer, DatabaseHealthCheck
    Security/          PasswordService
    Options/           DatabaseOptions, SeedOptions
    DependencyInjection.cs
  EquipmentBooking.Api/
    Controllers/       Auth, Equipment, Bookings, Health
    Authentication/    CurrentUser, CookieSession, ConfigureCookieOptions
    Configuration/     MVC, валидация ответов, rate limiting
    Errors/            CSRF filter, expected exception filter, IExceptionHandler
    Extensions/        регистрация API и построение pipeline
    Contracts/         CsrfResponse, HealthResponse
    Program.cs         composition root
```

Одна сущность, DTO, интерфейс или сервис — один файл. В Program.cs нет сценариев, SQL-запросов, определения маршрутов или настройки cookie. Он вызывает регистрацию слоёв, строит приложение, применяет pipeline и инициализацию БД.

## Прохождение запроса

1. MVC выбирает контроллер. Authentication/Authorization проверяют cookie и роль.
2. Глобальный ValidateCsrfTokenFilter проверяет небезопасные HTTP-методы через IAntiforgery.
3. [ApiController] проверяет DataAnnotations входного DTO.
4. Контроллер вызывает интерфейс сервиса, передавая CancellationToken.
5. Сервис выполняет сценарий через IApplicationDbContext и доменные сущности.
6. Infrastructure сохраняет изменения и переводит известные ошибки провайдера в AppException.
7. Контроллер возвращает типизированный DTO. ExpectedExceptionFilter преобразует ожидаемые ошибки в HTTP-ответ.

AuthController управляет cookie через CookieSession; AuthService только проверяет/создаёт пользователя. Сервисы получают текущий ID и роль через ICurrentUser. Пароли доступны через IPasswordService; конкретный PasswordHasher живёт в Infrastructure.

## Доступ к данным

IApplicationDbContext намеренно использует DbSet и EF Core async queries: Application знает EF как инструмент запросов, но не знает конкретный DbContext, Npgsql или SQLite. Это явный компромисс: не дублировать EF Core универсальным CRUD-репозиторием. При необходимости независимости от ORM этот интерфейс можно заменить специализированными query/repository портами.

Сущности имеют private setters. Изменения выполняются конструкторами и методами Update/Cancel, которые удерживают доменные инварианты. ORM-конфигурация вынесена в IEntityTypeConfiguration.

## Ошибки

- DomainRuleException → 400.
- AppException(NotFound / Conflict / Unauthorized) → 404 / 409 / 401.
- Некорректные DTO → 400 с errors по полям.
- Непредвиденные ошибки → IExceptionHandler, журналирование и обезличенный 500.

Ответы имеют формат ProblemDetails со status/title/instance и extensions traceId/error. Поле error сохранено для совместимости с React. Ожидаемые ошибки обрабатываются MVC-фильтром, поэтому не записываются как неожиданные сбои сервера.

## Бронирование и сохранность данных

Интервалы полуоткрытые [start, end); время сохраняется в UTC. Проверка пересечения в сервисе даёт понятный ответ, а PostgreSQL EXCLUDE USING gist является окончательной защитой при гонке запросов. Известный SQLSTATE 23P01 преобразуется в конфликт только для EX_Booking_NoOverlap. Остальные ошибки БД не маскируются как занятый слот или дубликат email.

При рефакторинге сохранены таблицы, столбцы и ID миграции 202610130001_Initial. Перенос классов в другой namespace не требует пересоздания БД. Добавлены designer и snapshot для следующих EF-миграций; SQL-ограничение исключения находится в Initial.cs.

## Конфигурация и миграции

DatabaseOptions и SeedOptions привязаны к конфигурации и проверяются на старте. Database:Provider допускает Postgres/Sqlite. Database:InitializeOnStartup=false отключает автоматические миграции и seed — это режим для размещения, где миграции запускаются отдельным этапом.

```powershell
dotnet tool restore
dotnet ef migrations add MigrationName --project src/EquipmentBooking.Infrastructure --startup-project src/EquipmentBooking.Api --output-dir Persistence/Migrations
dotnet ef migrations has-pending-model-changes --project src/EquipmentBooking.Infrastructure --startup-project src/EquipmentBooking.Api
dotnet ef migrations script --idempotent --project src/EquipmentBooking.Infrastructure --startup-project src/EquipmentBooking.Api
```

BookingDbContextFactory создаёт design-time контекст PostgreSQL без подключения к серверу. Для применения миграций используйте отдельный этап с корректной строкой подключения; не используйте design-time placeholder пароль. Автогенерация не заменяет ревью SQL, особенно при изменении ограничения пересечений.

## Проверки

- Доменные тесты: даты, длительность, UTC, отмена.
- HTTP-тесты SQLite: cookie/CSRF, роли, регистрация и дубликаты, изоляция записей, пересечения, отмена, обслуживание, валидация и формат ProblemDetails.
- PostgreSQL HTTP-тест: два конкурентных запроса, один успех и один 409. Запускается при TEST_POSTGRES и в CI.
- TreatWarningsAsErrors, общий Directory.Build.props и .editorconfig.
- React-контракт проверяется сохранением маршрутов, полей ответов и текстовых ошибок.

Рефакторинг задаёт поддерживаемую структуру MVP. Готовность к внешнему production дополнительно требует конфигурации HTTPS/reverse proxy, постоянных ключей Data Protection, секретов, наблюдаемости и политики миграций для выбранного окружения.
