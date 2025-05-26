using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Providers;

namespace EcommerceApi;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection service)
    {
        service.AddSingleton<JwtTokenGenerator>();
        service.AddSingleton<EmailSender>();
        service.AddScoped<ITenantProvider, TenantProvider>();

        return service;
    }
}
