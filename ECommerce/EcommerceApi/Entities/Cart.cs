using Sheared.Enums;

namespace EcommerceApi.Entities
{
    public class Cart
    {
        public Guid CartId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? CustomerId { get; set; }
        public CartStatus Status { get; set; } = CartStatus.Active;
        public DateTime? ExpiresAtUtc { get; set; }

        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
