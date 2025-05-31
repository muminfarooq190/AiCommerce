using EcommerceApi.Entities;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Enums;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Security.Claims;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/cart")]
public sealed class CartController(AppDbContext db, ITenantProvider tp)
    : ControllerBase
{

    private readonly AppDbContext _db = db;
    private readonly Guid _tenantId = tp.TenantId!.Value;
    private Guid CurrentUser => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    private static CartItemDto Map(CartItem ci) => new(ci.CartItemId, ci.ProductId,
        ci.Product.Name, ci.Product.SKU, ci.Qty, ci.UnitPriceSnap, ci.UnitPriceSnap * ci.Qty);

    private static CartDto Project(Cart cart)
    {
        var items = cart.Items.Select(Map).ToList();
        var subtotal = items.Sum(i => i.LineTotal);
        return new CartDto(cart.CartId, items, subtotal, items.Sum(i => i.Qty));
    }


    private async Task<Cart> GetOrCreateAsync(Guid customerId, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product)
                                  .FirstOrDefaultAsync(c => c.CustomerId == customerId &&
                                                            c.TenantId == _tenantId &&
                                                            c.Status == CartStatus.Active, ct);

        if (cart != null) return cart;

        cart = new Cart
        {
            CartId = Guid.NewGuid(),
            TenantId = _tenantId,
            CustomerId = customerId,
            CreatedBy = customerId
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync(ct);
        return cart;
    }


    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
    {
        var cart = await GetOrCreateAsync(CurrentUser, ct);
        await _db.Entry(cart).Collection(c => c.Items).LoadAsync(ct);
        await _db.Entry(cart).Collection(c => c.Items)
                            .Query().Include(i => i.Product).LoadAsync(ct);

        return Ok(Project(cart));
    }


    [HttpPost]
    [Route(Endpoints.Cart.Items)]
    public async Task<ActionResult<CartDto>> AddItem(
        [FromBody] AddItemRequest req, CancellationToken ct)
    {
        if (req.Qty <= 0) return BadRequest("Qty must be > 0");

        var product = await _db.Products
                               .FirstOrDefaultAsync(p => p.ProductId == req.ProductId &&
                                                         p.TenantId == _tenantId, ct);
        if (product is null) return NotFound("Product");

        var cart = await GetOrCreateAsync(CurrentUser, ct);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == req.ProductId);
        if (item is null)
        {
            item = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cart.CartId,
                ProductId = product.ProductId,
                Qty = req.Qty,
                UnitPriceSnap = product.Price,
                TenantId = _tenantId
            };
            _db.CartItems.Add(item);
        }
        else
        {
            item.Qty += req.Qty;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(Project(cart));
    }


    [HttpPut]
    [Route(Endpoints.Cart.Item)]
    public async Task<ActionResult<CartDto>> UpdateQty(
        Guid itemId, [FromBody] UpdateQtyRequest req, CancellationToken ct)
    {
        var item = await _db.CartItems.Include(i => i.Cart)
                                      .ThenInclude(c => c.Items)
                                      .ThenInclude(i => i.Product)
                                      .FirstOrDefaultAsync(i => i.CartItemId == itemId &&
                                                                i.TenantId == _tenantId, ct);
        if (item is null) return NotFound();

        if (item.Cart.CustomerId != CurrentUser) return Forbid();

        if (req.Qty <= 0)
        {
            _db.CartItems.Remove(item);
        }
        else
        {
            item.Qty = req.Qty;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(Project(item.Cart));
    }


    [HttpDelete]
    [Route(Endpoints.Cart.Item)]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartItemId == itemId &&
                                                                i.TenantId == _tenantId, ct);
        if (item is null) return NotFound();
        if (await _db.Carts.AnyAsync(c => c.CartId == item.CartId &&
                                          c.CustomerId == CurrentUser, ct) == false)
            return Forbid();

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete]
    [Route(Endpoints.Cart.Clear)]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var cart = await GetOrCreateAsync(CurrentUser, ct);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
