using Ecommerce.Entities;
using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Attributes;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceApi.Controllers;

[ApiController]
public class UsersController(AppDbContext context,IUserProvider userProvider, EmailSender emailSender,JwtTokenGenerator jwtTokenGenerator) : ControllerBase
{
    /// <summary>
    /// Registers a new user for an existing tenant.
    /// </summary>
    /// <remarks>
    /// <b>Error Messages:</b><br></br>
    /// <list type="bullet">
    ///   <item>User already exists.</item><br></br>
    ///   <item>TenantId cant be null</item><br></br>
    /// </list>
    /// <b>Returns:</b> 201 Created with <see cref="UserRegisterResponse"/> on success; 401 with problem details on failure.
    /// </remarks>
    /// <response code="201">User created</response>
    /// <response code="401">User already exists or tenant ID missing</response>
    [Produces("application/json")]
    [HttpPost(Endpoints.User.CreateUser)]
    [AppAuthorize(FeatureFactory.User.CanCreateUser)]
    public async Task<ActionResult<UserRegisterRequest>> CreateUser(UserRegisterRequest userRegisterRequest)
    {

        if (await context.Users.AnyAsync(u => u.Email == userRegisterRequest.Email))
        {
            ModelState.AddModelError(nameof(userRegisterRequest.Email), "User already exists.");

            return this.ApplicationProblem(
                detail: "User already exists.",
                title: "Registration Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                modelState: ModelState,
                errorCode: ErrorCodes.EmailAlreadyExist,
                instance: HttpContext.Request.Path
            );
        }

        var newUser = UserEntity.Create(
            password: PasswordHasher.Hash(userRegisterRequest.Password),
            email: userRegisterRequest.Email,
            phoneNumber: userRegisterRequest.PhoneNumber,
            firstName: userRegisterRequest.FirstName,
            lastName: userRegisterRequest.LastName,
            address: userRegisterRequest.Address,
            TenantId: userProvider.TenantId!
        );

        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();

        var user = await context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == newUser.Id);

        var token = jwtTokenGenerator.GenerateToken(
            newUser.Id,
            newUser.TenantId,
            newUser.FirstName + " " + newUser.LastName,
            newUser.Email,
            user!.Tenant.CompanyName,
            DateTime.UtcNow.AddMinutes(10)
        );

        string verificationUrl = $"{Request.Scheme}://{Request.Host}/{Endpoints.Authentication.Verify}?token=n={token}";
        await emailSender.SendEmailAsync(
            toEmail: newUser.Email,
            subject: "Verify Your Email Address",
            body: $@"
                    <p>Dear {newUser.FirstName} {newUser.LastName},</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='color:#2e6c80; font-weight:bold;'>Verify Email</a></p>
                    <p>This link is valid for the next 10 minutes.</p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Best regards,<br/>{userProvider.CompanyName}</p>");


        return Created(Endpoints.Authentication.Login, new UserRegisterResponse
        {
            Id = newUser.Id,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            CreatedAt = newUser.CreatedAt,
            UpdatedAt = newUser.UpdatedAt,
            Address = newUser.Address,
            LastLogin = newUser.LastLogin,
            PhoneNumber = newUser.PhoneNumber,
            TenantId = userProvider.TenantId,
            CompanyName = userProvider.CompanyName
        });
    }

    [Produces("application/json")]
    [HttpGet(Endpoints.User.GetUsers)]
    [AppAuthorize(FeatureFactory.User.CanGetUsers)]
    public async Task<ActionResult> GetUsers()
    {
        var user = await context.Users.ToListAsync();

        return Ok(user);
    }
}
