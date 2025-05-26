using Ecommerce.Entities;
using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.RequestModels;

namespace EcommerceApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PermisstionController(AppDbContext appDbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllPermissions()
    {

        return Ok(FeatureFactory.GetJsonRepresentation());
    }

    [HttpPost]
    [AppAuthorize(FeatureFactory.Permission.CanGivePermisston)]
    public async Task<IActionResult> GivePermission(GivePermisstionRequest request)
    {
        if (!FeatureFactory.GetFlattenedPermissionList().Contains(request.Permission))
        {
            return BadRequest("Invalid permission requested.");
        }

        var excestinguser = await appDbContext.Users.Include(u => u.Permissions).FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (excestinguser == null)
        {
            return BadRequest("User does exist.");
        }
        if (excestinguser.Permissions.Any(p => p.Name == request.Permission))
        {
            return BadRequest("User already has this permission.");
        }

        await appDbContext.UserPermissions.AddAsync(PermissionsEntity.Create(request.Permission, request.UserId));
        await appDbContext.SaveChangesAsync();
        return Ok();
    }
}
