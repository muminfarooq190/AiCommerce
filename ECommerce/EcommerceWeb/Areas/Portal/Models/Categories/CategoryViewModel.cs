using EcommerceApi.Enums;
using Sheared.Enums;

namespace EcommerceWeb.Areas.Portal.Models.Categories;

public class CategoryViewModel
{
	public Guid categoryId { get; set; }
	public Guid parentId { get; set; }
	public string name { get; set; }
	public string description { get; set; }
	public string slug { get; set; }
	public CategoryStatus status { get; set; }
	public bool isFeatured { get; set; }
	public int displayOrder { get; set; }
	public Guid tenantId { get; set; }
	//public Guid featuredImageId { get; set; }
	//public string featuredImageUri { get; set; }
	public string IconClass { get; set; }
	public string ColorTheme { get; set; }
	public string MetaTitle { get; set; }
	public string MetaDescription { get; set; }
}
