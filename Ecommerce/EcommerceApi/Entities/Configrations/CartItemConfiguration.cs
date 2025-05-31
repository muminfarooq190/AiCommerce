using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> ci)
    {

        ci.HasKey(i => i.CartItemId);
        ci.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();
        ci.HasOne(i => i.Cart).WithMany(c => c.Items)
                              .HasForeignKey(i => i.CartId)
                              .OnDelete(DeleteBehavior.Cascade);
    }
}
