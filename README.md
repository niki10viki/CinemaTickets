# CinemaTickets

![CI](https://github.com/niki10viki/CinemaTickets/actions/workflows/ci.yml/badge.svg)

Навчальний проєкт з дисципліни «Конструювання програмного забезпечення».
Варіант № 9 — продаж квитків у кінотеатрі.

## Склад рішення

- `CinemaTickets.Core` — бібліотека класів з логікою предметної області: замовлення
  `TicketOrder`, квиток `TicketLine`, сервіс `TicketOrderService`, політики ціноутворення,
  виняток `DomainRuleException`, результат `Result<T>`, журнал аудиту `FileAuditLog`.
- `CinemaTickets.App` — консольний застосунок: розбір введення (барикада), діалог
  оформлення замовлення, налаштування журналу, верхній обробник винятків.
- `CinemaTickets.Tests` — модульні тести xUnit.

## Вимоги

.NET SDK 8.0 або новіший, Git.

## Складання, тести й запуск

```bash
dotnet build CinemaTickets.App
dotnet test CinemaTickets.Tests
dotnet run --project CinemaTickets.App
```

Без аргументів застосунок показує демонстрації попередніх лабораторних робіт.
Діалог оформлення замовлення запускається так:

```bash
dotnet run --project CinemaTickets.App -- order [шлях-до-звіту]
```

Квиток вводиться рядком «ряд місце ціна», порожній рядок завершує введення, далі
застосунок просить суму оплати. Дробова частина пишеться за культурою системи
(для української — через кому). Якщо передано шлях, після оплати зберігається звіт.

## Обробка помилок

- Помилки введення повертає розбирач у вигляді `Result<T>`: діалог пояснює, що не так,
  і повторює запит.
- Порушення правил предметної області (зайняте місце, недостатня сума) ядро повідомляє
  винятком `DomainRuleException` з ідентифікатором правила.
- Помилки програміста не перехоплюються: перевірки передумов і `Debug.Assert`
  зупиняють роботу якомога ближче до дефекту.
- Збої середовища (файл звіту чи аудиту недоступний) перехоплюються конкретними типами
  `IOException` і `UnauthorizedAccessException`; файли закриваються через `using`.
- Код завершення: `0` — успіх, `1` — порушено правило, `2` — непередбачений збій.

Повна класифікація ситуацій — у [docs/errors.md](docs/errors.md).

## Журнали

- Журнал роботи пише `Microsoft.Extensions.Logging` у потік помилок (stderr): у діалозі
  видно рівні від Debug, у демонстраціях — від Warning. Записи однієї операції мають
  область `Order:{OrderId}`.
- Журнал аудиту `audit.log` створюється в поточному каталозі; файли `*.log` до
  репозиторію не потрапляють.

## Безперервна інтеграція

Конвеєр `.github/workflows/ci.yml` запускається на кожен push у `main` і `feature/**` та на
кожен pull request до `main`. Збірку зупиняє будь-який непройдений критерій якості:

| Крок | Команда | Що ламає збірку |
| --- | --- | --- |
| Форматування | `dotnet format --verify-no-changes` | відступи, порядок using, зауваження аналізаторів без автовиправлення |
| Складання | `dotnet build -c Release` | будь-яке попередження CS, CA, SA, IDE (TreatWarningsAsErrors) |
| Залежності | NuGetAudit під час відновлення | відомий вразливий пакет, зокрема транзитивний |
| Тести | `dotnet test -c Release` | хоча б один червоний тест |
| Покриття | coverlet, `/p:Threshold=60` | покриття рядків нижче за 60 % |

Ті самі перевірки локально (у Git Bash замість `/p:` пишіть `-p:`):

```bash
dotnet format --verify-no-changes
dotnet build -c Release
dotnet test -c Release -p:CollectCoverage=true -p:Threshold=60 -p:ThresholdStat=total
```

Версію збірки задає `Directory.Build.props` (`VersionPrefix`), SDK — `global.json`.

## Документація

- [docs/errors.md](docs/errors.md) — класифікація помилок і стратегії обробки.
- [docs/model.md](docs/model.md) — модель предметної області.
- [CODING_STANDARD.md](CODING_STANDARD.md) — стандарт кодування проєкту.
