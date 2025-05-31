using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class CollectionProductConfiguration : IEntityTypeConfiguration<CollectionProduct>
{
    public void Configure(EntityTypeBuilder<CollectionProduct> e)
    {
        e.ToTable("CollectionProducts");
       
        e.HasKey(k => new { k.CollectionId, k.ProductId });
        e.HasOne(k => k.Collection).WithMany(c => c.Products)
                                   .HasForeignKey(k => k.CollectionId);
        e.HasOne(k => k.Product).WithMany(p => p.CollectionProducts)
                                 .HasForeignKey(k => k.ProductId);
    }
}
