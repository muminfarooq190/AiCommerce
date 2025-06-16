namespace EcommerceWeb.Areas.Portal.Models.Categories;

public class CategoryPageViewModel
{
	public List<CategoryViewModel> Categories { get; set; } = new();
	public CategoryViewModel? Category { get; set; } = null;
	public string? SearchTerm { get; set; } = null;
	public string? Sort { get; set; } = null;
	public string? Filter { get; set; } = null;

	public int ActiveCategories { get; init; }
	public int FeaturedCategories { get; init; }
	public int ProductsInCategories { get; init; }
	public int TotalCategories { get; init; }
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public int TotalCount { get; set; } = 0;
	public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

}
