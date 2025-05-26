namespace EcommerceApi.Providers;
public interface ITenantProvider
{
    Guid? TenantId { get; }
}