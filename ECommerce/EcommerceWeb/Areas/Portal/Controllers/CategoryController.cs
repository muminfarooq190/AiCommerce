using Azure;
using EcommerceWeb.Areas.Portal.Models.Categories;
using EcommerceWeb.Extentions;
using EcommerceWeb.Services;
using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult;
using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Sheared;
using Sheared.Enums;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceWeb.Areas.Portal.Controllers;

[Area("Portal")]
[Authorize]
public class CategoryController(IApiClient apiClient,ILogger<CategoryController> _logger) : Controller
{
    public async Task<ActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
		var responce = await apiClient.GetAsync<List<CategoryDto>>(Endpoints.Categories.GetAll + "?pagenumber = 1 & page = 1");
		
		ModelState.AddApiResult(responce);

		if (responce.ResultType != ResultType.Success)
		{
			return View(new CategoryPageViewModel());
		}
		var categories = responce.Data.Select(a => new CategoryViewModel
		{
			categoryId = a.CategoryId,
			parentId = a.ParentId ?? Guid.Empty,
			name = a.Name,
			description = a.Description ?? string.Empty,
			slug = a.Slug,
			status = a.Status,
			isFeatured = a.IsFeatured,
			displayOrder = a.DisplayOrder,
			tenantId = a.TenantId,
			//featuredImageId = a.FeaturedImageId ?? Guid.Empty,
			//featuredImageUri = a.FeaturedImageUri ?? string.Empty

		}).ToList();
		return View(new CategoryPageViewModel
		{
			Categories = categories,
			PageNumber = pageNumber,
			PageSize = pageSize,
			TotalCount = 0
		});
    }

    [HttpPost]
    public async Task<ActionResult> AddCategory(CategoryPageViewModel categoryModel)
	{
		if (!ModelState.IsValid)
		{
			return View(categoryModel);	
		}

		// Map CategoryViewModel to CreateCategoryRequest
		CreateCategoryRequest modelAddCategory = new CreateCategoryRequest(
			categoryModel?.Category?.name,
			categoryModel?.Category?.slug,
			categoryModel?.Category?.parentId == Guid.Empty ? null : categoryModel?.Category.parentId,
			categoryModel?.Category?.description,
			categoryModel?.Category?.IconClass,
			categoryModel?.Category?.ColorTheme,
			Guid.NewGuid(),
			categoryModel.Category.isFeatured,
			categoryModel.Category.displayOrder,
			categoryModel?.Category?.MetaTitle,
			categoryModel?.Category?.MetaDescription,
			categoryModel.Category.status
		);

		var response = await apiClient.PostAsync<CreateCategoryRequest, CategoryDto>(
			Endpoints.Categories.Create, modelAddCategory
		);

		ModelState.AddApiResult(response);

		if (response.ResultType != ResultType.Success)
		{
			 _logger.LogError("Category Added failed: {Errors}", response.Errors.ToString());
			return View(categoryModel);
		}

		return View(new CategoryViewModel());
	}
}
