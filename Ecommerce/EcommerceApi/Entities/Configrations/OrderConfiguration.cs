using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> o)
    {
        o.HasKey(x => x.OrderId);
        o.HasIndex(x => x.OrderNumber).IsUnique();
        o.Property(x => x.Status).HasConversion<int>();

        o.OwnsOne(x => x.ShippingAddress, sa =>
        {
            sa.Property(p => p.Name).HasColumnName("Ship_Name");
            sa.Property(p => p.Line1).HasColumnName("Ship_Line1");
            sa.Property(p => p.Line2).HasColumnName("Ship_Line2");
            sa.Property(p => p.City).HasColumnName("Ship_City");
            sa.Property(p => p.State).HasColumnName("Ship_State");
            sa.Property(p => p.PostalCode).HasColumnName("Ship_PostalCode");
            sa.Property(p => p.Country).HasColumnName("Ship_Country");
        });

        o.OwnsOne(x => x.BillingAddress, ba =>
        {
            ba.Property(p => p.Name).HasColumnName("Bill_Name");
            ba.Property(p => p.Line1).HasColumnName("Bill_Line1");
            ba.Property(p => p.Line2).HasColumnName("Bill_Line2");
            ba.Property(p => p.City).HasColumnName("Bill_City");
            ba.Property(p => p.State).HasColumnName("Bill_State");
            ba.Property(p => p.PostalCode).HasColumnName("Bill_PostalCode");
            ba.Property(p => p.Country).HasColumnName("Bill_Country");
        });
    }
}
