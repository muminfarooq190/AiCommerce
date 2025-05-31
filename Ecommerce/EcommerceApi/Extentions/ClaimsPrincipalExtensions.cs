using System.Security.Claims;

namespace EcommerceApi.Extentions;
public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        if (user.HasClaim("IsPrimaryTanent",true.ToString()))
        {
            return true; // Primary tenant users have all permissions
        }

        return user.HasClaim("Permission", permission);
    }
}
