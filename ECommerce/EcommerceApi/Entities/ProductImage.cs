namespace EcommerceApi.Entities
{
    public class ProductImage
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public Guid MediaFileId { get; set; }
        public MediaFile MediaFile { get; set; } = default!;

        public int SortOrder { get; set; } = 0;

        public required Guid TenantId { get; set; }
    }
}
