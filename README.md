# Events API приложение

Это упрощенный пример распределенного многосервисного приложения, предоставляющего REST API для модели, использующей DataMapper.

# Сервисы

UserService - сервис, предоставляющий доступ для управления пользователями
EventService - сервис, предоставляющий доступ для управления событиями
BookingService - сервис, предоставляющий доступ к функциям бронирования событий

# Межсервисное общение

Межсервисное общение происходит на основе системы логирования событий Apache Kafka

## UserService
	
## Установка

    cd EventsAPI\src\UserService\RU.Uncio.UserService.Presentation

## Сборка сервиса

    dotnet build
	
## Запуск сервиса  (https://localhost:7134)

    dotnet run --launch-profile https
	
## Запуск приложения через Swagger (https://localhost:7134/swagger/index.html)

    dotnet run --launch-profile sw_https
	
## Запуск unit tests
	cd ..	
    dotnet test	
	
## Миграции
	dotnet ef migrations add InitialMigration --startup-project ../RU.Uncio.UserService.Presentation --project RU.Uncio.UserService.Infrastructure

## Структура проекта

**Слои архитектуры:**
*   Domain: сущности и модели данных
*   Application: реализация сервисов и DTO, маппинг объектов
*   Infrastructure: реализации репозиториев, DbContext
*   Presentation: реализация контроллеров и эндпойнтов, глобальный обработчик ошибок.

**Диаграмма слоёв:**
    Presentation --> Infrastructure --> Domain: Хранение и Обработка данных
    Presentation --> Application --> Domain: обработка бизнес-случаев

**Слои по папкам:**

├── domain/
│   ├── exceptions/
│   └── models/
├── application/
│   ├── auxiliary/
│   ├── backservices/
│   ├── dto/
│   ├── interfaces/
│   └── services/
├── utils/
│   ├── auxiliary/
│   ├── dataaccess/
│   │   ├── configurations/
│   │   └── AppDbContext
│   ├──  migrations/
│   └── repositories/
├── presentation/
│   ├── auxiliary/
│   ├── middlewares/
│   └── controllers/
└── Program

## RBAC — контроль доступа на основе ролей

**Роли:**
*   Anonimous: анонимный неаутентифицированный пользователь с ограниченными правами доступа
*   User: общий пользователь с ограниченными правами доступа
*   Admin: пользователь с расширенными правами доступа

**Права доступа:**
*   Anonimous: регистрация нового пользователя (по умолчанию роль нового пользователя User), аутентификация
*   User: аналогично Anonimous
*   Admin: аналогично Anonimous и User плюс листинг всех пользователей

## Swagger - получение токена аутентификации
* Login endpoint - POST /User/auth/login
* Скопируйте токен в кнопку Authorize Swagger

## Хранение секрета
* Developement - секрет хранится в файле конфигурации
* Production - переместите секрет в параметры окружения или другое безопасное место хранения секретов

# REST API

REST API сервиса описано ниже.

## Получение списка пользователей (Users)

### Запрос

`GET /users/`

curl -X 'GET' \
  'https://localhost:7134/Users' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer <token>'

### Ответ
{
  "data": [
    {
      "id": "ede5a3d2-34f7-41a1-bcd3-89b5e58340e9",
      "name": "User1234",
      "login": "User1234",
      "password": "",
      "role": 0
    },
    {
      "id": "d6a2b3db-d7da-4f76-bf2b-b1015b889709",
      "name": "string",
      "login": "Admin321@",
      "password": "",
      "role": 1
    }
  ],
  "success": true,
  "statusCode": 200,
  "dateTime": "2026-07-17T12:03:35.7543971Z",
  "message": "Getting all users from DB"
}

## Создание нового пользователя

### Запрос

`POST /Users/auth/register`

curl -X 'POST' \
  'https://localhost:7134/Users/auth/register' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer <token>' \
  -H 'Content-Type: application/json' \
  -d '{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "NewUser",
  "login": "NewUser123",
  "password": "NewUser123",
  "role": 0
}'

### Ответ

