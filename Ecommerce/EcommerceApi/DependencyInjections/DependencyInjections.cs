using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Providers;

namespace EcommerceApi.DependencyInjections;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionConfigrations(configuration);
        services.AddDatabases(configuration);
        services.AddMemoryCache();
        services.AddAppAuthorization();
        services.AddSingleton<JwtTokenGenerator>();
        services.AddSingleton<EmailSender>();
        services.AddSingleton<ITenantProvider, TenantProvider>();

        return services;
    }


}
