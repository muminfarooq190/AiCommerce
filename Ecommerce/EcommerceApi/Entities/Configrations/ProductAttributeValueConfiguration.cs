using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;
internal sealed class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> e)
    {
        e.ToTable("ProductAttributeValues");
        e.HasKey(v => new { v.ProductId, v.AttributeId });

        e.HasOne(v => v.Product).WithMany(p => p.AttributeValues)
                                  .HasForeignKey(v => v.ProductId);
        e.HasOne(v => v.Attribute).WithMany(a => a.Values)
                                  .HasForeignKey(v => v.AttributeId);
    }
}