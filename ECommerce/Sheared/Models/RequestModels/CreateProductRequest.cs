using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record CreateProductRequest(
         string Name,
         string SKU,
         Guid? BrandId,
         IEnumerable<Guid>? CategoryIds,
         string? Description,
         decimal Price,
         decimal? CompareAtPrice,
         decimal? CostPerItem,
         decimal? WeightKg,
         decimal? LengthCm,
         decimal? WidthCm,
         decimal? HeightCm,
         bool QualifiesForFreeShipping,
         int StockQuantity,
         string? Barcode,
         bool TrackInventory,
         string? MetaTitle,
         string? MetaDescription);
}
