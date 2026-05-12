# Events API application

This is a bare-bones example of an application providing a REST
API to a DataMapper-backed model.

## "Install"

    cd EventsAPI\src\RU.Uncio.EventsAPI

## Build the app

    dotnet build
	
## Run the app  (https://localhost:7134)

    dotnet run --launch-profile https
	
## Run the app within Swagger (https://localhost:7134/swagger/index.html)

    dotnet run --launch-profile sw_https
	
## Run unit tests
	cd ..	
    dotnet test

# REST API

The REST API to the app is described below.

## Get list of Events (paginated by default: Page number = 1, PageSize = 10)

### Request

`GET /events/`

curl -X 'GET' \
  'https://localhost:7134/Events?page=1&pageSize=10' \
  -H 'accept: application/json'

### Response
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

### Request

`GET /events/`

curl -X 'GET' \
  'https://localhost:7134/Events?title=Test&from=2026.01.12&to=2026.01.24&page=2&pageSize=5' \
  -H 'accept: application/json'

### Response
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

### Request

`GET /events/id`

    curl -X 'GET' \
		'https://localhost:7134/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
		-H 'accept: application/json'

### Response
	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 200,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}
	
## Create a new Event

### Request

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
		  "endAt": "2026-03-31T12:03:22.941Z"
		}'

### Response

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 201,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}

## Replace an event

### Request

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
	  "endAt": "2026-04-30T12:08:36.425Z"
		}'

### Response

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 204,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}

## Delete an event

### Request

`DELETE /events/id`

    curl -X 'DELETE' \
	  'https://localhost:7134/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
	  -H 'accept: text/plain'

### Response

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 204,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}
	
## Book an event

### Request

`POST /events/id/book`

curl -X 'POST' \
  'https://localhost:7134/Events/34ad8b51-a6bb-4a9f-8b2e-e5fd07bc855b/book' \
  -H 'accept: */*' \
  -d ''

### Response

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
	
## Check an event booking status

### Request

`GET /bookings/id`

    curl -X 'DELETE' \
	  'https://localhost:7134/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
	  -H 'accept: text/plain'

### Response

	Result JSON-schema
{
  "data": {
    "id": "4828c27c-adcd-4c32-8336-3499a9961449",
    "eventId": "34ad8b51-a6bb-4a9f-8b2e-e5fd07bc855b",
    "status": "Confirmed",
    "createdAt": "2026-04-27T20:41:28.5650768+03:00",
    "processedAt": "2026-04-27T20:41:33.5845099+03:00"
  },
  "success": true,
  "statusCode": 200,
  "dateTime": "2026-04-27T17:43:30.5176803Z",
  "message": "Getting booking with ID 4828c27c-adcd-4c32-8336-3499a9961449 from collection"
}
