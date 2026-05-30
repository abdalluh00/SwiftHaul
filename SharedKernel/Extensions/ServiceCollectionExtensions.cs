using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Auth;
using System.Reflection;

namespace SharedKernel.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedJwt(
            this IServiceCollection services,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // JWT already in JwtExtensions — keep this for backward compatibility
            JwtExtensions.AddSharedJwt(services, configuration);
            return services;
        }

        
        public static IServiceCollection AddCQRS(
            this IServiceCollection services,
            Assembly assembly)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            return services;
        }
    }
}
