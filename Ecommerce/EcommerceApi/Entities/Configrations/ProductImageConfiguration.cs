using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> e)
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
}
