using Ecommerce.Entities;

namespace EcommerceApi.Entities;

public class Permission : IBaseEntity
{ 
    private Permission() { }
    public Guid PermissionId { get; private set; } 
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public User? User { get; private set; }
    public Guid UserId { get; private set; }
    public required Guid TenantId { get; set; }
 
    public static Permission Create(string name)
    {
        return new Permission
        {
            PermissionId = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    public static Permission Create(string name,User user)
    {
        return new Permission
        {
            PermissionId = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = user,
            UserId = user.UserId
        };
    }
    public static Permission Create(string name,  Guid userid)
    {
        return new Permission
        {
            PermissionId = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userid
        };
    }
    public void Update(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
