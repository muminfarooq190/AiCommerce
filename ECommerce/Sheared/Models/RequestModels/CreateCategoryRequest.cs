using EcommerceApi.Enums;

namespace Sheared.Models.RequestModels
{
    public sealed record CreateCategoryRequest
     (
         string Name,
         string Slug,
         Guid? ParentId,
         string? Description,
         string? IconClass,
         string? ColorTheme,
         Guid? FeaturedImageId,
		 bool IsFeatured,
         int DisplayOrder,
         string? MetaTitle,
         string? MetaDescription,
         CategoryStatus Status = CategoryStatus.Active
     );
}
