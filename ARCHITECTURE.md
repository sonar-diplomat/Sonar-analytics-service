# Архитектура Analytics Service

## Обзор

Analytics Service — это микросервис для сбора и анализа пользовательских событий в музыкальном приложении. Сервис построен на основе **Clean Architecture** (Onion Architecture) и использует **gRPC** для межсервисного взаимодействия.

## Технологический стек

- **.NET 10.0** — платформа разработки
- **gRPC** — протокол для API (HTTP/2)
- **Entity Framework Core 10.0** — ORM для работы с базой данных
- **PostgreSQL** — реляционная база данных
- **Npgsql** — провайдер PostgreSQL для EF Core

## Архитектура проекта

Проект организован по принципу **Clean Architecture** с четким разделением на слои:

```
Analytics.API (Presentation Layer)
    ↓
Analytics.Application (Application Layer)
    ↓
Analytics.Domain (Domain Layer)
    ↑
Analytics.Infrastructure (Infrastructure Layer)
```

### Слои архитектуры

#### 1. **Analytics.Domain** — Доменный слой

**Назначение:** Содержит бизнес-логику и доменные сущности, не зависящие от внешних библиотек.

**Компоненты:**
- `UserEvent` — основная доменная сущность, представляющая событие пользователя
- `EventType` — перечисление типов событий (PlayStart, PlayFinish, Skip, Like, AddToPlaylist, Search)
- `ContextType` — перечисление типов контекста (Track, Playlist, Album, Radio, Search)

**Особенности:**
- Чистый C# код без зависимостей от внешних библиотек
- Содержит только бизнес-логику и доменные модели

#### 2. **Analytics.Application** — Слой приложения

**Назначение:** Содержит бизнес-логику приложения, use cases и абстракции репозиториев.

**Компоненты:**

**UserEvents:**
- `AddUserEventCommand` — команда для добавления события
- `AddUserEventHandler` — обработчик команды (CQRS паттерн)

**Recommendations:**
- `GetPopularCollectionsQuery` / `GetPopularCollectionsHandler` — получение популярных коллекций
- `GetRecentCollectionsQuery` / `GetRecentCollectionsHandler` — получение недавних коллекций пользователя
- `GetRecentTracksQuery` / `GetRecentTracksHandler` — получение недавних треков пользователя
- `CursorHelper` — утилита для работы с курсорной пагинацией

**Abstractions:**
- `IUserEventsRepository` — интерфейс репозитория для работы с событиями

**Особенности:**
- Использует паттерн **CQRS** (Command Query Responsibility Segregation)
- Зависит только от Domain слоя
- Содержит абстракции для работы с данными (интерфейсы репозиториев)

#### 3. **Analytics.Infrastructure** — Слой инфраструктуры

**Назначение:** Реализация технических деталей: работа с БД, внешние сервисы.

**Компоненты:**

**Persistence:**
- `AnalyticsDbContext` — контекст Entity Framework Core
- `UserEventsRepository` — реализация репозитория для работы с событиями
- `Migrations/` — миграции базы данных

**Особенности:**
- Использует Entity Framework Core для работы с PostgreSQL
- Реализует интерфейсы из Application слоя
- Содержит конфигурацию маппинга сущностей на таблицы БД

**Конфигурация БД:**
- Таблица: `user_events`
- Индексы на: `user_id`, `track_id`, `event_type`, `timestamp_utc`
- JSONB колонка для `payload` (гибкое хранение дополнительных данных)

#### 4. **Analytics.API** — Слой представления

**Назначение:** Точка входа в приложение, обработка gRPC запросов.

**Компоненты:**

**Services:**
- `AnalyticsGrpcService` — обработка запросов на добавление событий
- `RecommendationsGrpcService` — обработка запросов на получение рекомендаций

**Protos:**
- `analytics.proto` — определение gRPC сервиса для аналитики
- `recommendations.proto` — определение gRPC сервиса для рекомендаций

**Особенности:**
- Использует gRPC для межсервисного взаимодействия
- HTTP/2 протокол (настроен в `appsettings.json`)
- Маппинг между proto-сообщениями и доменными моделями

## gRPC API

### Analytics Service

#### `AddUserEvent`

