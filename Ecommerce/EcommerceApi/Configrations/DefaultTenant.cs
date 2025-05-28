namespace EcommerceApi.Configrations;

public class DefaultTenant
{
    public required Guid TenantId { get; set; }
    public required string CompanyName { get; set; }
}
