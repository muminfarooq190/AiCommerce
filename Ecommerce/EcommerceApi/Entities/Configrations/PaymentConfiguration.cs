using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> e)
    {
        e.HasKey(p => p.PaymentId);
        e.Property(p => p.Method).HasConversion<int>();
        e.Property(p => p.Status).HasConversion<int>();
    }
}