Добавляет событие пользователя в систему.

**Request (`UserEventRequest`):**
- `user_id` (int32) — ID пользователя
- `track_id` (int32, optional) — ID трека (0 если не указан)
- `event_type` (EventType) — тип события
- `context_type` (ContextType) — тип контекста
- `context_id` (int32, optional) — ID контекста (playlist/album, 0 если не указан)
- `position_ms` (int64) — позиция воспроизведения в миллисекундах (0 если не указано)
- `duration_ms` (int64) — длительность в миллисекундах (0 если не указано)
- `timestamp` (Timestamp) — время события (UTC)
- `payload` (string, optional) — дополнительные данные в JSON формате

**Response (`AddUserEventResponse`):**
- `success` (bool) — результат операции

**Особенности:**
- Значение `0` для опциональных полей (`track_id`, `context_id`, `position_ms`, `duration_ms`) интерпретируется как `null`
- Timestamp автоматически конвертируется в UTC
- Payload сохраняется как JSONB в PostgreSQL

### Recommendations Service

#### `GetPopularCollections`

Возвращает популярные коллекции (альбомы/плейлисты) на основе аналитики.

**Request (`GetPopularCollectionsRequest`):**
- `limit` (int32, optional) — максимальное количество результатов (по умолчанию 4)

**Response (`GetPopularCollectionsResponse`):**
- `collections` (repeated PopularCollection) — список популярных коллекций
  - `collection_id` (string) — ID коллекции
  - `collection_type` (CollectionType) — тип коллекции (Album/Playlist)
  - `score` (double) — рейтинг популярности
  - `plays` (int64) — количество прослушиваний
  - `likes` (int64) — количество лайков
  - `adds` (int64) — количество добавлений в плейлисты

**Алгоритм расчета рейтинга:**
```
score = plays × 1.0 + likes × 0.5 + adds × 0.7
```

#### `GetRecentCollections`

Возвращает недавно прослушанные коллекции пользователя с курсорной пагинацией.

**Request (`GetRecentCollectionsRequest`):**
- `user_id` (string) — GUID пользователя
- `limit` (int32, optional) — количество результатов (по умолчанию 5, максимум 50)
- `cursor` (string, optional) — курсор для пагинации

**Response (`GetRecentCollectionsResponse`):**
- `collections` (repeated RecentCollection) — список коллекций
  - `collection_id` (string) — ID коллекции
  - `collection_type` (CollectionType) — тип коллекции
  - `last_played_at` (Timestamp) — время последнего прослушивания
- `next_cursor` (string, optional) — курсор для следующей страницы

**Особенности пагинации:**
- Используется курсор на основе `last_played_at` и `collection_id`
- Курсор кодируется в Base64: `{ticks}|{guid}`
- Сортировка: по убыванию времени, затем по ID

#### `GetRecentTracks`

Возвращает недавно прослушанные треки пользователя с курсорной пагинацией.

**Request (`GetRecentTracksRequest`):**
- `user_id` (string) — GUID пользователя
- `limit` (int32, optional) — количество результатов (по умолчанию 5, максимум 50)
- `cursor` (string, optional) — курсор для пагинации

**Response (`GetRecentTracksResponse`):**
- `tracks` (repeated RecentTrack) — список треков
  - `track_id` (string) — ID трека
  - `last_played_at` (Timestamp) — время последнего прослушивания
  - `context_id` (string, optional) — ID контекста
  - `context_type` (ContextType) — тип контекста
- `next_cursor` (string, optional) — курсор для следующей страницы

## Типы данных

### Важные особенности

#### ID полей — Integer вместо GUID

**Историческая справка:** Изначально `user_id`, `track_id` и `context_id` были типа `Guid` (UUID), но были изменены на `int32` для оптимизации и упрощения работы с клиентами.

**Текущая реализация:**
- Proto: `int32` для всех ID полей
- Domain: `int` для `UserId`, `int?` для `TrackId` и `ContextId`
- Database: `integer` колонки в PostgreSQL

**Миграция:**
- Миграция `20251211030758_ChangeUserIdTrackIdContextIdToInt` изменяет типы колонок
- **Важно:** Миграция удаляет все существующие данные, так как UUID нельзя преобразовать в integer

