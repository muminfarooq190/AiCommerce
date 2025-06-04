using EcommerceWeb.Areas.Portal.Models.Product;
using EcommerceWeb.Extentions;
using EcommerceWeb.Services.Contarcts;
using EcommerceWeb.Utilities.ApiResult.Enums;
using Microsoft.AspNetCore.Mvc;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Net;

namespace EcommerceWeb.Areas.Portal.Controllers;

[Area("Portal")]
public class ProductController(IApiClient apiClient, ILogger<ProductController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        var apiResponce = await apiClient.GetAsync<PagedProductResponse>(Endpoints.Products.GetAll);

        ModelState.AddApiResult(apiResponce);

        if (apiResponce.ResultType != ResultType.Success)
        {
            if (apiResponce.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Index", "Authentication");
            }

            logger.LogError("Login failed: {Errors}", apiResponce.Errors.ToString());

            return View(new ProductPageViewModel());
        }

        var ViewModel = new ProductPageViewModel()
        {
            PageNumber = apiResponce.Data.PageNumber,
            PageSize = apiResponce.Data.PageSize,
            TotalCount = apiResponce.Data.TotalCount,
            TotalPages = apiResponce.Data.TotalPages,
            ActiveListings = apiResponce.Data.ActiveListings.ToString("N0"),
            LowStock = apiResponce.Data.LowStock.ToString("N0"),
            OutOfStock = apiResponce.Data.OutOfStock.ToString("N0"),
            TotalProducts = apiResponce.Data.TotalCount.ToString("N0"),
            Products = apiResponce.Data.Products.Select(_ => new ProductViewModel
            {
                Id = _.ProductId,
                Name = _.Name,
                Description = _.Description,
                Images = _.Images.Select(i => i.Uri).ToList(),
                Price = _.Price,
                StockQuantity = _.StockQuantity,
                StockStatus = _.StockQuantity <= 0 ? "Out of Stock" : _.StockQuantity > 5 ? "In Stock" : "Low Stock"
            }).ToList()
        };

        return View(ViewModel);
    }

    public async Task<IActionResult> SaveProduct(ProductPageViewModel productPageViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", productPageViewModel);
        }
        var newProduct = productPageViewModel.NewProduct;
        var payload = new CreateProductRequest(
            Name: newProduct.Name,
            Description: newProduct.Description,
            Price: newProduct.Price,
            StockQuantity: newProduct.StockQuantity,
            CategoryIds: newProduct.CategoryIds,
            Barcode: newProduct.Barcode,
            BrandId: newProduct.BrandId,
            CompareAtPrice: newProduct.CompareAtPrice,
            CostPerItem: newProduct.CostPerItem,
            LengthCm: newProduct.LengthCm,
            WidthCm: newProduct.WidthCm,
            HeightCm: newProduct.HeightCm,
            MetaDescription: newProduct.MetaDescription,
            MetaTitle: newProduct.MetaTitle,
            QualifiesForFreeShipping: newProduct.QualifiesForFreeShipping,
            SKU: newProduct.SKU,
            TrackInventory: newProduct.TrackInventory,
            WeightKg: newProduct.WeightKg
            );

        var apiResponce = await apiClient.PostAsync<CreateProductRequest, ProductDto>(Endpoints.Products.Create, payload);

        ModelState.AddApiResult(apiResponce);
        if (apiResponce.ResultType != ResultType.Success)
        {
            if (apiResponce.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Index", "Authentication");
            }
            logger.LogError("Product creation failed: {Errors}", apiResponce.Errors.ToString());
            return View("Index", productPageViewModel);
        }

        productPageViewModel.NewProduct = new();
        ViewBag.SuccessMessage = "Product saved successfully!";
        return View("Index", productPageViewModel);
    }

    [HttpGet("Portal/Product/Create")]
    public async Task<IActionResult> Create()
    {
        return View();
    }
}
