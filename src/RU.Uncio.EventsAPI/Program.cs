using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RU.Uncio.EventsAPI;
using RU.Uncio.EventsAPI.DTO;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Middlewares;
using RU.Uncio.EventsAPI.Repositories;
using RU.Uncio.EventsAPI.Services;
using System.Net;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Эта опция отключает автоматическую проверку валидации 
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddScoped<IEventRepository, InMemoryEventRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddHostedService<BookingService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(options =>
{
    // Путь к XML-файлу с документацией
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.MapPost("/{id}/book", async ([FromRoute] Guid id, IEventsService evService, IBookingService service/*, ILogger logger*/) =>
{
    try
    {
        var ev = evService.GetEvent(id);
        if(ev == null)
        {
            //logger.LogError($"Event with ID {id} is not found in the collection");
            return Results.NotFound(new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Event with ID {id} is not found in the collection"
            });
        }
        var result = await service.CreateBookingAsync(id);

        if(result != null)
        {
            var booking = new BookingDTO
            {
                Id = result.Id,
                EventId = result.EventId,
                Status = result.Status.ToString(),
                CreatedAt = result.CreatedAt,
                ProcessedAt = result.ProcessedAt,
            };
            //logger.LogInformation("Booking processed");
            return Results.Accepted(uri: $"/bookings/{booking.Id}", value: new ApiResult<BookingDTO>
            {
                Data = booking,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Getting event with ID {id} from collection"
            });
        }
        else
        {
            return Results.BadRequest(new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = $"Event with ID {id} is not found in the collection"
            });
        }        
    }
    catch (OperationCanceledException)
    {
        //logger.LogWarning("Client Closed Request");
        return Results.StatusCode(499); //Client Closed Request
    }
});

app.MapGet("/bookings/{id}", async ([FromRoute] Guid id, IBookingService service/*, ILogger logger*/) =>
{
    try
    {
        var result = await service.GetBookingByIdAsync(id);
        if(result != null)
        {
            var booking = new BookingDTO
            {
                Id = result.Id,
                EventId = result.EventId,
                Status = result.Status.ToString(),
                CreatedAt = result.CreatedAt,
                ProcessedAt = result.ProcessedAt,
            };
            return Results.Ok(value: new ApiResult<BookingDTO>
            {
                Data = booking,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Getting event with ID {id} from collection"
            });
        }
        else
        {
            return Results.NotFound(new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Event with ID {id} is not found in the collection"
            });
        }
    }
    catch (OperationCanceledException)
    {
        return Results.StatusCode(499); //Client Closed Request
    }
});

app.Run();
