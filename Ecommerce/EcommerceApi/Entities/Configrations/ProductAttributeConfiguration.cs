using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> e)
    {
        e.ToTable("ProductAttributes");
        e.HasKey(a => a.AttributeId);
        e.Property(a => a.DataType).HasConversion<int>();
        e.HasIndex(a => new { a.TenantId, a.Slug }).IsUnique();
    }
}
