using Ecommerce.Entities;
using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Entities;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceApi.Controllers.Tenant;

[ApiController]
public class AuthenticationController(
    AppDbContext context,
    JwtTokenGenerator jwtTokenGenerator,
    EmailSender emailSender) : ControllerBase
{
    /// <summary>
    /// Registers a new tenant and primary user.
    /// </summary>
    /// <remarks>
    /// <b>Error Messages:</b><br></br>
    /// <list type="bullet">
    ///   <item>A tenant with the same email or phone number already exists.</item><br></br>
    /// </list>
    /// <b>Returns:</b> 201 Created with <see cref="UserRegisterResponse"/> on success; 400 with problem details on failure.
    /// </remarks>
    /// <response code="201">Tenant and user created</response>
    /// <response code="400">Tenant already exists</response>
    [Produces("application/json")]
    [HttpPost(Endpoints.Authentication.RegisterTenant)]
    public async Task<ActionResult<UserRegisterRequest>> RegisterTenant(UserRegisterRequest userRegisterRequest)
    {
        var user = await context.Users
                                .Include(u => u.Tenant)
                                .Where(u => 
                                    u.Email == userRegisterRequest.Email || 
                                    u.PhoneNumber == userRegisterRequest.PhoneNumber ||
                                    (u.Tenant != null && u.Tenant.CompanyName == userRegisterRequest.CompanyName))
                                .FirstOrDefaultAsync();


        if (user != null)
        {
            if (user.Tenant!.CompanyName == userRegisterRequest.CompanyName)
            {
                ModelState.AddModelError(nameof(userRegisterRequest.CompanyName), "A tenant with the same company name already exists.");
                return this.ApplicationProblem(
                    detail: "A tenant with the same company name already exists.",
                    title: "Registration Failed",
                    statusCode: StatusCodes.Status409Conflict,
                    errorCode: ErrorCodes.CompanyAlreadyExist,
                    instance: HttpContext.Request.Path,
                    modelState: ModelState
                );
            }

            if (user.Email == userRegisterRequest.Email)
            {
                ModelState.AddModelError(nameof(userRegisterRequest.Email), "A tenant with the same email already exists.");
                return this.ApplicationProblem(
                    detail: "A tenant with the same email already exists.",
                    title: "Registration Failed",
                    statusCode: StatusCodes.Status409Conflict,
                    errorCode: ErrorCodes.EmailAlreadyExist,
                    instance: HttpContext.Request.Path,
                    modelState: ModelState
                );
            }

            if (user.PhoneNumber == userRegisterRequest.PhoneNumber)
            {
                ModelState.AddModelError(nameof(userRegisterRequest.PhoneNumber), "A tenant with the same phone number already exists.");
                return this.ApplicationProblem(
                    detail: "A tenant with the same phone number already exists.",
                    title: "Registration Failed",
                    statusCode: StatusCodes.Status409Conflict,
                    errorCode: ErrorCodes.PhoneAlreadyExist,
                    instance: HttpContext.Request.Path,
                    modelState: ModelState
                );
            }

        }

        Ecommerce.Entities.Tenant newtant = Ecommerce.Entities.Tenant.Create(userRegisterRequest.CompanyName);

        var newUser = Ecommerce.Entities.User.Create(
            password: PasswordHasher.Hash(userRegisterRequest.Password),
            email: userRegisterRequest.Email,
            phoneNumber: userRegisterRequest.PhoneNumber,
            firstName: userRegisterRequest.FirstName,
            lastName: userRegisterRequest.LastName,
            address: userRegisterRequest.Address,
            tenant: newtant,
            true
        );

        var features = FeatureFactory.GetFlattenedPermissionList();

        foreach (var feature in features)
        {
            newUser.AddPermission(Permission.Create(feature, newUser));
        }

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        var token = jwtTokenGenerator.GenerateToken(
             newUser.UserId,
             newtant.TenantId,
             newUser.FirstName + " " + newUser.LastName,
             newUser.Email,
             newtant.CompanyName,
             DateTime.UtcNow.AddMinutes(10)
         );

        string verificationUrl = $"{Request.Scheme}://{Request.Host}/{Endpoints.Authentication.Verify}?token={token}";

        string body = $@"<p>Dear {newUser.FirstName} {newUser.LastName},</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='color:#2e6c80; font-weight:bold;'>Verify Email</a></p>
                    <p>This link is valid for the next 10 minutes.</p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Best regards,<br/>CompanyName</p>";

        await emailSender.SendEmailAsync(toEmail: newUser.Email, subject: "Verify Your Email Address", body);

        return Created(Endpoints.Authentication.Login, value: new UserRegisterResponse
        {
            Id = newUser.UserId,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            CreatedAt = newUser.CreatedAt,
            UpdatedAt = newUser.UpdatedAt,
            Address = newUser.Address,
            LastLogin = newUser.LastLogin,
            PhoneNumber = newUser.PhoneNumber,
            TenantId = newtant.TenantId,
            CompanyName = newtant.CompanyName
        });
    }


}
