using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.Application.Backservices;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Application.Services;

namespace RU.Uncio.Application.Auxiliary
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
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEventsService, EventsService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddHostedService<BookingBackgroundService>();

            return services;
        }
    }
}
