using EcommerceApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Entities;

public class Category : IBaseEntity
{
    public Guid CategoryId { get; set; }

    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public string? IconClass { get; set; }    // e.g. "fa-tag"
    public string? ColorTheme { get; set; }    // e.g. "Blue"

    public Guid? FeaturedImageId { get; set; }
    public MediaFile? FeaturedImage { get; set; }

    public CategoryStatus Status { get; set; } = CategoryStatus.Active;
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    [MaxLength(180)]
    public string Slug { get; set; } = default!;
    [MaxLength(255)]
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    public required Guid TenantId { get; set; }
}
