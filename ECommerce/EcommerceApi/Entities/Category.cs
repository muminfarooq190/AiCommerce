using EcommerceApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.Text.Json;

namespace EcommerceApi.Entities
{
    public class Category
    {
        public long CategoryId { get; set; }

        /* ─── hierarchy ─── */
        public long? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        /* ─── display meta ─── */
        public string? IconClass { get; set; }    // e.g. "fa-tag"
        public string? ColorTheme { get; set; }    // e.g. "Blue"

        public long? FeaturedImageId { get; set; }
        public MediaFile? FeaturedImage { get; set; }

        /* ─── merchandising ─── */
        public CategoryStatus Status { get; set; } = CategoryStatus.Active;
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }

        /* ─── SEO ─── */
        [MaxLength(180)]
        public string Slug { get; set; } = default!;
        [MaxLength(255)]
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        /* ─── audit ─── */
        public long CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;



        /* ─── M:M with product ─── */
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }

}
