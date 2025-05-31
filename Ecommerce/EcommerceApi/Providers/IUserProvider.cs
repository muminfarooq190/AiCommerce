namespace EcommerceApi.Providers;
public interface IUserProvider
{
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public string Email { get; }
    public string CompanyName { get; }
    public bool IsAuthenticated { get; }
}