using Sheared.Enums;

namespace EcommerceApi.Entities;

public class OrderStatusHistory : IBaseEntity
{
    public long HistoryId { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public required Guid TenantId { get; set; }
}
