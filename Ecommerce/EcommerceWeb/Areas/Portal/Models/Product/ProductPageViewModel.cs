namespace EcommerceWeb.Areas.Portal.Models.Product;

public class ProductPageViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();
    public ProductViewModel NewProduct { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string TotalProducts { get; set; }
    public string ActiveListings { get; set; }
    public string LowStock { get; set; }
    public string OutOfStock { get; set; }
}