{
  "data": {
    "id": "3ba92ea7-d149-4d71-8986-e2894f89eef8",
    "name": "NewUser",
    "login": "NewUser123",
    "password": "",
    "role": 0
  },
  "success": true,
  "statusCode": 201,
  "dateTime": "2026-07-17T12:05:47.2836345Z",
  "message": "User NewUser : NewUser123 added to DB"
}

## Ауиентификация

### Запрос

`POST /Users/auth/login`

curl -X 'POST' \
  'https://localhost:7134/Users/auth/login' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "Admin321@",
  "password": "Admin321@",
  "twoFactorCode": "string",
  "twoFactorRecoveryCode": "string"
}'

### Ответ

{
  "data": "<token>",
  "success": true,
  "statusCode": 200,
  "dateTime": "2026-07-17T12:03:17.8398013Z",
  "message": "User Token"
}

## EventService

## Установка

    cd EventsAPI\src\EventService\RU.Uncio.EventService.Presentation

## Сборка сервиса

    dotnet build
	
## Запуск сервиса  (https://localhost:7134)

    dotnet run --launch-profile https
	
## Запуск сервиса через Swagger (https://localhost:7134/swagger/index.html)

    dotnet run --launch-profile sw_https
	
## Запуск unit tests
	cd ..	
    dotnet test	
	
## Миграции
	dotnet ef migrations add InitialMigration --startup-project ../RU.Uncio.EventService.Presentation --project RU.Uncio.EventService.Infrastructure

## Структура проекта

**Слои архитектуры:**
*   Domain: сущности и модели данных
*   Application: реализация сервисов и DTO, маппинг объектов
*   Infrastructure: реализации репозиториев, DbContext
*   Presentation: реализация контроллеров и эндпойнтов, глобальный обработчик ошибок.

**Диаграмма слоёв:**
    Presentation --> Infrastructure --> Domain: Хранение и Обработка данных
    Presentation --> Application --> Domain: обработка бизнес-случаев

**Слои по папкам:**

├── domain/
│   ├── exceptions/
│   └── models/
├── application/
│   ├── auxiliary/
│   ├── backservices/
│   ├── dto/
│   ├── interfaces/
│   └── services/
├── utils/
│   ├── auxiliary/
│   ├── dataaccess/
│   │   ├── configurations/
│   │   └── AppDbContext
│   ├──  migrations/
│   └── repositories/
├── presentation/
│   ├── auxiliary/
│   ├── middlewares/
│   └── controllers/
└── Program

## RBAC — контроль доступа на основе ролей

**Роли:**
*   Anonimous: анонимный неаутентифицированный пользователь с ограниченными правами доступа
*   User: общий пользователь с ограниченными правами доступа
*   Admin: пользователь с расширенными правами доступа

**Права доступа:**
*   Anonimous: листинг всех событий, получение подробных данных о конкретном событии
*   User: аналогично Anonimous
*   Admin: аналогично Anonimous и User плюс создание/обновление/удаление событий

## Swagger - получение токена аутентификации
* Login endpoint - POST /User/auth/login
* Скопируйте токен в кнопку Authorize Swagger

## Хранение секрета
* Developement - секрет хранится в файле конфигурации
* Production - переместите секрет в параметры окружения или другое безопасное место хранения секретов

# REST API

REST API сервиса описан ниже.

## Получение списка всех событий (Events, пагинация по умолчанию : Page number = 1, PageSize = 10)

### Запрос

`GET /events/`

curl -X 'GET' \
  'https://localhost:7134/Events?page=1&pageSize=10' \
  -H 'accept: application/json'

### Ответ
{
  "data": {
    "items": [],
    "currentItems": 0,
    "currentPage": 1,
    "totalPages": 0,
    "totalItems": 0
  },
  "success": true,
  "statusCode": 200,
  "dateTime": "2026-04-15T13:58:48.935942Z",
  "message": "Gettin paginated events from collection"
}

## Get filtered list of Events (custom paginated: Page number = 2, PageSize = 5)

### Запрос

`GET /events/`

