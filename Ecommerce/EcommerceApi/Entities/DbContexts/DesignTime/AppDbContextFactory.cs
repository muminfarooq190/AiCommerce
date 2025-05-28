using EcommerceApi.DependencyInjections;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceApi.Entities.DbContexts.DesignTime;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
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

        return provider.GetRequiredService<AppDbContext>();
    }
}
