namespace EcommerceWeb.Areas.Portal.Models.Product;

public class ProductPageViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();
    public ProductViewModel NewProduct { get; set; } = new();
}
