using Ecommerce.Entities;

namespace EcommerceApi.Entities;

public class PermissionsEntity
{
    public Guid Id { get; private set; } 
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public UserEntity? User { get; private set; }
    public Guid UserId { get; private set; }

    private PermissionsEntity() { } // EF Core requires a parameterless constructor

    public static PermissionsEntity Create(string name)
    {
        return new PermissionsEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static PermissionsEntity Create(string name,UserEntity user)
    {
        return new PermissionsEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = user,
            UserId = user.Id
        };
    }
    public static PermissionsEntity Create(string name,  Guid userid)
    {
        return new PermissionsEntity
        {
            Id = Guid.NewGuid(),
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
