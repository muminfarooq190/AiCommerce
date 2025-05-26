using Ecommerce.Entities;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace EcommerceApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AppAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string? _permission;

    public AppAuthorizeAttribute(string permission)
    {
        _permission = permission;
    }

    public AppAuthorizeAttribute() { }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;
        
        if (!IsUserAuthenticated(user, out Guid userId))
        {
            context.Result = BuildUnauthorizedResult();
            return;
        }

        if (string.IsNullOrWhiteSpace(_permission))
            return;

        var dbContext = httpContext.RequestServices.GetRequiredService<AppDbContext>();
        var tenantProvider = httpContext.RequestServices.GetRequiredService<ITenantProvider>();
        var memoryCache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();

        if (tenantProvider.TenantId == null)
        {
            context.Result = new JsonResult(new
            {
                error = "TenentId is missing.",
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            return;
        }

        string cacheKey = $"permissions:{userId}";
        if (!memoryCache.TryGetValue(cacheKey, out List<string>? permissions))
        {
            var userWithPermissions = await dbContext.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantProvider.TenantId);

            if (userWithPermissions == null)
            {
                context.Result = BuildUnauthorizedResult();
                return;
            }

            permissions = userWithPermissions.Permissions
                .Select(p => p.Name)
                .ToList();

            memoryCache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));
        }

        if (permissions == null || !permissions.Contains(_permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new JsonResult(new
            {
                error = "Permission denied.",
                requiredPermission = _permission
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }

    private static bool IsUserAuthenticated(ClaimsPrincipal user, out Guid userId)
    {
        userId = Guid.Empty;

        if (user?.Identity?.IsAuthenticated != true)
            return false;

        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out userId))
            return false;

        return true;
    }

    private static IActionResult BuildUnauthorizedResult() =>
        new JsonResult(new
        {
            error = "You are not authenticated."
        })
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
}
