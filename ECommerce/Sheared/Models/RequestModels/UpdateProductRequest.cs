using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record UpdateProductRequest(
        string Name,
        string SKU,
        ProductStatus Status,
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
