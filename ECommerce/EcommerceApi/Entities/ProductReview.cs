namespace EcommerceApi.Entities
{
    public class ProductReview
    {
        public Guid ReviewId { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public Guid CustomerId { get; set; }
        public byte Rating { get; set; }  // 1-5
        public string? Title { get; set; }
        public string? Body { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool VerifiedPurchase { get; set; } = false;

        public Guid TenantId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
