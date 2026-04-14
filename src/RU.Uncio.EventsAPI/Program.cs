using Microsoft.OpenApi;
using RU.Uncio.EventsAPI.Interfaces;
using RU.Uncio.EventsAPI.Middlewares;
using RU.Uncio.EventsAPI.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Эта опция отключает автоматическую проверку валидации 
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddScoped<IEventsService, EventsService>();
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

app.Run();
