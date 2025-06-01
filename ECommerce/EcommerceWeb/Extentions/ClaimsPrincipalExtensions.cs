using System.Security.Claims;

namespace EcommerceWeb.Extentions;
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
	public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissions)
	{
		if (user.HasClaim("IsPrimaryTanent", true.ToString()))
		{
			return true; // Primary tenant users have all permissions
		}
		for (int i = 0; i < permissions.Length; i++)
		{
			if (user.HasClaim("Permission", permissions[i]))
			{
				return true;
			}
		}
		return false;
	}
	public static bool HasPermissions(this ClaimsPrincipal user, params string[] permission)
	{
		if (user.HasClaim("IsPrimaryTanent", true.ToString()))
		{
			return true;
		}
		for (int i = 0; i < permission.Length; i++)
		{
			if (!user.HasClaim("Permission", permission[i]))
			{
				return false;
			}
		}
		return true;
	}
}
