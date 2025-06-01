using Sheared.Enums;

namespace Sheared.Models.ResponseModels;

public sealed record ProductDto(
   Guid ProductId,
   string Name,
   string SKU,
   ProductStatus Status,
   decimal Price,
   decimal? CompareAtPrice,
   decimal? CostPerItem,
   int StockQuantity,
   bool TrackInventory,
   bool QualifiesForFreeShipping,
   Guid? BrandId,
   string? BrandName,
   string? Description,
   IEnumerable<Guid> CategoryIds,
   IEnumerable<ProductImageDto> Images,
   string Slug,
   Guid TenantId,
   DateTime CreatedAtUtc,
   DateTime UpdatedAtUtc);
