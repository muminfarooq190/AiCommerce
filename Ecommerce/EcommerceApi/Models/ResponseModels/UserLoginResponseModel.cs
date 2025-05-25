namespace Ecommerce.Models.ResponseModels;

public class UserLoginResponseModel
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; } 
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required string Token { get; set; }
}
