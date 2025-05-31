using Ecommerce.Entities;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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
        var userProvider = httpContext.RequestServices.GetRequiredService<IUserProvider>();
        if (!userProvider.IsAuthenticated)
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

        var memoryCache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();


        string cacheKey = $"permissions:{userProvider.UserId}:{userProvider.TenantId}";
        UserEntity? userWithPermissions = null;
        if (!memoryCache.TryGetValue(cacheKey, out List<string>? permissions))
        {
            userWithPermissions = await dbContext.Users
                .Include(u => u.Tenant)
                .Include(u => u.Permissions)
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                                        u.Id == userProvider.UserId &&
                                        u.TenantId == userProvider.TenantId);

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

            if (userWithPermissions.IsTenantPrimary)
            {
                return; // Tenant primary users are always authorized
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

}