curl -X 'GET' \
  'https://localhost:7134/Events?title=Test&from=2026.01.12&to=2026.01.24&page=2&pageSize=5' \
  -H 'accept: application/json'

### Ответ
{
  "data": {
    "items": [],
    "currentItems": 0,
    "currentPage": 2,
    "totalPages": 0,
    "totalItems": 0
  },
  "success": true,
  "statusCode": 200,
  "dateTime": "2026-04-15T14:01:04.273336Z",
  "message": "Gettin paginated events from collection"
}

## Get a specific Event

### Запрос

`GET /events/id`

    curl -X 'GET' \
		'https://localhost:7134/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
		-H 'accept: application/json'

### Ответ
	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 200,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}
	
## Create a new Event

### Запрос

`POST /events/`

    curl -X 'POST' \
		  'https://localhost:7134/Events' \
		  -H 'accept: text/plain' \
		  -H 'Content-Type: application/json' \
		  -d '{
		  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
		  "title": "string",
		  "description": "string",
		  "startAt": "2026-02-15T12:03:22.941Z",
		  "endAt": "2026-03-31T12:03:22.941Z",
		  "totalSeats": 12
		}'

### Ответ

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 201,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}

## Replace an event

### Запрос

`PUT /events/id`

    curl -X 'PUT' \
	  'https://localhost:7134/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
	  -H 'accept: text/plain' \
	  -H 'Content-Type: application/json' \
	  -d '{
	  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
	  "title": "Test",
	  "description": "Test1",
	  "startAt": "2026-03-31T12:08:36.425Z",
	  "endAt": "2026-04-30T12:08:36.425Z",
		  "totalSeats": 12
		}'

### Ответ

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 204,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}

## Delete an event

### Запрос

`DELETE /events/id`

    curl -X 'DELETE' \
	  'https://localhost:7134/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
	  -H 'accept: text/plain'

### Ответ

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 204,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}

## BookingService

## Установка

    cd EventsAPI\src\BookingService\RU.Uncio.BookingService.Presentation

## Сборка сервиса

    dotnet build
	
## Запуск сервиса  (https://localhost:7134)

    dotnet run --launch-profile https
	
## Запуск сервиса через Swagger (https://localhost:7134/swagger/index.html)

    dotnet run --launch-profile sw_https
	
## Запуск unit tests
	cd ..	
    dotnet test	
	
## Миграции
	dotnet ef migrations add InitialMigration --startup-project ../RU.Uncio.BookingService.Presentation --project RU.Uncio.BookingService.Infrastructure

## Структура проекта

**Слои архитектуры:**
*   Domain: сущности и модели данных
*   Application: реализация сервисов и DTO, маппинг объектов
*   Infrastructure: реализации репозиториев, DbContext
*   Presentation: реализация контроллеров и эндпойнтов, глобальный обработчик ошибок.

**Диаграмма слоёв:**
    Presentation --> Infrastructure --> Domain: Хранение и Обработка данных
    Presentation --> Application --> Domain: обработка бизнес-случаев

**Слои по папкам:**

├── domain/
│   ├── exceptions/
│   └── models/
├── application/
│   ├── auxiliary/
│   ├── backservices/
│   ├── dto/
│   ├── interfaces/
│   └── services/
├── utils/
│   ├── auxiliary/
│   ├── dataaccess/
│   │   ├── configurations/
│   │   └── AppDbContext
│   ├──  migrations/
│   └── repositories/
├── presentation/
│   ├── auxiliary/
│   ├── middlewares/
│   └── controllers/
└── Program

## RBAC — контроль доступа на основе ролей

**Роли:**
*   Anonimous: анонимный неаутентифицированный пользователь с ограниченными правами доступа
*   User: общий пользователь с ограниченными правами доступа
*   Admin: пользователь с расширенными правами доступа

**Права доступа:**
*   Anonimous: нет прав доступа
*   User: бронирование событий, получение собственных бронирований, отмена собственного бронирования
*   Admin: аналогично Anonimous и User плюс отмена любого бронирования

