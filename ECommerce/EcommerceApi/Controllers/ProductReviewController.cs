using EcommerceApi.Entities;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Enums;
using System.Security.Claims;

namespace EcommerceApi.Controllers;

[ApiController]
[Route(Endpoints.Reviews.ByProduct)]
public sealed class ProductReviewController(AppDbContext db, ITenantProvider tp)
    : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly Guid _tenantId = tp.TenantId!.Value;
    private Guid CurrentUser => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public sealed record ReviewDto(Guid ReviewId, Guid CustomerId, byte Rating,
                                   string? Title, string? Body,
                                   DateTime CreatedAtUtc, bool Verified);

    public sealed record CreateReviewRequest(byte Rating,
                                             string? Title,
                                             string? Body);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> List(Guid productId, CancellationToken ct)
    {
        var list = await _db.ProductReviews
                            .Where(r => r.ProductId == productId &&
                                        r.TenantId == _tenantId &&
                                        r.IsApproved)
                            .AsNoTracking()
                            .OrderByDescending(r => r.CreatedAtUtc)
                            .ToListAsync(ct);

        return Ok(list.Select(Map));
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(
        Guid productId,
        [FromBody] CreateReviewRequest req,
        CancellationToken ct)
    {
        if (req.Rating is < 1 or > 5) return BadRequest("Rating must be 1-5.");

        bool productExists = await _db.Products.AnyAsync(
            p => p.ProductId == productId && p.TenantId == _tenantId, ct);
        if (!productExists) return NotFound("Product");

        var rv = new ProductReview
        {
            ReviewId = Guid.NewGuid(),
            ProductId = productId,
            CustomerId = CurrentUser,
            Rating = req.Rating,
            Title = req.Title,
            Body = req.Body,
            VerifiedPurchase = await _db.Orders
                .AnyAsync(o => o.CustomerId == CurrentUser &&
                               o.Items.Any(i => i.ProductId == productId) &&
                               o.Status == OrderStatus.Delivered, ct),
            TenantId = _tenantId
        };

        _db.ProductReviews.Add(rv);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(List), new { productId }, Map(rv));
    }

    private static ReviewDto Map(ProductReview r) => new(
        r.ReviewId, r.CustomerId, r.Rating, r.Title, r.Body,
        r.CreatedAtUtc, r.VerifiedPurchase);
}
