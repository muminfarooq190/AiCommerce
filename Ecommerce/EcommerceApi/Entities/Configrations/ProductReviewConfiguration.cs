using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> e)
        {
            e.HasKey(r => r.ReviewId);
            e.Property(r => r.Rating).IsRequired();
            e.HasIndex(r => new { r.ProductId, r.IsApproved });
        }
    }
