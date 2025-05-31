using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> e)
    {
        e.ToTable("Collections");
        e.HasKey(c => c.CollectionId);
        e.HasIndex(c => new { c.TenantId, c.Slug }).IsUnique();

        e.HasOne(c => c.HeroImage)
          .WithMany()
          .HasForeignKey(c => c.HeroImageId)
          .OnDelete(DeleteBehavior.SetNull);
    }
}
