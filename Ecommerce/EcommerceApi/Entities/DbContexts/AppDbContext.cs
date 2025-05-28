using Ecommerce.Entities;
using EcommerceApi.Configrations;
using EcommerceApi.Entities;
using EcommerceApi.Providers;
using EcommerceApi.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
        var Schema = tenantProvider.GetCurrentTenant()?.SchemaName;
        if (Schema == null)
        {
            var defaultTenant = serviceProvider.GetRequiredService<IOptions<DefaultTenant>>().Value;
            Schema = SchemaGenerater.Generate(defaultTenant.CompanyName);

        }
        modelBuilder.HasDefaultSchema(Schema);
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<PermissionsEntity> UserPermissions { get; set; }
}