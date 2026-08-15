using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Infrastructure.DataAccess;
using RU.Uncio.EventsService.Infrastructure;
using RU.Uncio.Infrastructure.Repositories;
using StackExchange.Redis;
using System.Collections.ObjectModel;

namespace RU.Uncio.EventService.Infrastructure.Auxiliary
{
    /// <summary>
    /// Infrastructure dependency injection
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // База данных
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.EnableRetryOnFailure(3, TimeSpan.FromMilliseconds(30), new List<string>())));

            // Репозитории
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddHostedService<BookingConsumer>();
            services.AddHostedService<TopicCreatorService>();

            var redisServer = configuration.GetSection("Redis:BootstrapServers").Value;
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisServer!));

            return services;
        }
    }
}
