namespace EcommerceApi.Entities;

public class ProductCategory : IBaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public required Guid TenantId { get; set; }
}
