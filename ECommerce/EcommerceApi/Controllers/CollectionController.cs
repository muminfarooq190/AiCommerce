using EcommerceApi.Entities;
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

    /* helpers */
    private static string Slugify(string s) =>
        System.Text.RegularExpressions.Regex
            .Replace(s.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-")
            .Trim('-');

    private CollectionDto Map(Collection c) => new(
        c.CollectionId, c.Name, c.Slug, c.BadgeLabel,
        c.IsFeatured, c.HeroImageId,
        c.Products.OrderBy(cp => cp.SortOrder).Select(cp => cp.ProductId));

    [HttpGet(Endpoints.Collections.GetAllCollection)]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetAll(CancellationToken ct)
    {
        var list = await _db.Collections
                            .Include(c => c.Products)
                            .AsNoTracking()
                            .ToListAsync(ct);

        return Ok(list.Select(Map));
    }


    [HttpGet(Endpoints.Collections.GetCollectionById)]
    public async Task<ActionResult<CollectionDto>> Get(Guid id, CancellationToken ct)
    {
        var c = await _db.Collections.Include(x => x.Products)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.CollectionId == id, ct);
        return c is null ? NotFound() : Ok(Map(c));
    }

    [HttpPost(Endpoints.Collections.AddCollection)]
    public async Task<ActionResult<CollectionDto>> Create(
        [FromBody] CreateCollectionRequest req, CancellationToken ct)
    {
        var slug = Slugify(req.Name);
        bool dupe = await _db.Collections
                             .AnyAsync(c => c.Slug == slug, ct);
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


    [HttpPut(Endpoints.Collections.UpdateCollection)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] CreateCollectionRequest req, CancellationToken ct)
    {
        var c = await _db.Collections.FirstOrDefaultAsync(
            x => x.CollectionId == id, ct);
        if (c is null) return NotFound();

        c.Name = req.Name;
        c.BadgeLabel = req.BadgeLabel;
        c.IsFeatured = req.IsFeatured;
        c.Slug = Slugify(req.Name);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }


    [HttpDelete(Endpoints.Collections.DeleteCollection)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var c = await _db.Collections.FirstOrDefaultAsync(
            x => x.CollectionId == id, ct);
        if (c is null) return NotFound();

        _db.Collections.Remove(c);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }


    [HttpGet]
    [Route(Endpoints.Collections.GetCollectionIds)]
    public async Task<ActionResult<IEnumerable<Guid>>> ListProducts(
        Guid id, CancellationToken ct)
    {
        var ids = await _db.CollectionProducts
                           .Where(cp => cp.CollectionId == id)
                           .OrderBy(cp => cp.SortOrder)
                           .Select(cp => cp.ProductId)
                           .ToListAsync(ct);
        return Ok(ids);
    }


    [HttpPost]
    [Route(Endpoints.Collections.AddProduct)]
    public async Task<IActionResult> AddProducts(
        Guid id, [FromBody] AddProductsRequest req, CancellationToken ct)
    {
        var colExists = await _db.Collections
                                     .AnyAsync(c => c.CollectionId == id, ct);
        if (!colExists) return NotFound("Collection");

        var productIds = req.ProductIds.Distinct().ToList();
        if (!productIds.Any()) return BadRequest("No products supplied.");

        var validProducts = await _db.Products.Where(p => productIds.Contains(p.ProductId))
                                              .Select(p => p.ProductId)
                                              .ToListAsync(ct);
        var invalid = productIds.Except(validProducts).ToList();
        if (invalid.Any()) return BadRequest($"Unknown products: {string.Join(',', invalid)}");

        int maxSort = await _db.CollectionProducts
                               .Where(cp => cp.CollectionId == id)
                               .Select(cp => (int?)cp.SortOrder).MaxAsync(ct) ?? -1;

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

    [HttpDelete]
    [Route(Endpoints.Collections.RemoveProduct)]
    public async Task<IActionResult> RemoveProduct(
        Guid id, Guid prodId, CancellationToken ct)
    {
        var link = await _db.CollectionProducts.FirstOrDefaultAsync(
            cp => cp.CollectionId == id && cp.ProductId == prodId, ct);
        if (link is null) return NotFound();

        _db.CollectionProducts.Remove(link);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
