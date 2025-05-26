using Ecommerce.Entities;
using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.RequestModels;

namespace EcommerceApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PermisstionController(AppDbContext appDbContext, ITenantProvider tenantProvider) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllPermissionsAsJson()
    {

        return Ok(FeatureFactory.GetJsonRepresentation());
    }

    [HttpGet]
    public IActionResult GetAllPermissions()
    {

        return Ok(FeatureFactory.GetFlattenedPermissionList());
    }

    [HttpPost]
    [AppAuthorize(FeatureFactory.Permission.CanGivePermisston)]
    public async Task<IActionResult> GivePermission(GivePermisstionRequest request)
    {
        if (!FeatureFactory.GetFlattenedPermissionList().Contains(request.Permission))
        {
            return Problem(
               detail: "Invalid permission requested.",
               title: "Failed",
               statusCode: StatusCodes.Status400BadRequest,
               instance: HttpContext.Request.Path
           );
        }
        var tenantid = tenantProvider.TenantId ?? throw new ArgumentNullException("TenantId is missing");

        var excestinguser = await appDbContext.Users.Include(u => u.Permissions).FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantid);
        if (excestinguser == null)
        {
            return Problem(
               detail: "User does exist.",
               title: "Failed",
               statusCode: StatusCodes.Status400BadRequest,
               instance: HttpContext.Request.Path
           );
        }

        if (excestinguser.Permissions.Any(p => p.Name == request.Permission))
        {
            return Problem(
               detail: "User already has this permission.",
               title: "Failed",
               statusCode: StatusCodes.Status409Conflict,
               instance: HttpContext.Request.Path
           );
        }

        await appDbContext.UserPermissions.AddAsync(PermissionsEntity.Create(request.Permission, request.UserId));
        await appDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [AppAuthorize(FeatureFactory.Permission.CanRemovePermisston)]
    public async Task<IActionResult> RemovePermissionByName(RemovePermisstionByNameRequest request)
    {
        if (!FeatureFactory.GetFlattenedPermissionList().Contains(request.Permission))
        {
            return Problem(
               detail: "Invalid permission requested.",
               title: "Failed",
               statusCode: StatusCodes.Status400BadRequest,
               instance: HttpContext.Request.Path
           );
        }
        var tenantid = tenantProvider.TenantId ?? throw new ArgumentNullException("TenantId is missing");
        var excestinguser = await appDbContext.Users.Include(u => u.Permissions).FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantid);

        if (excestinguser == null)
        {
            return Problem(
               detail: "User does exist.",
               title: "Failed",
               statusCode: StatusCodes.Status400BadRequest,
               instance: HttpContext.Request.Path
           );
        }

        if (excestinguser.IsTenantPrimary)
        {
            return Problem(
               detail: "You cannot remove permissions from the primary tenant user.",
               title: "Failed",
               statusCode: StatusCodes.Status403Forbidden,
               instance: HttpContext.Request.Path);
        }

        if (excestinguser.RemovePermission(request.Permission))
        {
            return Problem(
               detail: "User does not have this permission.",
               title: "Failed",
               statusCode: StatusCodes.Status409Conflict,
               instance: HttpContext.Request.Path
           );
        }

        await appDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [AppAuthorize(FeatureFactory.Permission.CanRemovePermisston)]
    public async Task<IActionResult> RemovePermission(RemovePermisstionRequest request)
    {

        var tenantid = tenantProvider.TenantId ?? throw new ArgumentNullException("TenantId is missing");
        var excestinguser = await appDbContext.Users.Include(u => u.Permissions).FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantid);

        if (excestinguser == null)
        {
            return Problem(
               detail: "User does exist.",
               title: "Failed",
               statusCode: StatusCodes.Status400BadRequest,
               instance: HttpContext.Request.Path
           );
        }

        if (excestinguser.IsTenantPrimary)
        {
            return Problem(
               detail: "You cannot remove permissions from the primary tenant user.",
               title: "Failed",
               statusCode: StatusCodes.Status403Forbidden,
               instance: HttpContext.Request.Path);
        }

        if (excestinguser.RemovePermission(request.PermissionId))
        {
            return Problem(
               detail: "User does not have this permission.",
               title: "Failed",
               statusCode: StatusCodes.Status409Conflict,
               instance: HttpContext.Request.Path
           );
        }

        await appDbContext.SaveChangesAsync();
        return Ok();
    }
}
