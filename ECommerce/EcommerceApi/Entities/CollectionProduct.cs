namespace EcommerceApi.Entities
{
    public class CollectionProduct
    {
        public Guid CollectionId { get; set; }
        public Collection Collection { get; set; } = default!;
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = default!;
        public int SortOrder { get; set; }
        public Guid TenantId { get; set; }
    }
}
