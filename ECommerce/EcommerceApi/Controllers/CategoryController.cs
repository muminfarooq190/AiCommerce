using EcommerceApi.Entities;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoryController(
        AppDbContext db,
        ITenantProvider tenantProvider,
        IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly ITenantProvider _tp = tenantProvider;
    private readonly IWebHostEnvironment _env = env;

    private const string UploadDir = "uploads";
    private const string ClaimNameId = ClaimTypes.NameIdentifier;

    private static string Slugify(string name) =>
        Regex.Replace(name.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimNameId) ??
                   throw new InvalidOperationException($"{ClaimNameId} claim missing"));


    private static CategoryDto ToDto(Category c) => new(
        c.CategoryId, c.ParentId, c.Name, c.Description,
        c.Slug, c.Status, c.IsFeatured, c.DisplayOrder,
        c.TenantId, c.FeaturedImageId,
        c.FeaturedImage?.Uri);


    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var list = await _db.Categories
                            .Include(c => c.FeaturedImage)
                            .Where(c => c.TenantId == _tp.TenantId)
                            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
                            .AsNoTracking().ToListAsync(ct);

        return Ok(list.Select(ToDto));
    }

    [HttpGet]
    [Route(Endpoints.Categories.ById)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var cat = await _db.Categories
                           .Include(c => c.FeaturedImage)
                           .AsNoTracking()
                           .FirstOrDefaultAsync(c => c.CategoryId == id &&
                                                      c.TenantId == _tp.TenantId, ct);

        return cat is null ? NotFound() : Ok(ToDto(cat));
    }

    [HttpPost]
    [Route(Endpoints.Categories.Create)]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest req,
        CancellationToken ct)
    {

        if (req.ParentId is not null &&
            !await _db.Categories.AnyAsync(c => c.CategoryId == req.ParentId &&
                                                c.TenantId == _tp.TenantId, ct))
            return BadRequest($"Parent category {req.ParentId} does not exist.");


        string slug = Slugify(req.Name);
        if (await _db.Categories.AnyAsync(c => c.Slug == slug && c.TenantId == _tp.TenantId, ct))
            return Conflict($"Slug '{slug}' already in use.");


        Guid? featuredId = null;
        if (req.FeaturedImageId is not null)
        {
            bool ok = await _db.MediaFiles.AnyAsync(m => m.MediaFileId == req.FeaturedImageId &&
                                                         m.TenantId == _tp.TenantId, ct);
            if (!ok) return BadRequest("FeaturedImageId not found.");
            featuredId = req.FeaturedImageId;
        }

        var cat = new Category
        {
            CategoryId = Guid.NewGuid(),
            TenantId = _tp.TenantId!.Value,
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
            FeaturedImageId = featuredId,
            CreatedBy = CurrentUserId,
            UpdatedBy = CurrentUserId
        };

        _db.Categories.Add(cat);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById),
                               new { id = cat.CategoryId },
                               ToDto(cat));
    }

    [HttpPut]
    [Route(Endpoints.Categories.Update)]
    public async Task<IActionResult> Update(Guid id,
        [FromBody] UpdateCategoryRequest req,
        CancellationToken ct)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(
            c => c.CategoryId == id && c.TenantId == _tp.TenantId, ct);
        if (cat is null) return NotFound();


        if (req.ParentId == id)
            return BadRequest("Category cannot be its own parent.");
        if (req.ParentId is not null &&
            !await _db.Categories.AnyAsync(c => c.CategoryId == req.ParentId &&
                                                c.TenantId == _tp.TenantId, ct))
            return BadRequest("Parent category not found.");

        string newSlug = Slugify(req.Name);
        if (newSlug != cat.Slug &&
            await _db.Categories.AnyAsync(c => c.Slug == newSlug &&
                                               c.TenantId == _tp.TenantId &&
                                               c.CategoryId != id, ct))
            return Conflict($"Slug '{newSlug}' already in use.");

        if (req.FeaturedImageId != cat.FeaturedImageId)
        {
            if (req.FeaturedImageId is not null &&
                !await _db.MediaFiles.AnyAsync(m => m.MediaFileId == req.FeaturedImageId &&
                                                    m.TenantId == _tp.TenantId, ct))
                return BadRequest("FeaturedImageId not found.");
            cat.FeaturedImageId = req.FeaturedImageId;
        }

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
        cat.UpdatedAtUtc = DateTime.UtcNow;
        cat.UpdatedBy = CurrentUserId;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete]
    [Route(Endpoints.Categories.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var cat = await _db.Categories
                           .Include(c => c.Children)
                           .Include(c => c.FeaturedImage)
                           .FirstOrDefaultAsync(c => c.CategoryId == id &&
                                                      c.TenantId == _tp.TenantId, ct);
        if (cat is null) return NotFound();

        if (cat.Children.Any())
            return BadRequest("Category has child categories; remove or re-parent them first.");

        if (await _db.ProductCategories.AnyAsync(pc => pc.CategoryId == id, ct))
            return BadRequest("Category is assigned to products; unassign before deleting.");


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


    [HttpPost]
    [Route(Endpoints.Categories.FeaturedImageUpload)]
    public async Task<ActionResult<CategoryDto>> UploadFeaturedImage(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var cat = await _db.Categories.FirstOrDefaultAsync(
            c => c.CategoryId == id && c.TenantId == _tp.TenantId, ct);
        if (cat is null) return NotFound();
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

            TenantId = _tp.TenantId!.Value
        };
        _db.MediaFiles.Add(media);

        /* attach to category */
        cat.FeaturedImageId = mediaId;
        cat.UpdatedAtUtc = DateTime.UtcNow;
        cat.UpdatedBy = CurrentUserId;

        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(cat));
    }

    [HttpDelete]
    [Route(Endpoints.Categories.FeaturedImageRemove)]
    public async Task<IActionResult> RemoveFeaturedImage(Guid id, CancellationToken ct)
    {
        var cat = await _db.Categories
                           .Include(c => c.FeaturedImage)
                           .FirstOrDefaultAsync(c => c.CategoryId == id &&
                                                      c.TenantId == _tp.TenantId, ct);
        if (cat is null) return NotFound();
        if (cat.FeaturedImage is null) return NoContent();

        var img = cat.FeaturedImage;
        cat.FeaturedImageId = null;
        cat.UpdatedAtUtc = DateTime.UtcNow;
        cat.UpdatedBy = CurrentUserId;

        await _db.SaveChangesAsync(ct);


        if (!await _db.Categories.AnyAsync(c => c.FeaturedImageId == img.MediaFileId, ct))
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
