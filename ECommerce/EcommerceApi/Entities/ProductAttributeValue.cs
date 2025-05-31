namespace EcommerceApi.Entities
{
    public class ProductAttributeValue
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public Guid AttributeId { get; set; }
        public ProductAttribute Attribute { get; set; } = default!;

        public string? ValueString { get; set; }
        public decimal? ValueNumber { get; set; }
        public bool? ValueBool { get; set; }

        public Guid TenantId { get; set; }
    }
}
