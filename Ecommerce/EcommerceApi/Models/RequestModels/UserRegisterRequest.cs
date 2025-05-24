namespace Ecommerce.Models.RequestModels;

public class UserRegisterRequest
{
    public required string Password { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
}
