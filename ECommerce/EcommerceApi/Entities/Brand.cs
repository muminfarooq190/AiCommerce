using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Entities
{
    public class Brand
    {
        public long BrandId { get; set; }
        [MaxLength(120)]
        public string Name { get; set; } = default!;
        public string? Slug { get; set; }           // if you need pretty URLs
        public string? Website { get; set; }

        /* audit */
        public long CreatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