#### Опциональные поля

В proto файлах опциональные поля представлены как:
- `int32` с значением `0` = отсутствует
- `int64` с значением `0` = отсутствует
- `string` пустая = отсутствует

В C# коде это маппится на nullable типы:
```csharp
request.TrackId == 0 ? null : request.TrackId
request.PositionMs == 0 ? null : checked((int)request.PositionMs)
```

## База данных

### Схема таблицы `user_events`

| Колонка | Тип | Nullable | Описание |
|---------|-----|----------|----------|
| `id` | uuid | NO | Уникальный идентификатор события |
| `user_id` | integer | NO | ID пользователя |
| `track_id` | integer | YES | ID трека (null для событий поиска) |
| `event_type` | integer | NO | Тип события (enum) |
| `context_type` | integer | NO | Тип контекста (enum) |
| `context_id` | integer | YES | ID контекста (playlist/album) |
| `position_ms` | integer | YES | Позиция воспроизведения в мс |
| `duration_ms` | integer | YES | Длительность в мс |
| `timestamp_utc` | timestamp with time zone | NO | Время события (UTC) |
| `payload` | jsonb | YES | Дополнительные данные в JSON |

### Индексы

- `idx_user_events_user` — на `user_id` (для быстрого поиска событий пользователя)
- `idx_user_events_track` — на `track_id` (для аналитики по трекам)
- `idx_user_events_event_type` — на `event_type` (для фильтрации по типам событий)
- `idx_user_events_ts` — на `timestamp_utc` (для временных запросов)

### Миграции

Проект использует Entity Framework Core Migrations:
- `20251210163306_InitialCreate` — первоначальная схема БД
- `20251211030758_ChangeUserIdTrackIdContextIdToInt` — изменение типов ID полей

**Применение миграций:**
```bash
dotnet ef database update --project Analytics.Infrastructure --startup-project Analytics.API
```

## Dependency Injection

### Регистрация сервисов

**Application Layer** (`Analytics.Application/DependencyInjection.cs`):
```csharp
services.AddScoped<AddUserEventHandler>();
services.AddScoped<GetPopularCollectionsHandler>();
services.AddScoped<GetRecentCollectionsHandler>();
services.AddScoped<GetRecentTracksHandler>();
```

**Infrastructure Layer** (`Analytics.Infrastructure/DependencyInjection.cs`):
```csharp
services.AddDbContext<AnalyticsDbContext>(options => {
    options.UseNpgsql(connectionString);
});
services.AddScoped<IUserEventsRepository, UserEventsRepository>();
```

**API Layer** (`Analytics.API/Program.cs`):
```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddGrpc();
```

## Конфигурация

### Строка подключения к БД

Поддерживается два способа указания строки подключения:
1. Переменная окружения: `ANALYTICS_DB_CONNECTION`
2. Секция конфигурации: `ConnectionStrings:AnalyticsDb`

Приоритет у переменной окружения.

### Kestrel настройки

В `appsettings.json` настроен HTTP/2 протокол для gRPC:
```json
{
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols": "Http2"
    }
  }
}
```

## Паттерны проектирования

### 1. Clean Architecture (Onion Architecture)

Четкое разделение на слои с зависимостями, направленными внутрь:
- Domain — ядро, без зависимостей
- Application — зависит только от Domain
- Infrastructure — зависит от Application и Domain
- API — зависит от всех слоев

### 2. CQRS (Command Query Responsibility Segregation)

Разделение операций записи и чтения:
- **Commands:** `AddUserEventCommand` + `AddUserEventHandler`
- **Queries:** `GetPopularCollectionsQuery` + `GetPopularCollectionsHandler`

### 3. Repository Pattern

Абстракция доступа к данным через интерфейс `IUserEventsRepository`:
- Позволяет легко заменить реализацию
- Упрощает тестирование
- Инкапсулирует логику работы с БД

### 4. Dependency Injection

Все зависимости регистрируются через DI контейнер .NET:
- Scoped lifetime для handlers и репозиториев
- DbContext также в Scoped scope (стандарт для EF Core)

## Особенности реализации

### 1. Курсорная пагинация

