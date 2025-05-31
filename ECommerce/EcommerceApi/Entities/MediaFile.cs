namespace EcommerceApi.Entities;

public class MediaFile : IBaseEntity
{
    public Guid MediaFileId { get; set; }
    public string FileName { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public string Uri { get; set; } = default!;   // S3 / local / CDN
    public long UploadedBy { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public required Guid TenantId { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
