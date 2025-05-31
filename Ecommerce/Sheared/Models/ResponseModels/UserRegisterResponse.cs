namespace Sheared.Models.ResponseModels;

public class UserRegisterResponse
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; } 
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required DateTime LastLogin { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required Guid TenantId { get; set; }
    public required string CompanyName { get; set; }

 }
