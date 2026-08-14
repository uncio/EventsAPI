using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.BookingService.Application.Backservices;
using RU.Uncio.BookingService.Application.Interfaces;
using RU.Uncio.BookingService.Application.Services;

namespace RU.Uncio.BookingService.Application.Auxiliary
{
    /// <summary>
    /// 
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Репозитории
            services.AddScoped<IBookingService, BookService>();
            services.AddHostedService<BookingBackgroundService>();

            return services;
        }
    }
}
