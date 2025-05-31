using EcommerceApi.Entities;

namespace Ecommerce.Entities;

public class Tenant : IBaseEntity
{
    private Tenant() { }

    public required Guid TenantId { get; set; }
    public required string CompanyName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static Tenant Create(string companyName)
    {
        return new Tenant
        {
            TenantId = Guid.NewGuid(),
            CompanyName = companyName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    public static Tenant Set(Guid guid, string companyName, DateTime CreatedAt, DateTime updatedAt)
    {
        return new Tenant
        {
            TenantId = Guid.NewGuid(),
            CompanyName = companyName,
            CreatedAt = CreatedAt,
            UpdatedAt = updatedAt
        };
    }
}
