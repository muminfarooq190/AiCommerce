using System.ComponentModel.DataAnnotations;

namespace EcommerceWeb.Models;

public class LoginViewModel
{
   
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
   
}

