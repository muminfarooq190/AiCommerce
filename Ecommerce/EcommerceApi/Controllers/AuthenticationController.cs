using Ecommerce.Entities;
using Ecommerce.Services;
using Ecommerce.Utilities;
using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.RequestModels;
using Models.ResponseModels;
using System.IdentityModel.Tokens.Jwt;

namespace Ecommerce.Controllers;

[Route("api/[controller]/[Action]")]
[ApiController]
public class AuthenticationController(AppDbContext context, JwtTokenGenerator jwtTokenGenerator, EmailSender emailSender,ITenantProvider tenantProvider) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserLoginResponse>> UserLogin(UserLoginRequest userLoginRequest)
    {
        Guid? TenantId = tenantProvider.TenantId;

        if (TenantId == null)
        {
            return BadRequest("TenantId header is missing or invalid.");
        }

        var user = context.Users.FirstOrDefault(u => u.Email == userLoginRequest.Email && u.TenantId == TenantId);

        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (user.IsLocked)
        {
            return Unauthorized("User account is locked.");
        }

        if (!PasswordHasher.Verify(user.Password, userLoginRequest.Password))
        {
            user.IncreaseFailedLogin();
            await context.SaveChangesAsync();
            return Unauthorized("Invalid username or password.");
        }

        if (!user.IsEmailVerified)
        {
            return Unauthorized("User email is not verified.");
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

    [HttpPost]
    [AppAuthorize(FeatureFactory.Authentication.CanCreateUser)]
    public async Task<ActionResult<UserRegisterRequest>> CreateUser(UserRegisterRequest userRegisterRequest)
    {
        Guid? TenantId = tenantProvider.TenantId;
        if (TenantId == null)
        {
            return BadRequest("TenantId header is missing or invalid.");
        }

        Guid tenantIdValue = (Guid)TenantId;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userRegisterRequest.Email && u.TenantId == tenantIdValue);

        if (user != null)
        {
            return BadRequest("User already exists.");
        }

        var newUser = UserEntity.Create(
            password: PasswordHasher.Hash(userRegisterRequest.Password),
            email: userRegisterRequest.Email,
            phoneNumber: userRegisterRequest.PhoneNumber,
            firstName: userRegisterRequest.FirstName,
            lastName: userRegisterRequest.LastName,
            address: userRegisterRequest.Address,
            tenantId: tenantIdValue
        );        

        context.Users.Add(newUser);
        context.SaveChanges();

        var link = jwtTokenGenerator.GenerateToken(
            newUser.Id,
            newUser.FirstName + " " + newUser.LastName,
            newUser.Email,
            DateTime.UtcNow.AddMinutes(10)
        );

        string verificationUrl = $"https://yourdomain.com/verify?token={link}";

        await emailSender.SendEmailAsync(
            toEmail: newUser.Email,
            subject: "Verify Your Email Address",
            body: $@"
                    <p>Dear {newUser.FirstName} {newUser.LastName},</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='color:#2e6c80; font-weight:bold;'>Verify Email</a></p>
                    <p>This link is valid for the next 10 minutes.</p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Best regards,<br/>Your Company Name</p>"
                    );

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
            TenantId = newUser.TenantId
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserRegisterRequest>> RegisterTenant(UserRegisterRequest userRegisterRequest)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userRegisterRequest.Email || u.PhoneNumber == userRegisterRequest.PhoneNumber);

        if (user != null)
        {
            return BadRequest("Tenant with same email or phone number already exists.");
        }

       TenantEntity newtant = TenantEntity.Create(userRegisterRequest.CompanyName);

        var newUser = UserEntity.Create(
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

        foreach(var feature in features)
        {
            newUser.AddPermission(PermissionsEntity.Create(feature,newUser));
        }

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        var link = jwtTokenGenerator.GenerateToken(
             newUser.Id,
             newUser.FirstName + " " + newUser.LastName,
             newUser.Email,
             DateTime.UtcNow.AddMinutes(10)
         );

        string verificationUrl = $"https://yourdomain.com/verify?token={link}";

        await emailSender.SendEmailAsync(
            toEmail: newUser.Email,
            subject: "Verify Your Email Address",
            body: $@"<p>Dear {newUser.FirstName} {newUser.LastName},</p>
                    <p>Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='color:#2e6c80; font-weight:bold;'>Verify Email</a></p>
                    <p>This link is valid for the next 10 minutes.</p>
                    <p>If you did not request this, please ignore this message.</p>
                    <p>Best regards,<br/>Your Company Name</p>");


        return CreatedAtAction(nameof(UserLogin), value: new UserRegisterResponse
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
            TenantId = newUser.TenantId
        });
    }

    [HttpGet]
    public async Task<ActionResult<string>> GetTenantId(UserLoginRequest userLoginRequest)
    {
        // Validate the user credentials against the database        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userLoginRequest.Email);

        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (user.IsLocked)
        {
            return Unauthorized("User account is locked.");
        }

        if (!PasswordHasher.Verify(user.Password, userLoginRequest.Password))
        {
            user.IncreaseFailedLogin();
            await context.SaveChangesAsync();
            return Unauthorized("Invalid username or password.");
        }

        if (user.IsEmailVerified)
        {
            return Unauthorized("User email is not verified.");
        }

        if (!user.IsTenantPrimary)
        {
            return Unauthorized("User is not a tenant primary user. User should be primary tenant account.");
        }

        user.ResetFailedLogin();
        await context.SaveChangesAsync();

        return Ok(user.TenantId);

    }

    [HttpPost]
    [AppAuthorize("SendLink")]
    public async Task<ActionResult> ResendLink(UserLoginRequest userLoginRequest)
    {
        Guid? TenantId = tenantProvider.TenantId;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userLoginRequest.Email);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (user.IsLocked)
        {
            return Unauthorized("User account is locked.");
        }

        if (!PasswordHasher.Verify(userLoginRequest.Password, user.Password))
        {
            user.IncreaseFailedLogin();
            await context.SaveChangesAsync();
            return Unauthorized("Invalid username or password.");
        }

        if (TenantId == null)
        {
            if (!user.IsTenantPrimary)
                return BadRequest("TenantId header is missing");

        }
        else if (TenantId != user.TenantId)
        {
            return Unauthorized("Invalid username or password.");
        }

        var link = jwtTokenGenerator.GenerateToken(
             user.Id,
             user.FirstName + " " + user.LastName,
             user.Email,
             DateTime.UtcNow.AddMinutes(10)
         );

        string verificationUrl = $"https://yourdomain.com/verify?token={link}";

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

    [HttpGet]
    public async Task<ActionResult> VerifyUser(string token)
    {
        var clams = jwtTokenGenerator.ValidateToken(token);

        if (clams == null)
        {
            return Unauthorized("Expirecd or invalid link");
        }

        var email = clams.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

        UserEntity user = await context.Users.FirstAsync(u => u.Email == email);


        if (user.IsEmailVerified)
        {
            return Unauthorized("emial is already verified");
        }

        user.VerifyEmail();

        var url = $"https://yourdomain.com/login?email={user.Email}";

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


        context.SaveChanges();
        return Ok("Email verified successfully.");
    }
}
