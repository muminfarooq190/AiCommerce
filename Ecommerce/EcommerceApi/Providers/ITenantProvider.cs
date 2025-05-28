using Ecommerce.Entities;

namespace EcommerceApi.Providers;
public interface ITenantProvider
{
    public TenantEntity? GetCurrentTenant();
    public void SetTenantId(Guid tenantId);
    public void ResetSetTenantId();
    Guid? TenantId { get; }
}