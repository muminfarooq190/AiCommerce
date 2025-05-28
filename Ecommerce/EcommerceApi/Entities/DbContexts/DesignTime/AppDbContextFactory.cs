using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceApi.Entities.DbContexts.DesignTime;


public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{

    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json") // or "appsettings.Development.json" if needed
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        string CompanyName = configuration.GetSection("DefaultTenant").GetValue<string?>("CompanyName") ?? throw new ArgumentNullException($"DefaultTenant CompanyName is not configured properly in appsetting");


        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton(o => new SchemaProvider(CompanyName))
            .AddSingleton<IConfiguration>(configuration)
            .BuildServiceProvider();

        return new AppDbContext(optionsBuilder.Options, serviceProvider);
    }
}
