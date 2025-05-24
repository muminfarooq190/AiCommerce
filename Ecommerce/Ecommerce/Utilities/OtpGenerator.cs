using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Utilities;

public static class OtpGenerator
{
    /// <summary>
    /// Generates a numeric OTP of the given length.
    /// </summary>
    public static string GenerateNumericOtp(int length = 6)
    {
        if (length <= 0 || length > 10)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 10.");

        var rng = new RNGCryptoServiceProvider();
        var otp = new StringBuilder();

        while (otp.Length < length)
        {
            byte[] randomByte = new byte[1];
            rng.GetBytes(randomByte);
            int digit = randomByte[0] % 10;
            otp.Append(digit);
        }

        return otp.ToString();
    }

    /// <summary>
    /// Generates an alphanumeric OTP (mixed letters and digits).
    /// </summary>
    public static string GenerateAlphanumericOtp(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var result = new StringBuilder();
        using var rng = RandomNumberGenerator.Create();
        byte[] buffer = new byte[1];

        while (result.Length < length)
        {
            rng.GetBytes(buffer);
            var index = buffer[0] % chars.Length;
            result.Append(chars[index]);
        }

        return result.ToString();
    }
}