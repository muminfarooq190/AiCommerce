using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Enums;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EcommerceApi.Controllers;

[ApiController]
public sealed class ProductController(
        AppDbContext db,
        IUserProvider userProvider,
        IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IWebHostEnvironment _env = env;

    private const string UploadDir = "uploads/products";
    private static string Slugify(string name) =>
        Regex.Replace(name.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');


    private static ProductDto ToDto(Product p) => new(
        p.ProductId,
        p.Name,
        p.SKU,
        p.Status,
        p.Price,
        p.CompareAtPrice,
        p.CostPerItem,
        p.StockQuantity,
        p.TrackInventory,
        p.QualifiesForFreeShipping,
        p.BrandId,
        p.Brand?.Name,
        p.Description,
        p.ProductCategories.Select(pc => pc.CategoryId),
        p.ProductImages
         .OrderBy(i => i.SortOrder)
         .Select(i => new ProductImageDto(i.MediaFileId, i.MediaFile.Uri, i.SortOrder)),
        p.Slug!,
        p.TenantId,
        p.CreatedAtUtc,
        p.UpdatedAtUtc);

    [AppAuthorize(FeatureFactory.Product.CanGetProduct)]
    [HttpGet]
    [Route(Endpoints.Products.GetAll)]
    [HttpGet]
    public async Task<ActionResult> GetAll(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (pageNumber <= 0 || pageSize <= 0)
        {
            ModelState.AddModelError(nameof(pageNumber), "PageNumber must be greater than zero.");
            ModelState.AddModelError(nameof(pageSize), "PageSize must be greater than zero.");
            return this.ApplicationProblem(
                detail: "Page and PageSize must be greater than zero.",
                title: "Invalid Pagination Parameters",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.InvalidPaginationParameters,
                instance: HttpContext.Request.Path
            );
        }

        var query = _db.Products
                       .Include(p => p.Brand)
                       .Include(p => p.ProductCategories)
                       .Include(p => p.ProductImages)
                           .ThenInclude(pi => pi.MediaFile)
                       .OrderBy(p => p.Name)
                       .AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var pagedList = await query
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(ct);

        return Ok(new
        {
            Products = pagedList.Select(ToDto),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [AppAuthorize(FeatureFactory.Product.CanGetProduct)]
    [HttpGet]
    [Route(Endpoints.Products.GetById)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _db.Products
                               .Include(p => p.Brand)
                               .Include(p => p.ProductCategories)
                               .Include(p => p.ProductImages).ThenInclude(pi => pi.MediaFile)
                               .AsNoTracking()
                               .FirstOrDefaultAsync(p => p.ProductId == id , ct);
        return product is null ? NotFound() : Ok(ToDto(product));
    }

    [AppAuthorize(FeatureFactory.Product.CanAddProduct)]
    [HttpPost]
    [Route(Endpoints.Products.Create)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest req, CancellationToken ct)
    {

        if (await _db.Products.AnyAsync(p => p.SKU == req.SKU , ct))
            return Conflict($"SKU '{req.SKU}' already exists.");


        if (req.BrandId is not null &&
            !await _db.Brands.AnyAsync(b => b.BrandId == req.BrandId , ct))
            return BadRequest("Brand not found.");

        var catIds = req.CategoryIds?.Distinct().ToList() ?? [];
        if (catIds.Any())
        {
            var missing = catIds.Except(
                await _db.Categories.Where(c => catIds.Contains(c.CategoryId) )
                                    .Select(c => c.CategoryId).ToListAsync(ct));
            if (missing.Any())
                return BadRequest($"Categories not found: {string.Join(',', missing)}");
        }


        string slug = Slugify(req.Name);
        if (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
            slug = $"{slug}-{Guid.NewGuid():N[..5]}";   

        var p = new Product
        {
            ProductId = Guid.NewGuid(),
            TenantId = userProvider.TenantId,
            Name = req.Name,
            SKU = req.SKU,
            Slug = slug,
            BrandId = req.BrandId,
            Description = req.Description,
            Price = req.Price,
            CompareAtPrice = req.CompareAtPrice,
            CostPerItem = req.CostPerItem,
            WeightKg = req.WeightKg,
            LengthCm = req.LengthCm,
            WidthCm = req.WidthCm,
            HeightCm = req.HeightCm,
            QualifiesForFreeShipping = req.QualifiesForFreeShipping,
            StockQuantity = req.StockQuantity,
            Barcode = req.Barcode,
            TrackInventory = req.TrackInventory,
            MetaTitle = req.MetaTitle,
            MetaDescription = req.MetaDescription,
            CreatedBy = userProvider.UserId,
            UpdatedBy = userProvider.UserId
        };

        _db.Products.Add(p);


        foreach (var cid in catIds)
            _db.ProductCategories.Add(new ProductCategory
            {
                ProductId = p.ProductId,
                CategoryId = cid,
                TenantId = p.TenantId
            });

        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = p.ProductId }, ToDto(p));
    }


    [AppAuthorize(FeatureFactory.Product.CanAddProduct)]
    [HttpPut]
    [Route(Endpoints.Products.Update)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest req,
        CancellationToken ct)
    {
        var p = await _db.Products
                         .Include(p => p.ProductCategories)
                         .FirstOrDefaultAsync(p => p.ProductId == id, ct);
        if (p is null) return NotFound();


        if (req.SKU != p.SKU &&
            await _db.Products.AnyAsync(x => x.SKU == req.SKU && x.ProductId != id, ct))
            return Conflict($"SKU '{req.SKU}' already exists.");

        if (req.BrandId != p.BrandId)
        {
            if (req.BrandId is not null &&
                !await _db.Brands.AnyAsync(b => b.BrandId == req.BrandId, ct))
                return BadRequest("Brand not found.");
            p.BrandId = req.BrandId;
        }

        var incoming = req.CategoryIds?.Distinct().ToList() ?? [];
        var current = p.ProductCategories.Select(pc => pc.CategoryId).ToList();
        var toAdd = incoming.Except(current);
        var toRemove = current.Except(incoming);

        if (toAdd.Any())
            foreach (var cid in toAdd)
                _db.ProductCategories.Add(new ProductCategory
                {
                    ProductId = id,
                    CategoryId = cid,
                    TenantId = p.TenantId
                });

        if (toRemove.Any())
        {
            var rows = _db.ProductCategories.Where(pc => pc.ProductId == id &&
                                                         toRemove.Contains(pc.CategoryId));
            _db.ProductCategories.RemoveRange(rows);
        }


        string newSlug = Slugify(req.Name);
        if (newSlug != p.Slug &&
            await _db.Products.AnyAsync(x => x.Slug == newSlug && x.ProductId != id, ct))
            newSlug = $"{newSlug}-{Guid.NewGuid():N[..5]}";

        p.Name = req.Name;
        p.SKU = req.SKU;
        p.Status = req.Status;
        p.Description = req.Description;
        p.Price = req.Price;
        p.CompareAtPrice = req.CompareAtPrice;
        p.CostPerItem = req.CostPerItem;
        p.WeightKg = req.WeightKg;
        p.LengthCm = req.LengthCm;
        p.WidthCm = req.WidthCm;
        p.HeightCm = req.HeightCm;
        p.QualifiesForFreeShipping = req.QualifiesForFreeShipping;
        p.StockQuantity = req.StockQuantity;
        p.Barcode = req.Barcode;
        p.TrackInventory = req.TrackInventory;
        p.MetaTitle = req.MetaTitle;
        p.MetaDescription = req.MetaDescription;
        p.Slug = newSlug;
        p.UpdatedAtUtc = DateTime.UtcNow;
        p.UpdatedBy = userProvider.UserId;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [AppAuthorize(FeatureFactory.Product.CanRemoveProduct)]
    [HttpDelete]
    [Route(Endpoints.Products.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var p = await _db.Products
                         .Include(p => p.ProductImages)
                         .FirstOrDefaultAsync(p => p.ProductId == id, ct);
        if (p is null) return NotFound();


        var images = p.ProductImages.Select(pi => pi.MediaFileId).ToList();

        _db.Products.Remove(p);
        await _db.SaveChangesAsync(ct);

        foreach (var mid in images)
        {
            bool used = await _db.ProductImages.AnyAsync(pi => pi.MediaFileId == mid, ct);
            if (!used)
            {
                var mf = await _db.MediaFiles.FirstAsync(m => m.MediaFileId == mid, ct);
                _db.MediaFiles.Remove(mf);
                await _db.SaveChangesAsync(ct);

                var path = Path.Combine(_env.WebRootPath, UploadDir,
                                        Path.GetFileName(mf.Uri));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
        }
        return NoContent();
    }


    [AppAuthorize(FeatureFactory.Product.CanAddProduct)]
    [HttpPost]
    [Route(Endpoints.Products.ImageUpload)]
    public async Task<ActionResult<ProductDto>> UploadImage(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var p = await _db.Products.FirstOrDefaultAsync(
            p => p.ProductId == id, ct);
        if (p is null) return NotFound();

        Directory.CreateDirectory(Path.Combine(_env.WebRootPath, UploadDir));
        var ext = Path.GetExtension(file.FileName);
        var mediaId = Guid.NewGuid();
        var newName = $"{mediaId}{ext}";
        var fullPath = Path.Combine(_env.WebRootPath, UploadDir, newName);
        await using (var fs = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(fs, ct);
        }


        var media = new MediaFile
        {
            MediaFileId = mediaId,
            FileName = file.FileName,
            MimeType = file.ContentType,
            Uri = $"/{UploadDir}/{newName}",
            TenantId = userProvider.TenantId
        };
        _db.MediaFiles.Add(media);

        var sort = p.ProductImages.Any() ? p.ProductImages.Max(i => i.SortOrder) + 1 : 0;
        _db.ProductImages.Add(new ProductImage
        {
            ProductId = id,
            MediaFileId = mediaId,
            SortOrder = sort,
            TenantId = p.TenantId
        });

        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(await _db.Products
                                 .Include(x => x.Brand)
                                 .Include(x => x.ProductCategories)
                                 .Include(x => x.ProductImages).ThenInclude(pi => pi.MediaFile)
                                 .FirstAsync(x => x.ProductId == id, ct)));
    }

    [AppAuthorize(FeatureFactory.Product.CanRemoveProduct)]
    [HttpDelete]
    [Route(Endpoints.Products.ImageRemove)]
    public async Task<IActionResult> RemoveImage(
        Guid id,
        Guid mediaId,
        CancellationToken ct)
    {
        var link = await _db.ProductImages
                            .Include(pi => pi.MediaFile)
                            .FirstOrDefaultAsync(pi => pi.ProductId == id &&
                                                        pi.MediaFileId == mediaId, ct);
        if (link is null) return NotFound();

        _db.ProductImages.Remove(link);
        await _db.SaveChangesAsync(ct);

        bool used = await _db.ProductImages.AnyAsync(pi => pi.MediaFileId == mediaId, ct);
        if (!used)
        {
            var mf = link.MediaFile;
            _db.MediaFiles.Remove(mf);
            await _db.SaveChangesAsync(ct);

            var path = Path.Combine(_env.WebRootPath, UploadDir,
                                    Path.GetFileName(mf.Uri));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        return NoContent();
    }
}
