using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RU.Uncio.Application.DTO;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Application.Auxiliary;
using RU.Uncio.EventsAPI;
using RU.Uncio.EventsAPI.Middlewares;
using RU.Uncio.Infrastructure.Auxiliary;
using RU.Uncio.Infrastructure.DataAccess;
using System.Net;
using System.Reflection;
using RU.Uncio.Presentation.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Эта опция отключает автоматическую проверку валидации 
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //db.Database.EnsureDeleted();
    db.Database.Migrate();
}

//app.UseAuthorization();

app.MapControllers();
app.MapBookingEndpoints(app.Logger);

app.Run();
