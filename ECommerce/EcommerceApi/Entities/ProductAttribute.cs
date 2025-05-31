using Sheared.Enums;

namespace EcommerceApi.Entities;

public class ProductAttribute : IBaseEntity
{
    public Guid AttributeId { get; set; }
    public required Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public AttributeDataType DataType { get; set; }
    public string? UnitLabel { get; set; }
    public bool IsFilterable { get; set; } = true;
    public bool IsVariation { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
}
