namespace Ecommerce.Entities;

public class TenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
}
