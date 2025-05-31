using Sheared.Enums;

namespace EcommerceApi.Entities;

public class Discount : IBaseEntity
{
    public Guid DiscountId { get; set; }
    public required Guid TenantId { get; set; }
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public DiscountType Type { get; set; }
    public decimal Value { get; set; }
    public int? MaxUses { get; set; }
    public int UsesSoFar { get; set; }
    public decimal? MinOrderTotal { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OrderDiscount> Orders { get; set; } = new List<OrderDiscount>();
}
