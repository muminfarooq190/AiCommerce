using EcommerceApi.Entities.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.DependencyInjections;

public static class AddDatabasesToDI
{
    public static IServiceCollection AddDatabses(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection is not configured properly in appsettings");

        AddAppDbContext(services, connection);
        AddTenantDbContext(services, connection);

        return services;
    }
    private static void AddAppDbContext(IServiceCollection services, string connection)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

        services.AddScoped(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
            return new AppDbContext(options, serviceProvider);
        });
    }
    private static void AddTenantDbContext(IServiceCollection services, string connection)
    {
        services.AddDbContext<TenantDbContext>(options => options.UseSqlServer(connection));

        services.AddScoped(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<TenantDbContext>>();
            return new TenantDbContext(options, serviceProvider);
        });

    }
}
