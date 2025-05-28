using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Providers;
using EcommerceApi.Utilities;

namespace EcommerceApi.DependencyInjections;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionConfigrations(configuration);
        services.AddDatabses(configuration);
        services.AddMemoryCache();
        services.AddAppAuthorization();
        services.AddSingleton<SchemaCloner>();
        services.AddSingleton<JwtTokenGenerator>();
        services.AddSingleton<EmailSender>();
        services.AddSingleton<ITenantProvider, TenantProvider>();
        services.AddTransient<SchemaGenerater>();

        return services;
    }


}
