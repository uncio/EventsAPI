using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.BookingService.Application.Interfaces;
using RU.Uncio.BookingService.Infrastructure.DataAccess;
using RU.Uncio.Infrastructure.Repositories;

namespace RU.Uncio.BookingService.Infrastructure.Auxiliary
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
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Репозитории
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddSingleton<IBookingProducer, BookingProducer>();

            return services;
        }
    }
}
