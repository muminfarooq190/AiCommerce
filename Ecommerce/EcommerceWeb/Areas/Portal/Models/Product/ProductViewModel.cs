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
    public IEnumerable<Guid> CategoryIds { get; internal set; }
    public string Barcode { get; internal set; }
    public Guid? BrandId { get; internal set; }
    public decimal? CompareAtPrice { get; internal set; }
    public decimal? CostPerItem { get; internal set; }
    public decimal? LengthCm { get; internal set; }
    public decimal? WidthCm { get; internal set; }
    public decimal? HeightCm { get; internal set; }
    public string MetaDescription { get; internal set; }
    public string MetaTitle { get; internal set; }
    public bool QualifiesForFreeShipping { get; internal set; }
    public string SKU { get; internal set; }
    public bool TrackInventory { get; internal set; }
    public decimal? WeightKg { get; internal set; }
}