using Ecommerce.Entities;
using EcommerceApi.Entities;
using EcommerceApi.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class AppDbContext : DbContext
{
    private readonly Guid? _tenantId;
    public AppDbContext(DbContextOptions<AppDbContext> options, IUserProvider tenantProvider) : base(options)
    {
        if(tenantProvider.IsAuthenticated)
        {
            _tenantId = tenantProvider.TenantId;
        }     

    /*──────────────────────── ON-MODEL-CREATING ───────────────────*/
    protected override void OnModelCreating(ModelBuilder mb)
    {
        /* global tenant filter on users */
        mb.Entity<UserEntity>()
          .HasQueryFilter(u => _tenantId == null || u.TenantId == _tenantId);

        /* register every aggregate-level configurator */
        ConfigureCatalog(mb);
        ConfigureOrderDomain(mb);
        ConfigureCartDomain(mb);
        ConfigureWishlist(mb);
        ConfigureReviews(mb);
        ConfigureDynamicAttributes(mb);
        ConfigureCollections(mb);
        ConfigureDiscounts(mb);
    }

    /*──────────────────────── CATALOG DOMAIN ─────────────────────*/
    private static void ConfigureCatalog(ModelBuilder mb)
    {
        ConfigureCategory(mb.Entity<Category>());
        ConfigureMediaFile(mb.Entity<MediaFile>());
        ConfigureBrand(mb.Entity<Brand>());
        ConfigureProduct(mb.Entity<Product>());
        ConfigureProductImage(mb.Entity<ProductImage>());
        ConfigureProductCategory(mb.Entity<ProductCategory>());
    }

    private static void ConfigureCategory(EntityTypeBuilder<Category> e)
    {
        e.ToTable("Categories");
        e.HasKey(c => c.CategoryId);

        e.Property(c => c.Name).IsRequired().HasMaxLength(160);
        e.Property(c => c.Slug).IsRequired().HasMaxLength(180);

        e.HasIndex(c => c.Slug).IsUnique();
        e.HasIndex(c => c.ParentId);
        e.HasIndex(c => new { c.Status, c.IsFeatured });

        e.Property(c => c.Status).HasConversion<int>();

        e.HasOne(c => c.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(c => c.FeaturedImage)
            .WithMany(m => m.Categories)
            .HasForeignKey(c => c.FeaturedImageId)
            .OnDelete(DeleteBehavior.SetNull);

        e.Property(c => c.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(c => c.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP")
                                       .ValueGeneratedOnAddOrUpdate();
    }

    private static void ConfigureMediaFile(EntityTypeBuilder<MediaFile> e)
    {
        e.ToTable("MediaFiles");
        e.HasKey(m => m.MediaFileId);

        e.Property(m => m.FileName).IsRequired().HasMaxLength(260);
        e.Property(m => m.Uri).IsRequired();
        e.Property(m => m.UploadedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigureBrand(EntityTypeBuilder<Brand> e)
    {
        e.ToTable("Brands");
        e.HasKey(br => br.BrandId);

        e.HasIndex(br => br.Name).IsUnique();
        e.Property(br => br.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private static void ConfigureProduct(EntityTypeBuilder<Product> e)
    {
        e.ToTable("Products");
        e.HasKey(p => p.ProductId);

        e.HasIndex(p => p.SKU).IsUnique();
        e.HasIndex(p => p.Status);
        e.HasIndex(p => p.Slug).IsUnique(false);

        e.Property(p => p.Status).HasConversion<int>();

        e.HasOne(p => p.Brand)
            .WithMany(br => br.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        e.Property(p => p.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(p => p.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP")
                                       .ValueGeneratedOnAddOrUpdate();
    }

    private static void ConfigureProductImage(EntityTypeBuilder<ProductImage> e)
    {
        e.ToTable("ProductImages");
        e.HasKey(pi => new { pi.ProductId, pi.MediaFileId });

        e.Property(pi => pi.SortOrder).HasDefaultValue(0);

        e.HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(pi => pi.MediaFile)
            .WithMany()
            .HasForeignKey(pi => pi.MediaFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureProductCategory(EntityTypeBuilder<ProductCategory> e)
    {
        e.ToTable("ProductCategories");
        e.HasKey(pc => new { pc.ProductId, pc.CategoryId });

        e.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    /*──────────────────────── ORDER DOMAIN ───────────────────────*/
    private static void ConfigureOrderDomain(ModelBuilder mb)
    {
        ConfigureOrder(mb.Entity<Order>());
        mb.Entity<OrderItem>().HasKey(i => i.OrderItemId);
        ConfigurePayment(mb.Entity<Payment>());
        ConfigureShipment(mb.Entity<Shipment>());
        ConfigureOrderStatusHistory(mb.Entity<OrderStatusHistory>());
        ConfigureOrderDiscount(mb.Entity<OrderDiscount>());
    }

    private static void ConfigureOrder(EntityTypeBuilder<Order> o)
    {
        o.HasKey(x => x.OrderId);
        o.HasIndex(x => x.OrderNumber).IsUnique();
        o.Property(x => x.Status).HasConversion<int>();

        o.OwnsOne(x => x.ShippingAddress, sa =>
        {
            sa.Property(p => p.Name).HasColumnName("Ship_Name");
            sa.Property(p => p.Line1).HasColumnName("Ship_Line1");
            sa.Property(p => p.Line2).HasColumnName("Ship_Line2");
            sa.Property(p => p.City).HasColumnName("Ship_City");
            sa.Property(p => p.State).HasColumnName("Ship_State");
            sa.Property(p => p.PostalCode).HasColumnName("Ship_PostalCode");
            sa.Property(p => p.Country).HasColumnName("Ship_Country");
        });

        o.OwnsOne(x => x.BillingAddress, ba =>
        {
            ba.Property(p => p.Name).HasColumnName("Bill_Name");
            ba.Property(p => p.Line1).HasColumnName("Bill_Line1");
            ba.Property(p => p.Line2).HasColumnName("Bill_Line2");
            ba.Property(p => p.City).HasColumnName("Bill_City");
            ba.Property(p => p.State).HasColumnName("Bill_State");
            ba.Property(p => p.PostalCode).HasColumnName("Bill_PostalCode");
            ba.Property(p => p.Country).HasColumnName("Bill_Country");
        });
    }

    private static void ConfigurePayment(EntityTypeBuilder<Payment> e)
    {
        e.HasKey(p => p.PaymentId);
        e.Property(p => p.Method).HasConversion<int>();
        e.Property(p => p.Status).HasConversion<int>();
    }

    private static void ConfigureShipment(EntityTypeBuilder<Shipment> e)
    {
        e.HasKey(s => s.ShipmentId);
        e.Property(s => s.Status).HasConversion<int>();
    }

    private static void ConfigureOrderStatusHistory(EntityTypeBuilder<OrderStatusHistory> e)
    {
        e.ToTable("OrderStatusHistory");
        e.HasKey(h => h.HistoryId);
    }

    private static void ConfigureOrderDiscount(EntityTypeBuilder<OrderDiscount> e)
    {
        e.HasKey(od => new { od.OrderId, od.DiscountId });
        e.HasOne(od => od.Order).WithMany(o => o.OrderDiscounts)
                                .HasForeignKey(od => od.OrderId);
        e.HasOne(od => od.Discount).WithMany(d => d.Orders)
                                   .HasForeignKey(od => od.DiscountId);
    }

    /*──────────────────────── CART DOMAIN ─────────────────────────*/
    private static void ConfigureCartDomain(ModelBuilder mb)
    {
        var cart = mb.Entity<Cart>();
        cart.HasKey(c => c.CartId);
        cart.Property(c => c.Status).HasConversion<int>();
        cart.HasIndex(c => c.CustomerId);

        var ci = mb.Entity<CartItem>();
        ci.HasKey(i => i.CartItemId);
        ci.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();
        ci.HasOne(i => i.Cart).WithMany(c => c.Items)
                              .HasForeignKey(i => i.CartId)
                              .OnDelete(DeleteBehavior.Cascade);
    }

    /*──────────────────────── WISHLIST ───────────────────────────*/
    private static void ConfigureWishlist(ModelBuilder mb)
    {
        var wl = mb.Entity<WishlistItem>();
        wl.HasKey(w => w.WishlistItemId);
        wl.HasIndex(w => new { w.CustomerId, w.ProductId }).IsUnique();
    }

    /*──────────────────────── REVIEWS ────────────────────────────*/
    private static void ConfigureReviews(ModelBuilder mb)
    {
        var pr = mb.Entity<ProductReview>();
        pr.HasKey(r => r.ReviewId);
        pr.Property(r => r.Rating).IsRequired();
        pr.HasIndex(r => new { r.ProductId, r.IsApproved });
    }

    /*──────────────────────── DYNAMIC ATTRIBUTES ─────────────────*/
    private static void ConfigureDynamicAttributes(ModelBuilder mb)
    {
        var pa = mb.Entity<ProductAttribute>();
        pa.HasKey(a => a.AttributeId);
        pa.Property(a => a.DataType).HasConversion<int>();
        pa.HasIndex(a => new { a.TenantId, a.Slug }).IsUnique();

        var pav = mb.Entity<ProductAttributeValue>();
        pav.HasKey(v => new { v.ProductId, v.AttributeId });
        pav.HasOne(v => v.Product).WithMany(p => p.AttributeValues)
                                  .HasForeignKey(v => v.ProductId);
        pav.HasOne(v => v.Attribute).WithMany(a => a.Values)
                                    .HasForeignKey(v => v.AttributeId);
    }

    /*──────────────────────── COLLECTIONS ────────────────────────*/
    private static void ConfigureCollections(ModelBuilder mb)
    {
        var coll = mb.Entity<Collection>();
        coll.HasKey(c => c.CollectionId);
        coll.HasIndex(c => new { c.TenantId, c.Slug }).IsUnique();
        coll.HasOne(c => c.HeroImage).WithMany()
                                     .HasForeignKey(c => c.HeroImageId)
                                     .OnDelete(DeleteBehavior.SetNull);

        var cp = mb.Entity<CollectionProduct>();
        cp.HasKey(k => new { k.CollectionId, k.ProductId });
        cp.HasOne(k => k.Collection).WithMany(c => c.Products)
                                   .HasForeignKey(k => k.CollectionId);
        cp.HasOne(k => k.Product).WithMany(p => p.CollectionProducts)
                                 .HasForeignKey(k => k.ProductId);
    }

    /*──────────────────────── DISCOUNTS ─────────────────────────*/
    private static void ConfigureDiscounts(ModelBuilder mb)
    {
        var d = mb.Entity<Discount>();
        d.HasKey(x => x.DiscountId);
        d.Property(x => x.Type).HasConversion<int>();
        d.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }

    /*──────────────────────── DB SETS ───────────────────────────*/
    // Auth / Tenancy
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PermissionsEntity> UserPermissions => Set<PermissionsEntity>();
    public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

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
