using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

/*── Discounts ─*/
internal sealed class DiscountConfiguration : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> e)
        {
            e.HasKey(d => d.DiscountId);
            e.Property(d => d.Type).HasConversion<int>();
            e.HasIndex(d => new { d.TenantId, d.Code }).IsUnique();
        }
    }
