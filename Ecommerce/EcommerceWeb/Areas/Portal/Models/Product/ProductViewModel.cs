namespace EcommerceWeb.Areas.Portal.Models.Product;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int StockQuantity { get; set; }
    public List<string> Images { get; set; }
    public string StockStatus { get; set; }
}