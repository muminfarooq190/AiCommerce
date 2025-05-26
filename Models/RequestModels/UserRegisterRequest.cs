using System.ComponentModel.DataAnnotations;

namespace Models.RequestModels;

public class UserRegisterRequest
{
    [Required]
    public required string CompanyName { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public required string Password { get; set; }
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    [Required]
    public required string PhoneNumber { get; set; }
    [Required]
    public required string Address { get; set; }
}
