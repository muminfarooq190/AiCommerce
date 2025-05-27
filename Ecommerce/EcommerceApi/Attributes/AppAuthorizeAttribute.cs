using Ecommerce.Entities;
using EcommerceApi.Models;
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
            
            var problem = new ProblemDetails
            {
                Title = "You are not authenticated.",
                Detail = "You are not authenticated.",
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Instance = context.HttpContext.Request.Path
            };

            problem.Extensions["errorCode"] = ErrorCodes.TanentIdMissing;

            context.Result = new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        if (string.IsNullOrWhiteSpace(_permission))
            return;

        var dbContext = httpContext.RequestServices.GetRequiredService<AppDbContext>();
        var tenantProvider = httpContext.RequestServices.GetRequiredService<ITenantProvider>();
        var memoryCache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();

        if (tenantProvider.TenantId == null)
        {
            var problem = new ProblemDetails
            {
                Title = "Tenant ID is missing.",
                Detail = "The request did not contain a valid tenant ID.",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Instance = context.HttpContext.Request.Path
            };

            problem.Extensions["errorCode"] = ErrorCodes.TanentIdMissing;

            context.Result = new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };

        }

        string cacheKey = $"permissions:{userId}";
        if (!memoryCache.TryGetValue(cacheKey, out List<string>? permissions))
        {
            var userWithPermissions = await dbContext.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantProvider.TenantId);

            if (userWithPermissions == null)
            {
                var problem = new ProblemDetails
                {
                    Title = "You are not authorized.",
                    Detail = "You are not authorized.",
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Instance = context.HttpContext.Request.Path
                };

                problem.Extensions["errorCode"] = ErrorCodes.InsufficientPermissions;

                context.Result = new ObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            permissions = userWithPermissions.Permissions
                .Select(p => p.Name)
                .ToList();

            memoryCache.Set(cacheKey, permissions, TimeSpan.FromMinutes(10));
        }

        if (permissions == null || !permissions.Contains(_permission, StringComparer.OrdinalIgnoreCase))
        {
            var problem = new ProblemDetails
            {
                Title = "You are not authorized.",
                Detail = "You are not authorized.",
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Instance = context.HttpContext.Request.Path
            };

            problem.Extensions["errorCode"] = ErrorCodes.InsufficientPermissions;

            context.Result = new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            return;
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

    
        
}
