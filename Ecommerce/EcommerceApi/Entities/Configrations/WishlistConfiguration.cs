using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class WishlistConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> e)
    {
        e.HasKey(w => w.WishlistItemId);
        e.HasIndex(w => new { w.CustomerId, w.ProductId }).IsUnique();

    }
}
