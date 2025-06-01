using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Extensions;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;

namespace EcommerceApi.Controllers;

[ApiController]
public sealed class CollectionController(
        AppDbContext db,
        IUserProvider userProvider) : ControllerBase
{
    private readonly AppDbContext _db = db;

    /* ──────────────── helpers ──────────────── */
    private static string Slugify(string s) =>
        System.Text.RegularExpressions.Regex
            .Replace(s.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-")
            .Trim('-');

    private CollectionDto Map(Collection c) => new(
        c.CollectionId,
        c.Name,
        c.Slug,
        c.BadgeLabel,
        c.IsFeatured,
        c.HeroImageId,
        c.Products.OrderBy(cp => cp.SortOrder).Select(cp => cp.ProductId));

    /* ──────────────── READ ──────────────── */

    [AppAuthorize(FeatureFactory.Collection.CanGetCollection)]
    [HttpGet(Endpoints.Collections.GetAllCollection)]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetAll(CancellationToken ct)
    {
        var list = await _db.Collections
                            .Include(c => c.Products)
                            .AsNoTracking()
                            .ToListAsync(ct);

        return Ok(list.Select(Map));
    }

    [AppAuthorize(FeatureFactory.Collection.CanGetCollection)]
    [HttpGet(Endpoints.Collections.GetCollectionById)]
    public async Task<ActionResult<CollectionDto>> Get(Guid id, CancellationToken ct)
    {
        var c = await _db.Collections
                         .Include(x => x.Products)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(x => x.CollectionId == id, ct);

        if (c is null)
        {
            ModelState.AddModelError(nameof(id), "Collection not found.");
            return this.ApplicationProblem(
                detail: $"Collection '{id}' not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        return Ok(Map(c));
    }

    /* ──────────────── CREATE ──────────────── */

    [AppAuthorize(FeatureFactory.Collection.CanAddCollection)]
    [HttpPost(Endpoints.Collections.AddCollection)]
    public async Task<ActionResult<CollectionDto>> Create([FromBody] CreateCollectionRequest req, CancellationToken ct)
    {
        var slug = Slugify(req.Name);
        bool dupe = await _db.Collections.AnyAsync(c => c.Slug == slug, ct);
        if (dupe) slug += "-" + Guid.NewGuid().ToString("N")[..5];

        var col = new Collection
        {
            CollectionId = Guid.NewGuid(),
            TenantId = userProvider.TenantId,
            Name = req.Name,
            Slug = slug,
            BadgeLabel = req.BadgeLabel,
            IsFeatured = req.IsFeatured,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Collections.Add(col);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = col.CollectionId }, Map(col));
    }

    /* ──────────────── UPDATE ──────────────── */

    [AppAuthorize(FeatureFactory.Collection.CanAddCollection)]
    [HttpPut(Endpoints.Collections.UpdateCollection)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCollectionRequest req, CancellationToken ct)
    {
        var c = await _db.Collections.FirstOrDefaultAsync(x => x.CollectionId == id, ct);

        if (c is null)
        {
            ModelState.AddModelError(nameof(id), "Collection not found.");
            return this.ApplicationProblem(
                detail: $"Collection '{id}' not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        c.Name = req.Name;
        c.BadgeLabel = req.BadgeLabel;
        c.IsFeatured = req.IsFeatured;
        c.Slug = Slugify(req.Name);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /* ──────────────── DELETE ──────────────── */

    [AppAuthorize(FeatureFactory.Collection.CanRemoveCollection)]
    [HttpDelete(Endpoints.Collections.DeleteCollection)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var c = await _db.Collections.FirstOrDefaultAsync(x => x.CollectionId == id, ct);

        if (c is null)
        {
            ModelState.AddModelError(nameof(id), "Collection not found.");
            return this.ApplicationProblem(
                detail: $"Collection '{id}' not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        _db.Collections.Remove(c);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /* ──────────────── LIST PRODUCT IDS ──────────────── */

    [AppAuthorize(FeatureFactory.Collection.CanGetCollection)]
    [HttpGet]
    [Route(Endpoints.Collections.GetCollectionIds)]
    public async Task<ActionResult<IEnumerable<Guid>>> ListProducts(Guid id, CancellationToken ct)
    {
        var exists = await _db.Collections.AnyAsync(c => c.CollectionId == id, ct);
        if (!exists)
        {
            ModelState.AddModelError(nameof(id), "Collection not found.");
            return this.ApplicationProblem(
                detail: $"Collection '{id}' not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        var ids = await _db.CollectionProducts
                           .Where(cp => cp.CollectionId == id)
                           .OrderBy(cp => cp.SortOrder)
                           .Select(cp => cp.ProductId)
                           .ToListAsync(ct);
        return Ok(ids);
    }

    /* ──────────────── ADD PRODUCTS ──────────────── */

    [AppAuthorize(FeatureFactory.Collection.CanAddCollection)]
    [HttpPost]
    [Route(Endpoints.Collections.AddProduct)]
    public async Task<IActionResult> AddProducts(Guid id, [FromBody] AddProductsRequest req, CancellationToken ct)
    {
        if (!await _db.Collections.AnyAsync(c => c.CollectionId == id, ct))
        {
            ModelState.AddModelError(nameof(id), "Collection not found.");
            return this.ApplicationProblem(
                detail: $"Collection '{id}' not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        var productIds = req.ProductIds.Distinct().ToList();
        if (!productIds.Any())
        {
            ModelState.AddModelError(nameof(req.ProductIds), "No products supplied.");
            return this.ApplicationProblem(
                detail: "No products supplied.",
                title: "Validation Error",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.ValidationFailed,
                instance: HttpContext.Request.Path);
        }

        var validProducts = await _db.Products
                                     .Where(p => productIds.Contains(p.ProductId))
                                     .Select(p => p.ProductId)
                                     .ToListAsync(ct);
        var invalid = productIds.Except(validProducts).ToList();
        if (invalid.Any())
        {
            ModelState.AddModelError(nameof(req.ProductIds), $"Unknown products: {string.Join(',', invalid)}");
            return this.ApplicationProblem(
                detail: $"Unknown products: {string.Join(',', invalid)}",
                title: "Validation Error",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.ValidationFailed,
                instance: HttpContext.Request.Path);
        }

        int maxSort = await _db.CollectionProducts
                               .Where(cp => cp.CollectionId == id)
                               .Select(cp => (int?)cp.SortOrder)
                               .MaxAsync(ct) ?? -1;

        foreach (var pid in validProducts)
        {
            bool exists = await _db.CollectionProducts.AnyAsync(
                cp => cp.CollectionId == id && cp.ProductId == pid, ct);
            if (!exists)
            {
                _db.CollectionProducts.Add(new CollectionProduct
                {
                    CollectionId = id,
                    ProductId = pid,
                    SortOrder = ++maxSort,
                    TenantId = userProvider.TenantId
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /* ──────────────── REMOVE PRODUCT ──────────────── */

    [AppAuthorize(FeatureFactory.Collection.CanRemoveCollection)]
    [HttpDelete]
    [Route(Endpoints.Collections.RemoveProduct)]
    public async Task<IActionResult> RemoveProduct(Guid id, Guid prodId, CancellationToken ct)
    {
        var link = await _db.CollectionProducts.FirstOrDefaultAsync(
            cp => cp.CollectionId == id && cp.ProductId == prodId, ct);

        if (link is null)
        {
            ModelState.AddModelError("link", "Product is not part of this collection.");
            return this.ApplicationProblem(
                detail: "Product is not part of this collection.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        _db.CollectionProducts.Remove(link);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}