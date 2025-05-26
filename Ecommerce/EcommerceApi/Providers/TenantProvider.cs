namespace EcommerceApi.Providers;
public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;

            if (context != null && context.Request.Headers.TryGetValue("TenantId", out var tenantId) && Guid.TryParse(tenantId, out var parsedTenantId))
            {
                return parsedTenantId;
            }           

            return null;
        }
    }
}
