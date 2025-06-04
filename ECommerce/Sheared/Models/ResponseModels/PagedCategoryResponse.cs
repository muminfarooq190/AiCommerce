using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record PagedCategoryResponse
    {
        public required IEnumerable<CategoryDto> Categories { get; init; }
        public required int TotalCount { get; init; }
        public required int PageNumber { get; init; }
        public required int PageSize { get; init; }

        // dashboard stats
        public required int ActiveCategories { get; init; }
        public required int FeaturedCategories { get; init; }
        public required int ProductsInCategories { get; init; }
        public required int TotalCategories { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

}
