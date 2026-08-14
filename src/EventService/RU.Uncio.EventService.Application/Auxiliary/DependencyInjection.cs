using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.EventService.Application.Interfaces;
using RU.Uncio.EventService.Application.Services;

namespace RU.Uncio.EventService.Application.Auxiliary
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
            services.AddScoped<IEventsService, EventsService>();

            return services;
        }
    }
}
