using EcommerceApi.Entities.DbContexts;
using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.DependencyInjections;

public static class AddDatabasesToDI
{
    public static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection is not configured properly in appsettings");

        AddAppDbContext(services, connection);

        return services;
    }
    private static void AddAppDbContext(IServiceCollection services, string connection)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

        services.AddScoped(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
            var _tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            return new AppDbContext(options, _tenantProvider);
        });
    }
    
}
