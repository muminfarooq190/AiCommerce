namespace EcommerceApi.Entities
{
    public class ProductImage
    {
        public long ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public long MediaFileId { get; set; }
        public MediaFile MediaFile { get; set; } = default!;

        public int SortOrder { get; set; } = 0;
    }
}
