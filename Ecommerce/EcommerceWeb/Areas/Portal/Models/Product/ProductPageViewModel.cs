using EcommerceWeb.Areas.Portal.Models.Categories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceWeb.Areas.Portal.Models.Product;

public class ProductPageViewModel
{
    public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
    public ProductViewModel NewProduct { get; set; } = new();
    public IEnumerable<SelectListItem> Categories { get; internal set; } = new List<SelectListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string TotalProducts { get; set; }
    public string ActiveListings { get; set; }
    public string LowStock { get; set; }
    public string OutOfStock { get; set; }
}
