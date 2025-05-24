using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Entities;

public class UserEntity
{
    [Key]
    public int Id { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }     
    public required DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public bool IsLocked { get; set; } = false; // Indicates if the user account is locked
    public int FailedLoginAttempts { get; set; } = 0; // Number of failed login attempts
    public bool IsEmailVerified { get; set; } = false; // Indicates if the user's email is verified
    public string? ProfilePictureUrl { get; set; } // Optional profile picture URL
    public required string Address { get; set; } // Optional address for the user   
    public bool IsTenantPrimary { get; set; } = false; // Indicates if the user is a tenant primarty user so it can be deleted.
    public Guid TenantId { get; set; } // Foreign key to TenantEntity
    public TenantEntity Tenant { get; set; } = null!; // Navigation property for TenantEntity
}
