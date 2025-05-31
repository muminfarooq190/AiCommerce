using Ecommerce.Entities;
using EcommerceApi.Entities;
using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    private readonly Guid? _tenantId;
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantId = tenantProvider.TenantId;

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<UserEntity>().HasQueryFilter(u => u.TenantId == _tenantId || _tenantId == null);
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<PermissionsEntity> UserPermissions { get; set; }
    public DbSet<TenantEntity> Tenants { get; set; }
}