Вместо offset-based пагинации используется курсорная:
- **Преимущества:** Стабильность при изменении данных, лучшая производительность
- **Реализация:** Base64-кодированный курсор: `{timestamp_ticks}|{id}`
- **Использование:** В методах `GetRecentCollections` и `GetRecentTracks`

### 2. Взвешенный рейтинг популярности

Популярность коллекций рассчитывается по формуле:
```
score = plays × 1.0 + likes × 0.5 + adds × 0.7
```

Веса настраиваются в `GetPopularCollectionsHandler`:
- `PlayWeight = 1.0`
- `LikeWeight = 0.5`
- `AddWeight = 0.7`

### 3. Гибкое хранение данных

Поле `payload` хранится как JSONB в PostgreSQL:
- Позволяет хранить произвольные дополнительные данные
- Поддерживает индексацию и запросы JSON в PostgreSQL
- Не требует изменения схемы БД при добавлении новых полей

### 4. Обработка времени

Все временные метки хранятся и обрабатываются в UTC:
- `TimestampUtc` в доменной модели
- Автоматическая конвертация при получении из gRPC: `request.Timestamp.ToDateTime().ToUniversalTime()`

### 5. Валидация и обработка ошибок

- Валидация типов в gRPC сервисах (например, проверка GUID для `user_id` в Recommendations)
- Использование `RpcException` для возврата ошибок клиенту
- Ограничения на `limit` (максимум 50 для пагинации)

## Производительность

### Оптимизации запросов

1. **AsNoTracking()** — используется для read-only запросов (рекомендации)
2. **Индексы** — на часто используемых полях (`user_id`, `track_id`, `event_type`, `timestamp_utc`)
3. **Группировка на уровне БД** — агрегация выполняется в PostgreSQL, а не в памяти
4. **Take() перед ToListAsync()** — ограничение результатов на уровне БД

### Рекомендации по масштабированию

1. **Партиционирование таблицы** — по `timestamp_utc` для больших объемов данных
2. **Read Replicas** — для read-heavy операций (рекомендации)
3. **Кэширование** — популярные коллекции можно кэшировать
4. **Асинхронная обработка** — для записи событий можно использовать очередь

## Безопасность

### Текущие меры

- Валидация входных данных в gRPC сервисах
- Ограничение размера запросов (limit)
- Использование параметризованных запросов (EF Core)

### Рекомендации

1. **Аутентификация** — добавить проверку токенов/API ключей
2. **Rate Limiting** — ограничение количества запросов от одного клиента
3. **Audit Logging** — логирование всех операций
4. **Шифрование** — TLS для gRPC соединений

## Тестирование

### Рекомендуемая структура тестов

1. **Unit тесты** — для handlers и бизнес-логики
2. **Integration тесты** — для репозиториев с тестовой БД
3. **API тесты** — для gRPC endpoints

### Моки и заглушки

- Использовать `IUserEventsRepository` интерфейс для мокирования
- In-memory БД для интеграционных тестов EF Core

## Развертывание

### Требования

- .NET 10.0 Runtime
- PostgreSQL 12+
- HTTP/2 поддержка (для gRPC)

### Переменные окружения

- `ANALYTICS_DB_CONNECTION` — строка подключения к PostgreSQL

### Docker (пример)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "Analytics.API.dll"]
```

## Известные ограничения и TODO

1. **Миграция типов ID** — при изменении с UUID на int все данные удаляются
2. **Нет версионирования API** — при изменении proto файлов требуется синхронизация всех клиентов
3. **Нет кэширования** — популярные коллекции пересчитываются при каждом запросе
4. **Нет batch операций** — события добавляются по одному
5. **Ограниченная аналитика** — только базовые метрики, нет сложных аналитических запросов

## Заключение

Analytics Service представляет собой хорошо структурированный микросервис, построенный на принципах Clean Architecture. Проект демонстрирует:

- Четкое разделение ответственности между слоями
- Использование современных паттернов (CQRS, Repository)
- Эффективную работу с базой данных
- Гибкость и расширяемость архитектуры

Проект готов к использованию в production среде, но требует дополнительных мер безопасности и оптимизаций для высоконагруженных систем.

