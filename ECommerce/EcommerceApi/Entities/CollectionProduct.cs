namespace EcommerceApi.Entities;

public class CollectionProduct : IBaseEntity
{
    public Guid CollectionId { get; set; }
    public Collection Collection { get; set; } = default!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public int SortOrder { get; set; }
    public required Guid TenantId { get; set; }
}
