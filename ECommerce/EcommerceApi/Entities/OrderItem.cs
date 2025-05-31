namespace EcommerceApi.Entities;

public class OrderItem : IBaseEntity
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public Guid? ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string ProductName { get; set; } = default!;

    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal LineTotal => (UnitPrice - Discount) * Qty + Tax;

    public decimal? WeightKg { get; set; }
    public string? SnapshotJson { get; set; }   
    public required Guid TenantId { get; set; }
}
