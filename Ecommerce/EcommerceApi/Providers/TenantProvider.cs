using Ecommerce.Entities;
using EcommerceApi.Entities.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceApi.Providers;
public class TenantProvider(IMemoryCache memoryCache, AppDbContext context) : ITenantProvider
{

    public TenantEntity? GetCurrentTenant()
    {
        if (TenantId == null)
        {
            return null;
        }

        string cacheKey = $"Tenants:{TenantId}";
        if (!memoryCache.TryGetValue(cacheKey, out TenantEntity? Tenant))
        {
            Tenant = context.Tenants
            .AsNoTracking()
            .FirstOrDefault(t => t.Id == TenantId);
        }

        return Tenant;
    }
    public Guid? TenantId
    {
        get;
        private set;
    }
    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public void ResetSetTenantId()
    {
        TenantId = null;
    }

    public async Task<TenantEntity?> GetCurrentTenantAsync()
    {
        if (TenantId == null)
        {
            return null;
        }

        string cacheKey = $"Tenants:{TenantId}";
        if (!memoryCache.TryGetValue(cacheKey, out TenantEntity? Tenant))
        {
            Tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == TenantId);
        }

        return Tenant;
    }
}
