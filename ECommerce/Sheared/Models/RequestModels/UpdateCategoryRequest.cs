using EcommerceApi.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record UpdateCategoryRequest
    (
        Guid TenantId,
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
