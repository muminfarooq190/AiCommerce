using EcommerceApi.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Entities;

[Index(nameof(CompanyName), IsUnique = true)]
public class TenantEntity
{
    private TenantEntity() { }

    public Guid Id { get; init; }
    public required string CompanyName { get; init; }
    public required string SchemaName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static TenantEntity Create(string companyName)
    {
        return new TenantEntity
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            SchemaName = SchemaGenerater.Generate(companyName),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    public static TenantEntity Set(Guid guid, string companyName, DateTime CreatedAt, DateTime updatedAt)
    {
        return new TenantEntity
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            SchemaName = SchemaGenerater.Generate(companyName),
            CreatedAt = CreatedAt,
            UpdatedAt = updatedAt
        };
    }
}
