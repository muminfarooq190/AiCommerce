using EcommerceApi.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EcommerceApi.Entities
{
    public class Product
    {
        public Guid ProductId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = default!;
        [MaxLength(80)]
        public string SKU { get; set; } = default!;    // unique
        public ProductStatus Status { get; set; } = ProductStatus.Active;


        public Guid? BrandId { get; set; }
        public Brand? Brand { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();


        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();


        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CompareAtPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostPerItem { get; set; }


        public decimal? WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public bool QualifiesForFreeShipping { get; set; }


        public int StockQuantity { get; set; }
        [MaxLength(120)]
        public string? Barcode { get; set; }
        public bool TrackInventory { get; set; }


        [MaxLength(200)]
        public string? Slug { get; set; }
        [MaxLength(255)]
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        /* ─── audit ─── */
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public Guid TenantId { get; set; }
    }
}
