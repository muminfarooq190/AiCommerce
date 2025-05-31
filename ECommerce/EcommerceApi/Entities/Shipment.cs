using Sheared.Enums;

namespace EcommerceApi.Entities
{
    public class Shipment
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;

        public string CarrierCode { get; set; } = default!;
        public string ServiceLevel { get; set; } = default!;
        public string? TrackingNumber { get; set; }
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
        public DateTime? ShippedAtUtc { get; set; }
        public DateTime? DeliveredAtUtc { get; set; }
        public decimal ShippingCost { get; set; }

        public Guid TenantId { get; set; }
    }
}
