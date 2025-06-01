using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Models;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;

namespace EcommerceApi.Controllers;

[ApiController]
public sealed class WishlistController(AppDbContext db, IUserProvider userProvider)
    : ControllerBase
{
    private readonly AppDbContext _db = db;

    public sealed record WishlistItemDto(Guid ProductId, string Name, string SKU, decimal Price);

    [AppAuthorize(FeatureFactory.Wishlist.CanGetWishlist)]
    [HttpGet(Endpoints.Wishlist.GetList)]
    public async Task<ActionResult<IEnumerable<WishlistItemDto>>> Get(CancellationToken ct)
    {
        var list = await _db.WishlistItems
                            .Include(w => w.Product)
                            .Where(w => w.CustomerId == userProvider.UserId)
                            .AsNoTracking()
                            .ToListAsync(ct);

        return Ok(list.Select(w => new WishlistItemDto(
            w.ProductId, w.Product.Name, w.Product.SKU, w.Product.Price)));
    }

    [AppAuthorize(FeatureFactory.Wishlist.CanAddWishlist)]
    [HttpPost(Endpoints.Wishlist.AddItem)]
    public async Task<IActionResult> Add(Guid productId, CancellationToken ct)
    {
        bool exists = await _db.WishlistItems
                               .AnyAsync(w => w.CustomerId == userProvider.UserId &&
                                              w.ProductId == productId, ct);
        if (exists) return NoContent();

        var productExists = await _db.Products
                                     .AnyAsync(p => p.ProductId == productId, ct);
        if (!productExists) return NotFound("Product");

        _db.WishlistItems.Add(new WishlistItem
        {
            WishlistItemId = Guid.NewGuid(),
            CustomerId = userProvider.UserId,
            ProductId = productId,
            TenantId = userProvider.TenantId
        });
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [AppAuthorize(FeatureFactory.Wishlist.CanRemoveWishlist)]
    [HttpDelete(Endpoints.Wishlist.RemoveItem)]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
    {
        var row = await _db.WishlistItems.FirstOrDefaultAsync(
            w => w.CustomerId == userProvider.UserId &&
                 w.ProductId == productId, ct);

        if (row is null) return NoContent();

        _db.WishlistItems.Remove(row);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
