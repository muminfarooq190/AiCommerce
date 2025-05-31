using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApi.Entities.Configrations;

internal sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> e)
    {
        e.ToTable("MediaFiles");
        e.HasKey(m => m.MediaFileId);
        e.Property(m => m.FileName).IsRequired().HasMaxLength(260);
        e.Property(m => m.Uri).IsRequired();
        e.Property(m => m.UploadedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
