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
		 string? FeaturedImageUri,
		 string IconClass,
         string ColorTheme,
         string MetaTitle,
	     string MetaDescription,
         int ProductCount,       
         DateTime CreatedAtUtc,	    
	     DateTime? UpdatedAtUtc
	);

}
