namespace EcommerceApi.Entities;

public interface IBaseEntity
{
    Guid TenantId { get; set; }
}
