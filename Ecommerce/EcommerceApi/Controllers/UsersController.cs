using Ecommerce.Entities;
using EcommerceApi.Attributes;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;

namespace EcommerceApi.Controllers;

[ApiController]
public class UsersController(AppDbContext appDbContext) : ControllerBase
{
    [Produces("application/json")]
    [HttpGet(Endpoints.UserEndpoints.GetUsers)]
    [AppAuthorize(FeatureFactory.User.CanGetUsers)]
    public async  Task<ActionResult> GetUsers()
    {
        var user = await appDbContext.Users.ToListAsync();

        return Ok(user);
    }
}
