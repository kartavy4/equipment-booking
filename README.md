# Equip — Equipment Booking

Сервис бронирования оборудования: ASP.NET Core Controllers, EF Core, PostgreSQL и React + TypeScript.

## Структура

- `EquipmentBooking/` — backend solution: Domain, Application, Infrastructure, Api и тесты.
- `EquipmentBooking.Web/` — отдельное React-приложение на Vite.
- `.github/workflows/` — CI для обоих проектов.

Папки backend и frontend остаются отдельными и соседними.

## Локальный запуск без Docker

Требования: .NET SDK 8, Node.js 22.

В первом терминале из корня репозитория:

```powershell
dotnet run --project EquipmentBooking/src/EquipmentBooking.Api --launch-profile Demo
```

Во втором:

```powershell
cd EquipmentBooking.Web
npm ci
npm run dev
```

Открыть http://localhost:5173. Зарегистрировать пользователя через интерфейс.
В Demo используется SQLite. Основная PostgreSQL-конфигурация обеспечивает защиту от конкурентного двойного бронирования.

## Запуск с PostgreSQL

Запустить Docker Desktop:

```powershell
cd EquipmentBooking
Copy-Item .env.example .env
# Установить собственные ADMIN_PASSWORD и POSTGRES_PASSWORD в .env.
docker compose up --build -d
```

React запускается отдельно в соседней папке. API доступен на http://localhost:5080.

## Проверки

```powershell
dotnet test EquipmentBooking/EquipmentBooking.sln -c Release
cd EquipmentBooking.Web
npm ci
npm test
npm run build
```

Локально PostgreSQL-тест требует переменную TEST_POSTGRES со строкой подключения к отдельной тестовой БД. В GitHub Actions PostgreSQL запускается автоматически.

## Документация

- [Backend: запуск, API и ограничения MVP](EquipmentBooking/README.md)
- [Архитектура backend](EquipmentBooking/docs/architecture.md)
- [Frontend](EquipmentBooking.Web/README.md)

Локальные БД, .env, зависимости и результаты сборки исключены из Git.
