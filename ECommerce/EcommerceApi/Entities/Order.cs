using Sheared.Enums;
using Sheared.Models;

namespace EcommerceApi.Entities
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = default!;
        public Guid TenantId { get; set; }
        public Guid CustomerId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime PlacedAtUtc { get; set; } = DateTime.UtcNow;

        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal GrandTotal => Subtotal - DiscountTotal + ShippingTotal + TaxTotal;


        public Address ShippingAddress { get; set; } = new();
        public Address BillingAddress { get; set; } = new();

        /* navs */
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

        /* audit */
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
