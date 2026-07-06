using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Application.Services;
using RU.Uncio.EventsAPI.Services;

namespace RU.Uncio.Application.Auxiliary
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Репозитории
            services.AddScoped<IEventsService, EventsService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddHostedService<BookingBackgroundService>();

            return services;
        }
    }
}
