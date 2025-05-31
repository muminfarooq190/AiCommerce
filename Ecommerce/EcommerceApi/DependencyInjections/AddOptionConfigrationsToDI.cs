using Ecommerce.Configurations;

namespace EcommerceApi.DependencyInjections;

public static class AddOptionConfigrationsToDI
{
    public static IServiceCollection AddOptionConfigrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        return services;
    }
}
