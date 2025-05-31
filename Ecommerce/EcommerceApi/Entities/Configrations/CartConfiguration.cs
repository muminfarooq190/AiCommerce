using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

/*────────────────────  CART & SOCIAL  ─────────────────────*/
internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> cart)
    {
        cart.HasKey(c => c.CartId);
        cart.Property(c => c.Status).HasConversion<int>();
        cart.HasIndex(c => c.CustomerId);
    }
}
