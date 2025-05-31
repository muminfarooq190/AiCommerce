using Sheared.Enums;

namespace EcommerceApi.Entities;

public class Payment : IBaseEntity
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod Method { get; set; } = PaymentMethod.Card;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? ProviderTxnId { get; set; }
    public string? ProviderPayload { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public required Guid TenantId { get; set; }
}
