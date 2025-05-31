using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class OrderDiscountConfiguration : IEntityTypeConfiguration<OrderDiscount>
{
    public void Configure(EntityTypeBuilder<OrderDiscount> e)
    {
        e.HasKey(od => new { od.OrderId, od.DiscountId });
        e.HasOne(od => od.Order).WithMany(o => o.OrderDiscounts)
                                .HasForeignKey(od => od.OrderId);
        e.HasOne(od => od.Discount).WithMany(d => d.Orders)
                                   .HasForeignKey(od => od.DiscountId);
    }
}
