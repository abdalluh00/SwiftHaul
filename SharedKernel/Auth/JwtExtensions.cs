using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Settings;
using System.Text;

namespace SharedKernel.Auth
{
    //Setup JWT authentication and authorization services
    public static class JwtExtensions
    {
        public static IServiceCollection AddSharedJwt(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>()!;

            services.Configure<JwtSettings>(
                configuration.GetSection("JwtSettings"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
