using Ecommerce.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceApi.Providers;
public class TenantProvider : ITenantProvider
{
    private readonly IMemoryCache memoryCache;
    private readonly AppDbContext? context;
    public TenantProvider(IMemoryCache memoryCache)
    {
        this.memoryCache = memoryCache;
    }

    public TenantEntity? GetCurrentTenant()
    {
        if (TenantId == null)
        {
            return null;
        }

        string cacheKey = $"Tenants:{TenantId}";
        if (!memoryCache.TryGetValue(cacheKey, out TenantEntity? Tenant))
        {
            if (context != null)
            {
                Tenant = context.Tenants
                    .AsNoTracking()
                    .FirstOrDefault(t => t.Id == TenantId);
            }

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
            if (context != null)
            {
                Tenant = await context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == TenantId);
            }
        }

        return Tenant;
    }
}
