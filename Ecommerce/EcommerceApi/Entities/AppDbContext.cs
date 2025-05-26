using EcommerceApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TenantEntity> Tenant { get; set; }
    public DbSet<PermissionsEntity> UserPermissions { get; internal set; }
}
