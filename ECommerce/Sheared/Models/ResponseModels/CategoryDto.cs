using EcommerceApi.Enums;

namespace Sheared.Models.ResponseModels
{
    public sealed record CategoryDto(
         Guid CategoryId,
         Guid? ParentId,
         string Name,
         string? Description,
         string Slug,
         CategoryStatus Status,
         bool IsFeatured,
         int DisplayOrder,
         Guid TenantId,
         Guid? FeaturedImageId,
         string? FeaturedImageUri);

}
