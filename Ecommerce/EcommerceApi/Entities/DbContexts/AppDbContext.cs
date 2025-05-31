using Ecommerce.Entities;
using EcommerceApi.Entities;
using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public sealed class AppDbContext : DbContext
{
    private Guid? TenantId;    

    public AppDbContext(DbContextOptions<AppDbContext> options, IUserProvider tenantProvider) : base(options)
    {
        if (tenantProvider.IsAuthenticated)
        {
            TenantId = tenantProvider.TenantId;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => TenantId == null || u.TenantId == TenantId);
        modelBuilder.Entity<Permission>()
                    .HasQueryFilter(u => TenantId == null || u.TenantId == TenantId);
        modelBuilder.Entity<Cart>()
            .HasQueryFilter(u => TenantId == null || u.TenantId == TenantId);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // Auth / Tenancy
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> UserPermissions => Set<Permission>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // Catalogue
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionProduct> CollectionProducts => Set<CollectionProduct>();

    // Commerce
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<OrderDiscount> OrderDiscounts => Set<OrderDiscount>();

    // Social
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
}
