namespace Ecommerce.Models.RequestModels;

public record UserLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
