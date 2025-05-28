using EcommerceApi.Providers;

namespace EcommerceApi.Middlewares;

public class TenentExtracterMiddleware(RequestDelegate next, ITenantProvider tenantProvider)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("TenantId", out var tenantId) && Guid.TryParse(tenantId, out var parsedTenantId))
        {
             tenantProvider.SetTenantId(parsedTenantId);
        }
        else
        {
            tenantProvider.ResetSetTenantId();
        }

            await next(context);
    }

}