## Swagger - получение токена аутентификации
* Login endpoint - POST /User/auth/login
* Скопируйте токен в кнопку Authorize Swagger

## Хранение секрета
* Developement - секрет хранится в файле конфигурации
* Production - переместите секрет в параметры окружения или другое безопасное место хранения секретов

# REST API

REST API сервиса описан ниже.

## Бронирование события

### Запрос

`POST /events/id/book`

curl -X 'POST' \
  'https://localhost:7134/Events/14c43653-3a34-4faf-b0c4-068928adbe21/book' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>' \
  -d ''

### Ответ

Result JSON-schema
{
  "data": {
    "id": "4828c27c-adcd-4c32-8336-3499a9961449",
    "eventId": "34ad8b51-a6bb-4a9f-8b2e-e5fd07bc855b",
    "status": "Pending",
    "createdAt": "2026-04-27T20:41:28.5650768+03:00",
    "processedAt": null
  },
  "success": true,
  "statusCode": 202,
  "dateTime": "2026-04-27T17:41:28.5690582Z",
  "message": "Adding booking for event with ID 34ad8b51-a6bb-4a9f-8b2e-e5fd07bc855b in collection"
}
	
## Получение бронирования

### Запрос

`GET /bookings/id`

curl -X 'GET' \
  'https://localhost:7134/bookings/4828c27c-adcd-4c32-8336-3499a9961449' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>'

### Ответ

	Result JSON-schema
{
  "data": {
    "id": "4828c27c-adcd-4c32-8336-3499a9961449",
    "eventId": "34ad8b51-a6bb-4a9f-8b2e-e5fd07bc855b",
    "status": "Confirmed",
    "createdAt": "2026-04-27T20:41:28.5650768+03:00",
    "processedAt": "2026-04-27T20:41:33.5845099+03:00"
  },*
  "success": true,
  "statusCode": 200,
  "dateTime": "2026-04-27T17:43:30.5176803Z",
  "message": "Getting booking with ID 4828c27c-adcd-4c32-8336-3499a9961449 from collection"
}	

## Отмена бронирования

### Запрос

`DELETE /bookings/id`

curl -X 'DELETE' \
  'https://localhost:7134/bookings/4c63ead7-86d7-481e-b827-aeb5a52ef74d' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer <token>'

### Ответ

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 204,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}

# Поток данных Kafka - BookingConfirmed

## Топик - "booking-confirmed"

BookingService публикует в топик сообщение о подтверждении бронирования
EventService подписан на событие топика о новых подтвержденных бронированиях и уменьшает количество свободных мест для события

## Порт сервера Kafka

"BootstrapServers": "localhost:9092"

# Кеширование данных Redis - Events Service

## Кеширование данных индивидуальных событий

Данные по индивидуальным событиям кешируются в связи с частым доступом к ним. TTL - 5 мин, данные по событию могут измениться (количество доступных мест), 
устаревание в 5 мин не критично. При обновлении данных кеш также обновляется, при удалении события, кеш по данному событию очищается.

## Кеширование данных TOP 10 событий

Данные по 10 самым востребованным событиям кешируются в связи с частым доступом к ним. TTL - 15 мин, устаревание в 15 мин не критично для выборки.

## Порт сервера Redis (по умолчанию)

"ConnectionString": "localhost:6379"

## При отсутствии доступа к серверу Redis данные берутся непосредственно из репозитория


# Система наблюдаемости

## Инструменты наблюдаемости - Prometheus, Grafana, Jaeger

Для запуска стека мониторинга после запуска сервисов подгрузите dashbord, сохраненный в JSON формате (Events-API-1787738289241.json)
## Prometheus

Ports - 9090
Метрики — инструментация ASP.NET Core (latency, throughput, error rate) и метрики рантайма .NET

## Jaeger

Ports - 16686(UI), 4317(OTLP gRPC)
Трейсы — автоматическая инструментация входящих HTTP-запросов, исходящих HTTP-запросов и запросов EF Core

## Grafana

Ports - 3000
Дашборд экспортированных данных


