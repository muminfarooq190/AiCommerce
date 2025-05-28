using Ecommerce.Entities;
using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Entities.DbContexts;

public class TenantDbContext(DbContextOptions<TenantDbContext> options , IServiceProvider serviceProvider) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var schemaProvider = serviceProvider.GetRequiredService<SchemaProvider>();        
        modelBuilder.HasDefaultSchema(schemaProvider.Schema);
    }
    public DbSet<TenantEntity> Tenants { get; set; }
}
