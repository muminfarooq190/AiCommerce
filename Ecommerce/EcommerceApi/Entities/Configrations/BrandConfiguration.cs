using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> e)
    {
        e.ToTable("Brands");
        e.HasKey(br => br.BrandId);

        e.HasIndex(br => br.Name).IsUnique();
        e.Property(br => br.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}