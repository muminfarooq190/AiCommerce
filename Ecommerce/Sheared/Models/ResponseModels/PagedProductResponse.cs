namespace Sheared.Models.ResponseModels;

public class PagedProductResponse
{
    public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public int ActiveListings { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}
