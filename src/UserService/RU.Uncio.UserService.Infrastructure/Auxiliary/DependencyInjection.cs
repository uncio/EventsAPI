using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.Infrastructure.Repositories;
using RU.Uncio.UserService.Application.Interfaces;
using RU.Uncio.UserService.Infrastructure.DataAccess;

namespace RU.Uncio.UserService.Infrastructure.Auxiliary
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
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
