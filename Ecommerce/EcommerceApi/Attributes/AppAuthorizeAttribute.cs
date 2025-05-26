using Ecommerce.Entities;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace EcommerceApi.Attributes;

public class AppAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string? _permission;

    public AppAuthorizeAttribute(string permission)
    {
        _permission = permission;
    }
    public AppAuthorizeAttribute(){    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new ForbidResult();
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new ForbidResult("Bearer");
            return;
        }

        if (!Guid.TryParse(userId,out Guid guidUserid))
        {
            context.Result = new ForbidResult("Bearer");
            return;
        }

        if (string.IsNullOrEmpty(_permission))
        {
            return;
        }

        var db = httpContext.RequestServices.GetRequiredService<AppDbContext>();
        var tenent = httpContext.RequestServices.GetRequiredService<ITenantProvider>();
        var cache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();

        string cacheKey = $"perm:{userId}";

        List<string>? permissions;

        if (!cache.TryGetValue(cacheKey, out permissions))
        {
            permissions = await db.UserPermissions
                .Where(p => p.UserId == guidUserid && p.User!.TenantId == tenent.TenantId)
                .Select(p => p.Name)
                .ToListAsync();

            // Cache the permissions for 10 minutes
            cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));
        }

        if (permissions == null || !permissions.Contains(_permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult("Bearer");
        }
    }
}
