using Microsoft.EntityFrameworkCore;

namespace EcommerceWeb.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TenantConfigEntity> TenantConfigs { get; set; } = null!;

}
