namespace Sheared.Models.ResponseModels;

public class GetPermisstionResponse
{
    public Guid PermissionId { get;  set; }
    public required string Name { get;  set; }
    public DateTime CreatedAt { get;  set; }
    public DateTime UpdatedAt { get;  set; }
}
