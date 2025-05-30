namespace Sheared.Models.RequestModels;
using System.ComponentModel.DataAnnotations;

public class GetTenantRequest
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
}
