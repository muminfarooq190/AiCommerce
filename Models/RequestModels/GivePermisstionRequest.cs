namespace Models.RequestModels;

public class GivePermisstionRequest
{
    public required Guid UserId { get; set; }
    public required string Permission { get; set; }
}
