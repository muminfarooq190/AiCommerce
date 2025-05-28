using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceApi.Entities.DbContexts.DesignTime;
public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{

    public TenantDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json") // or "appsettings.Development.json" if needed
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        string Sceama = configuration.GetValue<string?>("TenantDabaseSchema") ?? throw new ArgumentNullException($"TenantDabaseSchema is not configured properly in appsetting");

        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton(o => new SchemaProvider(Sceama))
            .AddSingleton<IConfiguration>(configuration)
            .BuildServiceProvider();

        return new TenantDbContext(optionsBuilder.Options, serviceProvider);
    }
}

