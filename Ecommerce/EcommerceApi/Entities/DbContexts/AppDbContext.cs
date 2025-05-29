using Ecommerce.Entities;
using EcommerceApi.Configrations;
using EcommerceApi.Entities;
using EcommerceApi.Providers;
using EcommerceApi.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        ConfigureCategory(modelBuilder.Entity<Category>());
        ConfigureMediaFile(modelBuilder.Entity<MediaFile>());
        ConfigureProductCategory(modelBuilder.Entity<ProductCategory>());

        ConfigureBrand(modelBuilder.Entity<Brand>());
        ConfigureProduct(modelBuilder.Entity<Product>());
        ConfigureProductImage(modelBuilder.Entity<ProductImage>());
    }
    private static void ConfigureCategory(EntityTypeBuilder<Category> e)
    {
        e.ToTable("Categories");

        e.HasKey(c => c.CategoryId);

        e.Property(c => c.Name)
         .IsRequired()
         .HasMaxLength(160);

        e.Property(c => c.Slug)
         .IsRequired()
         .HasMaxLength(180);

        e.HasIndex(c => c.Slug).IsUnique();
        e.HasIndex(c => c.ParentId);
        e.HasIndex(c => new { c.Status, c.IsFeatured });

        e.Property(c => c.Status)
         .HasConversion<int>();   // enum→int

        /* parent / children */
        e.HasOne(c => c.Parent)
         .WithMany(p => p.Children)
         .HasForeignKey(c => c.ParentId)
         .OnDelete(DeleteBehavior.SetNull);

        /* featured image */
        e.HasOne(c => c.FeaturedImage)
         .WithMany(m => m.Categories)
         .HasForeignKey(c => c.FeaturedImageId)
         .OnDelete(DeleteBehavior.SetNull);

        /* audit */
        e.Property(c => c.CreatedAtUtc)
         .HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(c => c.UpdatedAtUtc)
         .HasDefaultValueSql("CURRENT_TIMESTAMP")
         .ValueGeneratedOnAddOrUpdate();
       
    }

    /* ─────────────────────────────────────────── */
    private static void ConfigureMediaFile(EntityTypeBuilder<MediaFile> e)
    {
        e.ToTable("MediaFiles");
        e.HasKey(m => m.MediaFileId);

        e.Property(m => m.FileName).IsRequired().HasMaxLength(260);
        e.Property(m => m.Uri).IsRequired();
        e.Property(m => m.UploadedAtUtc)
         .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    /* ─────────────────────────────────────────── */
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
    /* ─────────────────────────────────────── */
    private static void ConfigureBrand(EntityTypeBuilder<Brand> e)
    {
        e.ToTable("Brands");
        e.HasKey(br => br.BrandId);

        e.HasIndex(br => br.Name).IsUnique();        // optional
        e.Property(br => br.CreatedAtUtc)
          .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

    /* ─────────────────────────────────────── */
    private static void ConfigureProduct(EntityTypeBuilder<Product> e)
    {
        e.ToTable("Products");
        e.HasKey(p => p.ProductId);

        e.HasIndex(p => p.SKU).IsUnique();
        e.HasIndex(p => p.Status);
        e.HasIndex(p => p.Slug).IsUnique(false);     // unique per tenant if multi-store

        e.Property(p => p.Status)
          .HasConversion<int>();

        /* foreign keys */
        e.HasOne(p => p.Brand)
          .WithMany(br => br.Products)
          .HasForeignKey(p => p.BrandId)
          .OnDelete(DeleteBehavior.SetNull);

        /* audit */
        e.Property(p => p.CreatedAtUtc)
          .HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(p => p.UpdatedAtUtc)
          .HasDefaultValueSql("CURRENT_TIMESTAMP")
          .ValueGeneratedOnAddOrUpdate();
    }

    /* ─────────────────────────────────────── */
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
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<PermissionsEntity> UserPermissions { get; set; }
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
}