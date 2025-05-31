namespace EcommerceApi.Entities
{
    public class CartItem
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = default!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public int Qty { get; set; }
        public decimal UnitPriceSnap { get; set; }
        public Guid TenantId { get; set; }

        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
