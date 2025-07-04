﻿using Ecommerce.Configurations;
using EcommerceApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Utilities;

public class JwtTokenGenerator(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _jwtSettings = options.Value;

    public string GenerateToken(Guid userId, Guid tenatid,string name, string email, string CompanyName)
    {
        return GenerateToken(userId, tenatid, name, email,CompanyName, DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes));
    }
    public string GenerateToken(Guid userId,Guid tenatid, string name, string email, string CompanyName, DateTime expiryInMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                new Claim(AppClaims.UserId, userId.ToString()),
                new Claim(AppClaims.TenantId, tenatid.ToString()),
                new Claim(AppClaims.Name, name),
                new Claim(AppClaims.CompanyName, CompanyName),
                new Claim(AppClaims.Email, email),
            };

        var token = new JwtSecurityToken(
               issuer: _jwtSettings.Issuer,
               audience: _jwtSettings.Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
               signingCredentials: creds
           );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }

            return null;
        }
        catch
        {            
            return null;
        }
    }
}
