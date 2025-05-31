using EcommerceApi.Entities;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Entities;

public class UserEntity
{
    [Key]
    public Guid Id { get; private set; }
    public  string Password { get; private set; }
    public  string Email { get; private set; }
    public  string PhoneNumber { get; private set; }
    public  string FirstName { get; private set; }
    public  string LastName { get; private set; }     
    public  DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public  DateTime LastLogin { get; private set; } = DateTime.UtcNow;
    public bool IsLocked { get; private set; } = false; // Indicates if the user account is locked
    public int FailedLoginAttempts { get; private set; } = 0; // Number of failed login attempts
    public bool IsEmailVerified { get; private set; } = false; // Indicates if the user's email is verified
    public string? ProfilePictureUrl { get; private set; } // Optional profile picture URL
    public  string Address { get; private set; } // Optional address for the user   
    public bool IsTenantPrimary { get; private set; } = false; // Indicates if the user is a tenant primarty user so it can be deleted.
    public TenantEntity Tenant { get; private set; }
    public Guid TenantId { get; private set; }
    private List<PermissionsEntity> _Permissions { get; set; } = new(); // Navigation property for PermissionsEntity

    public IReadOnlyList<PermissionsEntity> Permissions => _Permissions; // Read-only collection of permissions

    private UserEntity() { } // Private constructor to enforce the use of the factory method

    public void AddPermission(PermissionsEntity permission)
    {
        _Permissions.Add(permission);
    }

    public bool RemovePermission(string name)
    {
        var existingPermission = _Permissions.FirstOrDefault(p=> p.Name == name);
        if (existingPermission != null) {
            _Permissions.Remove(existingPermission);
            return true;
        }
        return false;
    }

    public bool RemovePermission(PermissionsEntity permission)
    {
        if (_Permissions.Contains(permission))
        {
            _Permissions.Remove(permission);
            return true;
        }
        return false;
    }

    public bool RemovePermission(Guid id)
    {
        var existingPermission = _Permissions.FirstOrDefault(p => p.Id == Id);
        if (existingPermission != null)
        {
            _Permissions.Remove(existingPermission);
            return true;
        }
        return false;
    }

    public void AddPermissions(IEnumerable<PermissionsEntity> permission)
    {
        _Permissions.AddRange(permission);
    }
    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }

    public void IncreaseFailedLogin()
    { 
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5) // Lock account after 5 failed attempts
        {
            IsLocked = true;
            return;
        }       
    }
    public void ResetFailedLogin()
    {
        FailedLoginAttempts = 0;
        IsLocked = false;
    }
    public static UserEntity Create(
        string password,
        string email,
        string phoneNumber,
        string firstName,
        string lastName,
        string address,
        Guid TenantId,
        bool isTenentPrimary = false)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            Password = password,
            Email = email,
            PhoneNumber = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            Address = address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,    
            TenantId = TenantId,
            IsTenantPrimary = isTenentPrimary
        };
    }
    public static UserEntity Create(
    string password,
    string email,
    string phoneNumber,
    string firstName,
    string lastName,
    string address,
    TenantEntity tenant,
    bool isTenentPrimary = false
        )
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant), "Tenant cannot be null.");
        }
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            Password = password,
            Email = email,
            PhoneNumber = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            Address = address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            TenantId = tenant.Id,
            Tenant = tenant,
            IsTenantPrimary = isTenentPrimary
        };
    }
    public void VerifyEmail()
    {
        IsEmailVerified = true;       
    }
}
