using Ecommerce.Services;
using Ecommerce.Utilities;

namespace EcommerceApi;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection service)
    {
        service.AddSingleton<JwtTokenGenerator>();
        service.AddSingleton<EmailSender>();

        return service;
    }
}
