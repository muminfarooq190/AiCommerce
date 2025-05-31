using System.ComponentModel.DataAnnotations;

namespace Sheared.Models.RequestModels;

public record UserLoginRequest
{
    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public required string Password { get; set; }
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required Guid TenentId { get; set; }
}
