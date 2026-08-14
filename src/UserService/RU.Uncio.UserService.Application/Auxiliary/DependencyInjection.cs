using Microsoft.Extensions.DependencyInjection;
using RU.Uncio.UserService.Application.Interfaces;
using RU.Uncio.UserService.Application.Services;

namespace RU.Uncio.UserService.Application.Auxiliary
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
            services.AddScoped<IUserService, UsersService>();

            return services;
        }
    }
}
