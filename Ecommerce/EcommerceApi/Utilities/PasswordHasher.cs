using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Utilities;
public class PasswordHasher
{
    private static readonly PasswordHasher<object> _hasher = new();

    public static string Hash(string password)
    {
        return _hasher.HashPassword(password, password);
    }

    public static bool Verify(string hashedPassword, string providedPassword)
    {
        return PasswordVerificationResult.Success == _hasher.VerifyHashedPassword(providedPassword, hashedPassword, providedPassword);
    }
}