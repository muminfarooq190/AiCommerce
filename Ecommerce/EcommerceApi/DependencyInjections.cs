using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Providers;
using EcommerceApi.Utilities;

namespace EcommerceApi;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection service)
    {
        service.AddSingleton<SchemaCloner>();
        service.AddSingleton<JwtTokenGenerator>();
        service.AddSingleton<EmailSender>();
        service.AddSingleton<ITenantProvider, TenantProvider>();

        return service;
    }
}
