namespace EcommerceApi.Entities
{
    public class ProductCategory
    {
        public long ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public long CategoryId { get; set; }
        public Category Category { get; set; } = default!;
    }
}
