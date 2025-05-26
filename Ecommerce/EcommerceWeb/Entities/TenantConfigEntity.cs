namespace EcommerceWeb.Entities;

public class TenantConfigEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid TenantId { get; set; }
    public string? Url { get; set; }
    public required string TempUrlPrefix { get; set; }

}
