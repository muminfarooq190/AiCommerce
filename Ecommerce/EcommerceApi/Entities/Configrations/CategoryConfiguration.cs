using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> e)
    {
        e.ToTable("Categories");
        e.HasKey(c => c.CategoryId);

        e.Property(c => c.Name).IsRequired().HasMaxLength(160);
        e.Property(c => c.Slug).IsRequired().HasMaxLength(180);

        e.HasIndex(c => c.Slug).IsUnique();
        e.HasIndex(c => c.ParentId);
        e.HasIndex(c => new { c.Status, c.IsFeatured });

        e.Property(c => c.Status).HasConversion<int>();

        e.HasOne(c => c.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(c => c.FeaturedImage)
            .WithMany(m => m.Categories)
            .HasForeignKey(c => c.FeaturedImageId)
            .OnDelete(DeleteBehavior.SetNull);

        e.Property(c => c.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        e.Property(c => c.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP")
                                       .ValueGeneratedOnAddOrUpdate();
    }
}
