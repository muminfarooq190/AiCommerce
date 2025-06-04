using EcommerceApi.Enums;
using Sheared.Enums;
using System.ComponentModel.DataAnnotations;

namespace EcommerceWeb.Areas.Portal.Models.Categories;

public class CategoryViewModel
{
	public Guid categoryId { get; set; }
	
	public Guid? parentId { get; set; }
	[Required]
	[Display(Name = "Category Name")]
	public string name { get; set; }
	[Required]
	public string description { get; set; }
	[Required]
	public string slug { get; set; }
	[Required]
	public CategoryStatus status { get; set; }
	[Required]
	public bool isFeatured { get; set; }
	[Required]
	public int displayOrder { get; set; }
	public Guid tenantId { get; set; }
	public Guid? FeaturedImageId { get; set; } 
	public string? FeaturedImageUri { get; set; }
	[Required]
	public string IconClass { get; set; }
	[Required]
	public string ColorTheme { get; set; }
	[Required]
	public string MetaTitle { get; set; }
	[Required]
	public string MetaDescription { get; set; }
	[Required]
	public IFormFile? ImageFile { get; set; }
}
