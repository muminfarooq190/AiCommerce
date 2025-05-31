namespace EcommerceApi.Entities
{
    public class OrderDiscount
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public Guid DiscountId { get; set; }
        public Discount Discount { get; set; } = default!;
        public decimal AmountApplied { get; set; }
    }
}
