using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Enums;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Text.RegularExpressions;

namespace EcommerceApi.Controllers;

[ApiController]
public sealed class CategoryController(
        AppDbContext db,
        IUserProvider userProvider,
        IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IWebHostEnvironment _env = env;

    private const string UploadDir = "uploads";

    private static string Slugify(string name) =>
        Regex.Replace(name.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');


    private static CategoryDto ToDto(Category c) => new(
        c.CategoryId, c.ParentId, c.Name, c.Description,
        c.Slug, c.Status, c.IsFeatured, c.DisplayOrder,
        c.TenantId, c.FeaturedImageId,
        c.FeaturedImage?.Uri,c.IconClass,c.ColorTheme,
        c.MetaTitle,c.MetaDescription, c.ProductCategories?.Count ?? 0,
	    c.CreatedAtUtc,c.UpdatedAtUtc
		);


    /* ──────────────── READ ──────────────── */

    [AppAuthorize(FeatureFactory.Category.CanGetCategory)]
    [HttpGet]
    [Route(Endpoints.Categories.GetAll)]
    public async Task<ActionResult> GetAllCategories(
     int pageNumber = 1,
     int pageSize = 10,
     CancellationToken ct = default)
    {
        if (pageNumber <= 0 || pageSize <= 0)
        {
            ModelState.AddModelError(nameof(pageNumber), "PageNumber must be > 0.");
            ModelState.AddModelError(nameof(pageSize), "PageSize must be > 0.");
            return this.ApplicationProblem(
                detail: "PageNumber and PageSize must be greater than zero.",
                title: "Invalid Pagination Parameters",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.InvalidPaginationParameters,
                instance: HttpContext.Request.Path);
        }

        var query = _db.Categories
                       .Include(c => c.FeaturedImage)
                       .Include(c => c.ProductCategories)
					   .OrderBy(c => c.CreatedAtUtc)
					   .ThenBy(c => c.DisplayOrder)
                       .ThenBy(c => c.Name)
                       .AsNoTracking();                       ;

     
        var totalCategories = await query.CountAsync(ct);
        var activeCategories = await query.CountAsync(c => c.Status == CategoryStatus.Active, ct);
        var featuredCategories = await query.CountAsync(c => c.IsFeatured, ct);

        var productsInCategories = await _db.ProductCategories
                                            .Select(pc => pc.ProductId)
                                            .Distinct()
                                            .CountAsync(ct);

   
        var pagedList = await query
                             .Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync(ct);

    
        return Ok(new PagedCategoryResponse
        {
            Categories = pagedList.Select(ToDto),
            TotalCount = totalCategories,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActiveCategories = activeCategories,
            FeaturedCategories = featuredCategories,
            ProductsInCategories = productsInCategories,
            TotalCategories = totalCategories
		});
	}


	[AppAuthorize(FeatureFactory.Category.CanGetCategory)]
    [HttpGet]
    [Route(Endpoints.Categories.GetById)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var cat = await _db.Categories
                           .Include(c => c.FeaturedImage)
                           .AsNoTracking()
                           .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
      
        if (cat is null)
        {
            ModelState.AddModelError(nameof(id), "category id not found");
            return this.ApplicationProblem(
                detail: $"Category '{id}' not found.",
                title: "Category Not Found",
                statusCode: StatusCodes.Status404NotFound,
                 modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path
            );
        }
        return Ok(ToDto(cat));
    }


    /* ──────────────── CREATE ──────────────── */

    [AppAuthorize(FeatureFactory.Category.CanAddCategory)]
    [HttpPost]
    [Route(Endpoints.Categories.Create)]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest req,
        CancellationToken ct)
    {
        /* validation block */
        if (req.ParentId is not null &&
            !await _db.Categories.AnyAsync(c => c.CategoryId == req.ParentId, ct))
        {
            ModelState.AddModelError(nameof(req.ParentId), $"Parent category {req.ParentId} does not exist.");
        }

        string slug = Slugify(req.Name);
        if (await _db.Categories.AnyAsync(c => c.Slug == slug, ct))
        {
            ModelState.AddModelError(nameof(req.Name), $"Slug '{slug}' already in use.");
        }

        if (!ModelState.IsValid)
        {
            return this.ApplicationProblem(
                detail: "One or more validation errors occurred.",
                title: "Validation Error",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.ValidationFailed,
                instance: HttpContext.Request.Path
            );
        }

        /* create entity */
        var cat = new Category
        {
            CategoryId = Guid.NewGuid(),
            TenantId = userProvider.TenantId,
            ParentId = req.ParentId,
            Name = req.Name,
            Description = req.Description,
            IconClass = req.IconClass,
            ColorTheme = req.ColorTheme,
            IsFeatured = req.IsFeatured,
            DisplayOrder = req.DisplayOrder,
            Slug = slug,
            MetaTitle = req.MetaTitle,
            MetaDescription = req.MetaDescription,
            FeaturedImageId = req.FeaturedImageId,
            CreatedBy = userProvider.UserId,
            UpdatedBy = userProvider.UserId           
        };

        _db.Categories.Add(cat);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = cat.CategoryId }, ToDto(cat));
    }


    /* ──────────────── UPDATE ──────────────── */

    [AppAuthorize(FeatureFactory.Category.CanAddCategory)]
    [HttpPut]
    [Route(Endpoints.Categories.Update)]
    public async Task<IActionResult> Update(Guid id,
        [FromBody] UpdateCategoryRequest req,
        CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        if (cat is null)
        {
            ModelState.AddModelError(nameof(id), "Category Id not found.");
            return this.ApplicationProblem(
                detail: $"Category '{id}' not found.",
                title: "Category Not Found",
                statusCode: StatusCodes.Status404NotFound,
                 modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path
            );
        }

        /* validation */
        if (req.ParentId == id)
        {
            ModelState.AddModelError(nameof(req.ParentId), "Category cannot be its own parent.");
        }

        if (req.ParentId is not null &&
            !await _db.Categories.AnyAsync(c => c.CategoryId == req.ParentId, ct))
        {
            ModelState.AddModelError(nameof(req.ParentId), "Parent category not found.");
        }

        string newSlug = Slugify(req.Name);
        if (newSlug != cat.Slug &&
            await _db.Categories.AnyAsync(c => c.Slug == newSlug && c.CategoryId != id, ct))
        {
            ModelState.AddModelError(nameof(req.Name), $"Slug '{newSlug}' already in use.");
        }

        if (req.FeaturedImageId is not null &&
            req.FeaturedImageId != cat.FeaturedImageId &&
            !await _db.MediaFiles.AnyAsync(m => m.MediaFileId == req.FeaturedImageId, ct))
        {
            ModelState.AddModelError(nameof(req.FeaturedImageId), "FeaturedImageId not found.");
        }

        if (!ModelState.IsValid)
        {
            return this.ApplicationProblem(
                detail: "One or more validation errors occurred.",
                title: "Validation Error",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.ValidationFailed,
                instance: HttpContext.Request.Path
            );
        }

        /* apply updates */
        cat.ParentId = req.ParentId;
        cat.Name = req.Name;
        cat.Description = req.Description;
        cat.IconClass = req.IconClass;
        cat.ColorTheme = req.ColorTheme;
        cat.IsFeatured = req.IsFeatured;
        cat.DisplayOrder = req.DisplayOrder;
        cat.Slug = newSlug;
        cat.MetaTitle = req.MetaTitle;
        cat.MetaDescription = req.MetaDescription;
        cat.Status = req.Status;
        cat.FeaturedImageId = req.FeaturedImageId;
        cat.UpdatedAtUtc = DateTime.UtcNow;
        cat.UpdatedBy = userProvider.UserId;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }


    /* ──────────────── DELETE ──────────────── */

    [AppAuthorize(FeatureFactory.Category.CanRemoveCategory)]
    [HttpDelete]
    [Route(Endpoints.Categories.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var cat = await _db.Categories
                           .Include(c => c.Children)
                           .Include(c => c.FeaturedImage)
                           .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        if (cat is null)
        {
            ModelState.AddModelError(nameof(id), "Category not found.");
            return this.ApplicationProblem(
                detail: $"Category '{id}' not found.",
                title: "Category Not Found",
                statusCode: StatusCodes.Status404NotFound,
                 modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path
            );
        }

        if (cat.Children.Any())
        {
            ModelState.AddModelError(nameof(id), "Category has child categories; remove or re‑parent them first.");
            return this.ApplicationProblem(
                detail: "Category has child categories; remove or re‑parent them first.",
                title: "Delete Denied",
                statusCode: StatusCodes.Status400BadRequest,
                 modelState: ModelState,
                errorCode: ErrorCodes.InvalidState,
                instance: HttpContext.Request.Path
            );
        }

        if (await _db.ProductCategories.AnyAsync(pc => pc.CategoryId == id, ct))
        {
            ModelState.AddModelError(nameof(id), "Category is assigned to products.");
            return this.ApplicationProblem(
                detail: "Category is assigned to products; unassign before deleting.",
                title: "Delete Denied",
                statusCode: StatusCodes.Status400BadRequest,
                 modelState: ModelState,
                errorCode: ErrorCodes.InvalidState,
                instance: HttpContext.Request.Path
            );
        }

        var img = cat.FeaturedImage;

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync(ct);

        if (img is not null &&
            !await _db.Categories.AnyAsync(c => c.FeaturedImageId == img.MediaFileId, ct))
        {
            _db.MediaFiles.Remove(img);
            await _db.SaveChangesAsync(ct);

            var fullPath = Path.Combine(_env.WebRootPath, UploadDir, Path.GetFileName(img.Uri));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        return NoContent();
    }
}
