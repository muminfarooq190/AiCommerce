using Ecommerce.Entities;
using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Attributes;
using EcommerceApi.Entities.DbContexts;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Security.Claims;

namespace EcommerceApi.Controllers;

/// <summary>
/// Handles authentication and user management endpoints for the API.
/// </summary>
/// <remarks>
/// <b>Error Messages Returned by This Controller:</b>
/// <list type="bullet">
///   <item>TenantId header is missing or invalid.</item>
///   <item>Invalid username or password.</item>
///   <item>User already exists.</item>
///   <item>User email is not verified.</item>
///   <item>User account is locked.</item>
///   <item>A tenant with the same email or phone number already exists.</item>
///   <item>User is not a tenant primary user. User should be primary tenant account.</item>
///   <item>Expirecd or invalid link.</item>
///   <item>User account is already verified.</item>
///   <item>emial is already verified.</item>
///   <item>TenantId cant be null</item>
/// </list>
/// <b>HTTP Status Codes Used:</b>
/// <list type="bullet">
///   <item>400 BadRequest</item>
///   <item>401 Unauthorized</item>
///   <item>201 Created</item>
///   <item>200 OK</item>
/// </list>
/// </remarks>
[Produces("application/json")]
[ApiController]
public class AuthenticationController(
    TenantDbContext tenantDbContext,
    AppDbContext context,
    JwtTokenGenerator jwtTokenGenerator,
    EmailSender emailSender,
    ITenantProvider tenantProvider) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <remarks>
    /// <b>Error Messages:</b><br></br>
    /// <list type="bullet">
    ///   <item>TenantId header is missing or invalid.</item><br></br>
    ///   <item>Invalid username or password.</item><br></br>
    ///   <item>User email is not verified.</item><br></br>
    ///   <item>User account is locked.</item><br></br>
    /// </list>
    /// <b>Returns:</b> 200 OK with <see cref="UserLoginResponse"/> on success; 400/401 with problem details on failure.
    /// </remarks>
    /// <response code="200">Login successful</response>
    /// <response code="400">Bad request or invalid credentials</response>
    /// <response code="401">Unauthorized (email not verified or account locked)</response>
    [Produces("application/json")]
    [HttpPost(Endpoints.AuthenticationEndpoints.Login)]
    public async Task<ActionResult<UserLoginResponse>> UserLogin(UserLoginRequest userLoginRequest)
    {
        Guid? TenantId = tenantProvider.TenantId;

        if (TenantId == null)
        {
            ModelState.AddModelError("TenantId", "TenantId header is missing or invalid.");
            return this.ApplicationProblem(
                detail: "TenantId header is missing or invalid.",
                title: "Login Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                modelState: ModelState,
                errorCode: ErrorCodes.TenantIdMissingOrInvalid
            );

        }

        var user = context.Users.FirstOrDefault(u => u.Email == userLoginRequest.Email);

        if (user == null)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "Invalid email or password.");
            return this.ApplicationProblem(
                detail: "Invalid email or password.",
                title: "Login Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.InvalidEmailOrPassword,
                modelState: ModelState
            );
        }

        if (!PasswordHasher.Verify(user.Password, userLoginRequest.Password))
        {
            user.IncreaseFailedLogin();
            await context.SaveChangesAsync();

            ModelState.AddModelError(nameof(userLoginRequest.Email), "Invalid email or password.");

            return this.ApplicationProblem(
                detail: "Invalid email or password.",
                title: "Login Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.InvalidEmailOrPassword,
                modelState: ModelState
            );
        }

        if (!user.IsEmailVerified)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "User email is not verified.");
            return this.ApplicationProblem(
                detail: "User email is not verified.",
                title: "Login Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.UnverifiedEmail,
                modelState: ModelState
            );
        }

        if (user.IsLocked)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "User account is locked.");
            return this.ApplicationProblem(
                detail: "User account is locked.",
                title: "Login Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.AccountLocked,
                modelState: ModelState
            );
        }

        user.ResetFailedLogin();
        user.UpdateLastLogin();
        await context.SaveChangesAsync();

        return Ok(new UserLoginResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Token = jwtTokenGenerator.GenerateToken(user.Id, user.FirstName + " " + user.LastName, user.Email)
        });

    }

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
    [HttpPost(Endpoints.AuthenticationEndpoints.CreateUser)]
    [AppAuthorize(FeatureFactory.Authentication.CanCreateUser)]
    public async Task<ActionResult<UserRegisterRequest>> CreateUser(UserRegisterRequest userRegisterRequest)
    {
        TenantEntity? tenant = await tenantProvider.GetCurrentTenantAsync() ?? throw new ArgumentNullException("TenantId cant be null");

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userRegisterRequest.Email);

        if (user != null)
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
            TenantId: tenant.Id
        );

        await context.Users.AddAsync(newUser);
        await context.SaveChangesAsync();

        var token = jwtTokenGenerator.GenerateToken(
            newUser.Id,
            newUser.FirstName + " " + newUser.LastName,
            newUser.Email,
            DateTime.UtcNow.AddMinutes(10)
        );

        string verificationUrl = $"{Request.Scheme}://{Request.Host}/api/verify?token={token}";

        await emailSender.SendEmailAsync(
            toEmail: newUser.Email,
            subject: "Verify Your Email Address",
            body: $@"
                    <p>Dear {newUser.FirstName} {newUser.LastName},</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='color:#2e6c80; font-weight:bold;'>Verify Email</a></p>
                    <p>This link is valid for the next 10 minutes.</p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Best regards,<br/>{tenant.CompanyName}</p>");


        return CreatedAtAction(nameof(UserLogin), new UserRegisterResponse
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
            TenantId = tenant.Id,
            CompanyName = tenant.CompanyName
        });
    }

    /// <summary>
    /// Gets the tenant ID for a user if credentials are valid and user is primary tenant.
    /// </summary>
    /// <remarks>
    /// <b>Error Messages:</b><br></br>
    /// <list type="bullet">
    ///   <item>Invalid username or password.</item><br></br>
    ///   <item>User account is locked.</item><br></br>
    ///   <item>User email is not verified.</item><br></br>
    ///   <item>User is not a tenant primary user. User should be primary tenant account.</item><br></br>
    /// </list>
    /// <b>Returns:</b> 200 OK with tenant ID on success; 400/401 with problem details on failure.
    /// </remarks>
    /// <response code="200">Tenant ID returned</response>
    /// <response code="400">Invalid credentials</response>
    /// <response code="401">Unauthorized (account locked, email not verified, or not primary tenant)</response>
    [Produces("application/json")]
    [HttpPost(Endpoints.AuthenticationEndpoints.GetTenentId)]
    public async Task<ActionResult<Guid>> GetTenantId(GetTenantRequest userLoginRequest)
    {
        var Tenant = await tenantDbContext.Tenants.FirstOrDefaultAsync(t => t.CompanyName == userLoginRequest.CompanyName);

        if (Tenant == null)
        {
            ModelState.AddModelError(nameof(userLoginRequest.CompanyName), "invalid company name.");
            return this.ApplicationProblem(
                detail: "invalid company name.",
                title: "Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.CompanyNotExist,
                modelState: ModelState
            );
        }
        // Validate the user credentials against the database        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userLoginRequest.Email && u.TenantId == Tenant.Id);

        if (user == null)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "Invalid Email or password.");
            return this.ApplicationProblem(
                detail: "Invalid Email or password.",
                title: "Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.InvalidEmailOrPassword,
                modelState: ModelState
            );
        }


        if (!PasswordHasher.Verify(user.Password, userLoginRequest.Password))
        {
            user.IncreaseFailedLogin();
            await context.SaveChangesAsync();

            ModelState.AddModelError(nameof(userLoginRequest.Email), "Invalid Email or password.");
            return this.ApplicationProblem(
                detail: "Invalid Email or password.",
                title: "Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.InvalidEmailOrPassword,
                modelState: ModelState
            );
        }

        if (user.IsLocked)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "account is locked.");
            return this.ApplicationProblem(
                detail: "account is locked.",
                title: "Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.AccountLocked,
                modelState: ModelState
            );

        }

        if (!user.IsEmailVerified)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "email is not verified.");
            return this.ApplicationProblem(
                detail: "email is not verified.",
                title: "Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.UnverifiedEmail,
                modelState: ModelState
            );

        }

        if (!user.IsTenantPrimary)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "User is not a tenant primary user. User should be primary tenant account.");
            return this.ApplicationProblem(
                detail: "User is not a tenant primary user. User should be primary tenant account.",
                title: "Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.AccountNotPrimary,
                modelState: ModelState
            );
        }

        user.ResetFailedLogin();
        await context.SaveChangesAsync();
        // need to change the return type to Guid
        return Ok(user.TenantId);

    }

    /// <summary>
    /// Resends the email verification link to the user.
    /// </summary>
    /// <remarks>
    /// <b>Error Messages:</b><br></br>
    /// <list type="bullet">
    ///   <item>Expirecd or invalid link.</item><br></br>
    ///   <item>Invalid username or password</item><br></br>
    ///   <item>TenantId header is missing</item><br></br>
    ///   <item>User account is locked.</item><br></br>
    ///   <item>User account is already verified.</item><br></br>
    /// </list>
    /// <b>Returns:</b> 200 OK on success; 400/401 with problem details on failure.
    /// </remarks>
    /// <response code="200">Link resent successfully</response>
    /// <response code="400">Bad request or invalid credentials</response>
    /// <response code="401">Unauthorized (account locked or already verified)</response>
    [Produces("application/json")]
    [HttpPost(Endpoints.AuthenticationEndpoints.ResendLink)]
    public async Task<ActionResult> ResendLink(UserLoginRequest userLoginRequest)
    {
        Guid? TenantId = tenantProvider.TenantId;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userLoginRequest.Email);
        if (user == null)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "Invalid email or password");
            return this.ApplicationProblem(
                detail: "Invalid Email or password",
                title: "Verify Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.InvalidEmailOrPassword,
                modelState: ModelState
            );
        }

        if (!PasswordHasher.Verify(user.Password, userLoginRequest.Password))
        {
            user.IncreaseFailedLogin();
            await context.SaveChangesAsync();

            ModelState.AddModelError(nameof(userLoginRequest.Email), "Invalid email or password");
            return this.ApplicationProblem(
                detail: "Invalid Email or password",
                title: "Verify Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.InvalidEmailOrPassword,
                modelState: ModelState
            );
        }

        if (TenantId == null)
        {
            if (!user.IsTenantPrimary)
                ModelState.AddModelError(nameof(userLoginRequest.Email), "TenantId header is missing");
            return this.ApplicationProblem(
                detail: "TenantId header is missing",
                title: "Verify Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.TanentIdMissing,
                modelState: ModelState
            );
        }
        else if (TenantId != user.TenantId)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "Invalid username or password");
            return this.ApplicationProblem(
                detail: "Invalid username or password",
                title: "Verify Failed",
                statusCode: StatusCodes.Status400BadRequest,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.InvalidEmailOrPassword,
                modelState: ModelState
            );

        }

        if (user.IsLocked)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "User account is locked.");
            return this.ApplicationProblem(
                detail: "User account is locked.",
                title: "Verify Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                instance: HttpContext.Request.Path,
                errorCode: ErrorCodes.AccountLocked,
                modelState: ModelState
            );
        }

        if (user.IsEmailVerified)
        {
            ModelState.AddModelError(nameof(userLoginRequest.Email), "User account is already verified.");
            return this.ApplicationProblem(
                detail: "User account is already verified.",
                title: "Verify Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                errorCode: ErrorCodes.AlreadyVerifiedEmail,
                modelState: ModelState,
                instance: HttpContext.Request.Path
            );

        }

        var token = jwtTokenGenerator.GenerateToken(
             user.Id,
             user.FirstName + " " + user.LastName,
             user.Email,
             DateTime.UtcNow.AddMinutes(10)
         );

        string verificationUrl = $"{Request.Scheme}://{Request.Host}/api/authentication/verify?token={token}";

        await emailSender.SendEmailAsync(
            toEmail: user.Email,
            subject: "Verify Your Email Address",
            body: $@"
                    <p>Dear {user.FirstName} {user.LastName},</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='color:#2e6c80; font-weight:bold;'>Verify Email</a></p>
                    <p>This link is valid for the next 10 minutes.</p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Best regards,<br/>Your Company Name</p>"
                    );

        return Ok("Link resent successfully.");
    }

    /// <summary>
    /// Verifies a user's email using a token.
    /// </summary>
    /// <remarks>
    /// <b>Error Messages:</b><br></br>
    /// <list type="bullet">
    ///   <item>Expirecd or invalid link.</item><br></br>
    ///   <item>emial is already verified.</item><br></br>
    /// </list>
    /// <b>Returns:</b> 200 OK on success; 400/401 with problem details on failure.
    /// </remarks>
    /// <response code="200">Email verified successfully</response>
    /// <response code="400">Invalid or expired link</response>
    /// <response code="401">Email already verified</response>
    [Produces("application/json")]
    [HttpGet(Endpoints.AuthenticationEndpoints.Verify)]
    public async Task<ActionResult> Verify(string token)
    {
        var clams = jwtTokenGenerator.ValidateToken(token);

        if (clams == null)
        {
            ModelState.AddModelError(nameof(token), "Expirecd or invalid link.");
            return this.ApplicationProblem(
               detail: "Expirecd or invalid link.",
               title: "Verify Failed",
               statusCode: StatusCodes.Status400BadRequest,
               instance: HttpContext.Request.Path,
               errorCode: ErrorCodes.ExpiredOrInvalidLink,
               modelState: ModelState
               );
        }

        var UserId = clams.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(UserId) || !Guid.TryParse(UserId, out Guid userIdValue))
        {
            ModelState.AddModelError(nameof(token), "Expirecd or invalid link.");
            return this.ApplicationProblem(
               detail: "Expirecd or invalid link.",
               title: "Verify Failed",
               statusCode: StatusCodes.Status400BadRequest,
               instance: HttpContext.Request.Path,
               errorCode: ErrorCodes.ExpiredOrInvalidLink,
               modelState: ModelState
               );

        }

        UserEntity? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userIdValue);

        if (user == null)
        {
            ModelState.AddModelError(nameof(token), "Expirecd or invalid link.");
            return this.ApplicationProblem(
                detail: "Expirecd or invalid link.",
                title: "Verify Failed",
                statusCode: StatusCodes.Status400BadRequest,
                errorCode: ErrorCodes.ExpiredOrInvalidLink,
                modelState: ModelState,
                instance: HttpContext.Request.Path
            );
        }

        if (user.IsEmailVerified)
        {
            ModelState.AddModelError("email", "emial is already verified.");
            return this.ApplicationProblem(
                detail: "emial is already verified.",
                title: "Verify Failed",
                statusCode: StatusCodes.Status401Unauthorized,
                errorCode: ErrorCodes.AlreadyVerifiedEmail,
                modelState: ModelState,
                instance: HttpContext.Request.Path
                );
        }

        user.VerifyEmail();

        string url = $"{Request.Scheme}://{Request.Host}/api/Login?email={user.Email}";

        await emailSender.SendEmailAsync(
         toEmail: user.Email,
         subject: "Email Verified Successfully",
         body: $@"
            <p>Dear {user.FirstName} {user.LastName},</p>
            <p>Thank you for verifying your email address.</p>
            <p>Your email <strong>{user.Email}</strong> has been successfully confirmed.</p>
            <p>You can now access all the features of your account.</p>
            <p><a href='{url}'>Click here to log in</a></p>
            <p>If you did not perform this action, please contact our support team immediately.</p>            
            <p>Best regards,<br/>Your Company Name</p>"
     );

        await context.SaveChangesAsync();
        return Ok();
    }
}
