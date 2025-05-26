namespace Models.RequestModels;

public class RemovePermisstionRequest
{
    public required Guid UserId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class RemovePermisstionByNameRequest
{
    public required Guid UserId { get; set; }
    public required string Permission { get; set; }
}