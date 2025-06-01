using EcommerceApi.Attributes;
using EcommerceApi.Entities;
using EcommerceApi.Enums;
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

namespace EcommerceApi.Controllers;

[ApiController]
public sealed class CartController(AppDbContext db, IUserProvider userProvider) : ControllerBase
{
    private readonly AppDbContext _db = db;

    /* ──────────────── mapping helpers ──────────────── */
    private static CartItemDto Map(CartItem ci) => new(
        ci.CartItemId,
        ci.ProductId,
        ci.Product.Name,
        ci.Product.SKU,
        ci.Qty,
        ci.UnitPriceSnap,
        ci.UnitPriceSnap * ci.Qty);

    private static CartDto Project(Cart cart)
    {
        var items = cart.Items.Select(Map).ToList();
        var subtotal = items.Sum(i => i.LineTotal);
        return new CartDto(cart.CartId, items, subtotal, items.Sum(i => i.Qty));
    }

    private async Task<Cart> GetOrCreateAsync(Guid customerId, CancellationToken ct)
    {
        var cart = await _db.Carts
                            .Include(c => c.Items)
                                .ThenInclude(i => i.Product)
                            .FirstOrDefaultAsync(c => c.CustomerId == customerId &&
                                                       c.Status == CartStatus.Active, ct);

        if (cart != null) return cart;

        cart = new Cart
        {
            CartId = Guid.NewGuid(),
            TenantId = userProvider.TenantId,
            CustomerId = customerId,
            CreatedBy = customerId
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    /* ──────────────── READ ──────────────── */

    [AppAuthorize(FeatureFactory.Cart.CanGetCart)]
    [HttpGet(Endpoints.Cart.GetItem)]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
    {
        var cart = await GetOrCreateAsync(userProvider.UserId, ct);
        await _db.Entry(cart).Collection(c => c.Items).LoadAsync(ct);
        await _db.Entry(cart).Collection(c => c.Items).Query().Include(i => i.Product).LoadAsync(ct);
        return Ok(Project(cart));
    }

    /* ──────────────── CREATE / ADD ITEM ──────────────── */

    [AppAuthorize(FeatureFactory.Cart.CanAddCart)]
    [HttpPost]
    [Route(Endpoints.Cart.AddItem)]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddItemRequest req, CancellationToken ct)
    {
        /* validation */
        if (req.Qty <= 0)
        {
            ModelState.AddModelError(nameof(req.Qty), "Qty must be greater than zero.");
            return this.ApplicationProblem(
                detail: "Quantity must be greater than zero.",
                title: "Validation Error",
                statusCode: StatusCodes.Status400BadRequest,
                modelState: ModelState,
                errorCode: ErrorCodes.ValidationFailed,
                instance: HttpContext.Request.Path);
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == req.ProductId, ct);
        if (product is null)
        {
            ModelState.AddModelError(nameof(req.ProductId), "Product not found.");
            return this.ApplicationProblem(
                detail: "Product not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        var cart = await GetOrCreateAsync(userProvider.UserId, ct);

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
                TenantId = userProvider.TenantId
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

    /* ──────────────── UPDATE / CHANGE QTY ──────────────── */

    [AppAuthorize(FeatureFactory.Cart.CanAddCart)]
    [HttpPut]
    [Route(Endpoints.Cart.UpdateItemQty)]
    public async Task<ActionResult<CartDto>> UpdateQty(Guid itemId, [FromBody] UpdateQtyRequest req, CancellationToken ct)
    {
        var item = await _db.CartItems
                            .Include(i => i.Cart)
                                .ThenInclude(c => c.Items)
                                    .ThenInclude(i => i.Product)
                            .FirstOrDefaultAsync(i => i.CartItemId == itemId, ct);

        if (item is null)
        {
            ModelState.AddModelError(nameof(itemId), "Cart item not found.");
            return this.ApplicationProblem(
                detail: "Cart item not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        if (item.Cart.CustomerId != userProvider.UserId)
        {
            ModelState.AddModelError("user", "You are not authorized to modify this cart item.");
            return this.ApplicationProblem(
                detail: "You are not authorized to modify this cart item.",
                title: "Forbidden",
                statusCode: StatusCodes.Status403Forbidden,
                modelState: ModelState,
                errorCode: ErrorCodes.Forbidden,
                instance: HttpContext.Request.Path);
        }

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

    /* ──────────────── DELETE / REMOVE ITEM ──────────────── */

    [AppAuthorize(FeatureFactory.Cart.CanRemoveCart)]
    [HttpDelete]
    [Route(Endpoints.Cart.RemoveItem)]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartItemId == itemId, ct);
        if (item is null)
        {
            ModelState.AddModelError(nameof(itemId), "Cart item not found.");
            return this.ApplicationProblem(
                detail: "Cart item not found.",
                title: "Resource Not Found",
                statusCode: StatusCodes.Status404NotFound,
                modelState: ModelState,
                errorCode: ErrorCodes.ResourceNotFound,
                instance: HttpContext.Request.Path);
        }

        var owner = await _db.Carts.AnyAsync(c => c.CartId == item.CartId && c.CustomerId == userProvider.UserId, ct);
        if (owner == false)
        {
            ModelState.AddModelError("user", "You are not authorized to remove this item.");
            return this.ApplicationProblem(
                detail: "You are not authorized to remove this item.",
                title: "Forbidden",
                statusCode: StatusCodes.Status403Forbidden,
                modelState: ModelState,
                errorCode: ErrorCodes.Forbidden,
                instance: HttpContext.Request.Path);
        }

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    /* ──────────────── DELETE / CLEAR CART ──────────────── */

    [AppAuthorize(FeatureFactory.Cart.CanRemoveCart)]
    [HttpDelete]
    [Route(Endpoints.Cart.Clear)]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var cart = await GetOrCreateAsync(userProvider.UserId, ct);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
