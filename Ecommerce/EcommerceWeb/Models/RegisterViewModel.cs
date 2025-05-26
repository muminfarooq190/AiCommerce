using System.ComponentModel.DataAnnotations;

namespace EcommerceWeb.Models;
public class RegisterViewModel
{
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Address")]
    public required string Address { get; set; }

    [Required]
    [Display(Name = "Phone Number")]
    public required string PhoneNumber { get; set; }

    [Required]
    [Display(Name = "Company Name")]
    public required string CompanyName { get; set; }
}
