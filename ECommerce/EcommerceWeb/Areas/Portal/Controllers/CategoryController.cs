using EcommerceWeb.Areas.Portal.Models.Categories;
using EcommerceWeb.Extentions;
using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceWeb.Areas.Portal.Controllers;

[Area("Portal")]
[Authorize]
[Route("portal/[controller]")]

public class CategoryController(IApiClient apiClient, ILogger<CategoryController> _logger) : Controller
{
	private async Task<CategoryPageViewModel> getCategoriesData(int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		var url = Endpoints.Categories.GetAll + $"?pagenumber={pageNumber}&pagesize={pageSize}";
		if (!string.IsNullOrEmpty(filter) && filter != "all")
			url += $"&filter={filter}";
		if (!string.IsNullOrEmpty(search))
			url += $"&search={Uri.EscapeDataString(search)}";
		url += $"&sort={sort}";
		var response = await apiClient.GetAsync<PagedCategoryResponse>(url);

		ModelState.AddApiResult(response);

		if (response.ResultType != ResultType.Success || response.Data == null)
		{
			ViewData["ServerError"] = $"{response.Details ?? response.Title ?? "Something went wrong"}";
			return new CategoryPageViewModel();
		}

		var categories = response.Data.Categories.Select(a => new CategoryViewModel
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
			ColorTheme = a.ColorTheme,
			FeaturedImageId = a.FeaturedImageId,
			FeaturedImageUri = a.FeaturedImageUri,
			IconClass = a.IconClass ?? string.Empty,
			MetaTitle = a.MetaTitle ?? string.Empty,
			MetaDescription = a.MetaDescription ?? string.Empty,
			ProductCount = a.ProductCount,
			CreatedAtUtc = a.CreatedAtUtc,
			UpdatedAtUtc = a.UpdatedAtUtc
		}).ToList();

