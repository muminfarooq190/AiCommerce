using EcommerceApi.DependencyInjections;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceApi.Entities.DbContexts.DesignTime;
public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        IServiceProvider provider =
                new ServiceCollection()
                .AddDatabses(configuration)
                .AddSingleton<IConfiguration>(configuration)
                .AddOptionConfigrations(configuration)
                .AddDependencies(configuration)
                .BuildServiceProvider();

        return provider.GetRequiredService<TenantDbContext>();
    }
}

