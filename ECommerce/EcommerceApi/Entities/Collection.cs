namespace EcommerceApi.Entities
{
    public class Collection
    {
        public Guid CollectionId { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public Guid? HeroImageId { get; set; }
        public MediaFile? HeroImage { get; set; }
        public string? BadgeLabel { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<CollectionProduct> Products { get; set; } = new List<CollectionProduct>();
    }
}