		return new CategoryPageViewModel
		{
			Categories = categories,
			PageNumber = response.Data.PageNumber,
			PageSize = response.Data.PageSize,
			ActiveCategories = response.Data.ActiveCategories,
			FeaturedCategories = response.Data.FeaturedCategories,
			ProductsInCategories = response.Data.ProductsInCategories,
			TotalCategories = response.Data.TotalCategories,
			TotalCount = response.Data.TotalCount
		};
	}

	[HttpGet]
	public async Task<ActionResult> Index(int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		var model = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
		model.Filter = filter;
		model.SearchTerm = search;
		model.Sort = sort;
		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> Index(CategoryPageViewModel categoryModel, int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		if (!ModelState.IsValid)
		{
			TempData["StatusMessage"] = "0";
			categoryModel = await getCategoriesData(1, 10);
			return View(categoryModel);
		}
		if (categoryModel.Category.FeaturedImageId == null && (categoryModel.Category.ImageFile == null || categoryModel.Category.ImageFile.Length == 0))
		{
			ModelState.AddModelError("Category.ImageFile", "Featured image is required.");
			TempData["StatusMessage"] = "0";
			categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
			return View(categoryModel);
		}
		Guid? featuredImageId = null;

		string featuredImageUri = string.Empty;
		if (categoryModel.Category?.ImageFile != null && categoryModel.Category.ImageFile.Length > 0)
		{
			var uploadResponse = await apiClient.PostMultipartAsync<MediaFileDto>(
				Endpoints.Media.Upload,
				categoryModel.Category.ImageFile
			);
			ModelState.AddApiResult(uploadResponse);

			if (uploadResponse.ResultType != ResultType.Success)
			{
				ViewData["ServerError"] = $"{uploadResponse.Details ?? uploadResponse.Title ?? "Something went wrong"}";
				categoryModel = await getCategoriesData(1, 10);
				TempData["StatusMessage"] = "0";
				_logger.LogError("Image upload failed: {Errors}", uploadResponse.Errors?.ToString());
				return View(categoryModel);
			}

			featuredImageId = uploadResponse.Data?.MediaFileId;
		}

		CreateCategoryRequest modelAddCategory = new CreateCategoryRequest(
			categoryModel?.Category?.name,
			categoryModel?.Category?.slug,
			categoryModel?.Category?.parentId == Guid.Empty ? null : categoryModel?.Category.parentId,
			categoryModel?.Category?.description,
			categoryModel?.Category?.IconClass,
			categoryModel?.Category?.ColorTheme,
			featuredImageId,
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
			ViewData["ServerError"] = $"{response.Details ?? response.Title ?? "Something went wrong"}";

			TempData["StatusMessage"] = "0";
			categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
			_logger.LogError("Category Added failed: {Errors}", response.Errors.ToString());
			return View(categoryModel);
		}
		else
		{
			TempData["StatusMessage"] = "created";
			return RedirectToAction("Index", new { pageNumber, pageSize, filter, search, sort });
		}


	}
	[HttpGet]
	[Route("Index/{id}")]
	public async Task<ActionResult> Index(Guid id, int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		var endpoint = Endpoints.Categories.GetById.Replace("{id:guid}", id.ToString());
		var response = await apiClient.GetAsync<CategoryDto>(endpoint);
		ModelState.AddApiResult(response);
		if (response.ResultType != ResultType.Success)
		{
			ViewData["ServerError"] = $"{response.Title ?? response.Details ?? "Something went wrong"}";
			_logger.LogError("Failed to load category: {Errors}", response.Errors?.ToString());
			CategoryPageViewModel categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
			return View(categoryModel);
		}

		var model = new CategoryPageViewModel
		{
			Category = new CategoryViewModel
			{
				categoryId = response.Data.CategoryId,
				parentId = response.Data.ParentId ?? Guid.Empty,
				name = response.Data.Name,
				description = response.Data.Description,
				slug = response.Data.Slug,
				status = response.Data.Status,
				isFeatured = response.Data.IsFeatured,
				displayOrder = response.Data.DisplayOrder,
				tenantId = response.Data.TenantId,
				FeaturedImageId = response.Data.FeaturedImageId,
				FeaturedImageUri = response.Data.FeaturedImageUri,
				IconClass = response.Data.IconClass,
				ColorTheme = response.Data.ColorTheme,
				MetaTitle = response.Data.MetaTitle,
				MetaDescription = response.Data.MetaDescription,
				CreatedAtUtc = response.Data.CreatedAtUtc,
				UpdatedAtUtc = response.Data.UpdatedAtUtc
			}
		};
		var url = Endpoints.Categories.GetAll + $"?pagenumber={pageNumber}&pagesize={pageSize}";
		if (!string.IsNullOrEmpty(filter) && filter != "all")
			url += $"&filter={filter}";
		if (!string.IsNullOrEmpty(search))
			url += $"&search={Uri.EscapeDataString(search)}";
		url += $"&sort={sort}";
		var categoriesResponse = await apiClient.GetAsync<PagedCategoryResponse>(url);

		if (categoriesResponse.ResultType == ResultType.Success)
		{
			model.Categories = categoriesResponse.Data.Categories
			.Select(c => new CategoryViewModel
			{
				categoryId = c.CategoryId,
				parentId = c.ParentId,
				name = c.Name,

			}).ToList();
		}

		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Route("Index/{id}")]
	public async Task<ActionResult> Index(Guid id, CategoryPageViewModel categoryModel, int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		try
		{
			if (!ModelState.IsValid)
			{
				categoryModel = await getCategoriesData(1, 5);
				TempData["StatusMessage"] = "1";
				return View(categoryModel);
			}
			if (categoryModel.Category.FeaturedImageId == null && (categoryModel.Category.ImageFile == null || categoryModel.Category.ImageFile.Length == 0))
			{
				ModelState.AddModelError("Category.ImageFile", "Featured image is required.");
				TempData["StatusMessage"] = "1";
				categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
				return View(categoryModel);
			}
			Guid? featuredImageId = categoryModel.Category.FeaturedImageId;

			if (categoryModel.Category?.ImageFile != null && categoryModel.Category.ImageFile.Length > 0)
			{
				var uploadResponse = await apiClient.PostMultipartAsync<MediaFileDto>(
					Endpoints.Media.Upload,
					categoryModel.Category.ImageFile
				);
				ModelState.AddApiResult(uploadResponse);

				if (uploadResponse.ResultType != ResultType.Success)
				{
					ViewData["ServerError"] = $"{uploadResponse.Details ?? uploadResponse.Title ?? "Something went wrong"}";
					_logger.LogError("Image upload failed: {Errors}", uploadResponse.Errors?.ToString());
					categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
					TempData["StatusMessage"] = "1";
					return View(categoryModel);
				}

				featuredImageId = uploadResponse.Data?.MediaFileId;
			}

			UpdateCategoryRequest updateRequest = new UpdateCategoryRequest(
					categoryModel.Category.tenantId,
					categoryModel.Category.name,
					categoryModel.Category.slug,
					categoryModel.Category.parentId == Guid.Empty ? null : categoryModel.Category.parentId,
					categoryModel.Category.description,
					categoryModel.Category.IconClass,
					categoryModel.Category.ColorTheme,
					featuredImageId,
					categoryModel.Category.isFeatured,
					categoryModel.Category.displayOrder,
					categoryModel.Category.MetaTitle,
					categoryModel.Category.MetaDescription,
					categoryModel.Category.status
				);

			var endpoint = Endpoints.Categories.Update.Replace("{id:guid}", id.ToString());

			var response = await apiClient.PutAsync<UpdateCategoryRequest, CategoryDto>(endpoint, updateRequest);
			ModelState.AddApiResult(response);

			if (response.ResultType != ResultType.Success)
			{
				ViewData["ServerError"] = $"{response.Details ?? response.Title ?? "Something went wrong"}";
				_logger.LogError("Category Update failed: {Errors}", response.Errors?.ToString());
				categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
				TempData["StatusMessage"] = "1";
				return View(categoryModel);
			}
			TempData["StatusMessage"] = "updated";
			return RedirectToAction("Index", new { pageNumber, pageSize, filter, search, sort });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Error updating category ID: {id}");
			categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
			TempData["StatusMessage"] = "error";
			return View(categoryModel);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Route("Delete/{id}")]
	public async Task<ActionResult> Delete(Guid id, int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		var endpoint = Endpoints.Categories.Update.Replace("{id:guid}", id.ToString());
		var response = await apiClient.DeleteAsync<object>(endpoint);
		ModelState.AddApiResult(response);
		if (response.ResultType != ResultType.Success)
		{
			CategoryPageViewModel categoryModel = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
			ViewData["ServerError"] = $"{response.Title ?? response.Details ?? "Something went wrong"}";
			_logger.LogError("Failed to delete category", response.Errors?.ToString());
			return View("Index", categoryModel);
		}
		TempData["StatusMessage"] = "deleted";
		return RedirectToAction("Index", new { pageNumber, pageSize, filter, search, sort });
	}
	[HttpGet("Search")]
	public async Task<IActionResult> Search(int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		var model = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
		return Json(new
		{
			categories = model.Categories.Select(c => new
			{
				c.categoryId,
				c.name,
				c.parentId,
				c.ProductCount,
				c.status,
				c.isFeatured,
				c.CreatedAtUtc,
				c.UpdatedAtUtc
			}),
			totalCount = model.TotalCount
		});
	}

	[HttpGet("TablePartial")]
	public async Task<IActionResult> TablePartial(int pageNumber = 1, int pageSize = 5, string filter = "all", string search = null, string sort = "asc")
	{
		var model = await getCategoriesData(pageNumber, pageSize, filter, search, sort);
		return PartialView("_CategoryTable", model);
	}
}
