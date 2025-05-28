using Ecommerce.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Entities.DbContexts;

public class TenantDbContext(DbContextOptions<TenantDbContext> options, IServiceProvider serviceProvider) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var Schema = configuration.GetValue<string>("TenantDatabaseSchema") ?? throw new ArgumentNullException("TenantDatabaseSchema is not configrade in Appsetting");
        modelBuilder.HasDefaultSchema(Schema);
    }
    public DbSet<TenantEntity> Tenants { get; set; }
}
