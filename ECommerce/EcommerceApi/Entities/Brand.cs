using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Entities
{
    public class Brand
    {
        public Guid BrandId { get; set; }
        [MaxLength(120)]
        public string Name { get; set; } = default!;
        public string? Slug { get; set; }           
        public string? Website { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public required Guid TenantId { get; set; }
    }
}
