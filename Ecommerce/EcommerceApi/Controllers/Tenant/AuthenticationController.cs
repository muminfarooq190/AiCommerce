using Ecommerce.Entities;
using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Configrations;
using EcommerceApi.Entities;
using EcommerceApi.Entities.DbContexts;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using EcommerceApi.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceApi.Controllers.Tenant;

[ApiController]
public class AuthenticationController(
    TenantDbContext tenantDbContext,
    ITenantProvider tenantProvider,
    SchemaCloner schemaCloner,
    IServiceProvider serviceProvider,
    JwtTokenGenerator jwtTokenGenerator,
    EmailSender emailSender,
    IOptions<DefaultTenant> defaultTenant) : ControllerBase
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
    [HttpPost(Endpoints.AuthenticationEndpoints.RegisterTenant)]
    public async Task<ActionResult<UserRegisterRequest>> RegisterTenant(UserRegisterRequest userRegisterRequest)
    {
        var Tenant = await tenantDbContext.Tenants.FirstOrDefaultAsync(t => t.CompanyName == userRegisterRequest.CompanyName);

        if (Tenant != null)
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

        TenantEntity newtant = TenantEntity.Create(userRegisterRequest.CompanyName);

        tenantDbContext.Tenants.Add(newtant);
        await tenantDbContext.SaveChangesAsync();

        tenantProvider.SetTenantId(newtant.Id);

        var oldSchema = SchemaGenerater.Generate(defaultTenant.Value.CompanyName);
        await schemaCloner.CloneSchema(oldSchema, newtant.SchemaName);

        var newUser = UserEntity.Create(
            password: PasswordHasher.Hash(userRegisterRequest.Password),
            email: userRegisterRequest.Email,
            phoneNumber: userRegisterRequest.PhoneNumber,
            firstName: userRegisterRequest.FirstName,
            lastName: userRegisterRequest.LastName,
            address: userRegisterRequest.Address,
            TenantId: newtant.Id,
            true
        );

        var features = FeatureFactory.GetFlattenedPermissionList();

        foreach (var feature in features)
        {
            newUser.AddPermission(PermissionsEntity.Create(feature, newUser));
        }

        using var scope = serviceProvider.CreateScope();

        var appdbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        appdbContext.Users.Add(newUser);
        await appdbContext.SaveChangesAsync();

        var token = jwtTokenGenerator.GenerateToken(
             newUser.Id,
             newUser.FirstName + " " + newUser.LastName,
             newUser.Email,
             DateTime.UtcNow.AddMinutes(10)
         );

        string verificationUrl = $"{Request.Scheme}://{Request.Host}/api/verify?token={token}";

        string body = $@"<p>Dear {newUser.FirstName} {newUser.LastName},</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='color:#2e6c80; font-weight:bold;'>Verify Email</a></p>
                    <p>This link is valid for the next 10 minutes.</p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Best regards,<br/>{defaultTenant.Value.CompanyName}</p>";

        await emailSender.SendEmailAsync(toEmail: newUser.Email, subject: "Verify Your Email Address", body);

        return Created("api/Login", value: new UserRegisterResponse
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
            TenantId = newtant.Id,
            CompanyName = newtant.CompanyName
        });
    }


}
