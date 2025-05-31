using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> e)
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
}