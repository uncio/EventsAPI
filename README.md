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

# REST API

The REST API to the app is described below.

## Get list of Events

### Request

`GET /events/`

    curl -X 'GET' \ 
		'https://localhost:7134/api/Events' \ 
		-H 'accept: application/json'

### Response
	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 200,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}

## Get a specific Event

### Request

`GET /events/id`

    curl -X 'GET' \
		'https://localhost:7134/api/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
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
		  'https://localhost:7134/api/Events' \
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
	  'https://localhost:7134/api/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
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
	  'https://localhost:7134/api/Events/3fa85f64-5717-4562-b3fc-2c963f66afa6' \
	  -H 'accept: text/plain'

### Response

	Result JSON-schema
	{
	  "success": true,
	  "statusCode": 204,
	  "dateTime": "2026-03-31T11:54:40.113Z",
	  "message": "string"
	}
