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
[Route(Endpoints.Discounts.Base)]
public sealed class DiscountController(AppDbContext db, IUserProvider userProvider)
    : ControllerBase
{
    private readonly AppDbContext _db = db;
    private Guid CurrentUser => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static DiscountDto Map(Discount d) => new(
        d.DiscountId, d.Code, d.Type, d.Value, d.IsActive,
        d.UsesSoFar, d.MaxUses, d.StartsAtUtc, d.ExpiresAtUtc);


    [HttpGet]
    public async Task<ActionResult<IEnumerable<DiscountDto>>> All(CancellationToken ct)
        => Ok((await _db.Discounts.AsNoTracking()
                                  .ToListAsync(ct)).Select(Map));


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DiscountDto>> Get(Guid id, CancellationToken ct)
    {
        var d = await _db.Discounts.FirstOrDefaultAsync(
            x => x.DiscountId == id, ct);
        return d is null ? NotFound() : Ok(Map(d));
    }

    [HttpPost]
    public async Task<ActionResult<DiscountDto>> Create(
        [FromBody] UpsertDiscountRequest req, CancellationToken ct)
    {
        bool dupe = await _db.Discounts.AnyAsync(
            x => x.Code == req.Code, ct);
        if (dupe) return Conflict("Duplicate code.");

        var d = new Discount
        {
            DiscountId = Guid.NewGuid(),
            TenantId = userProvider.TenantId,
            Code = req.Code.ToUpperInvariant(),
            Type = req.Type,
            Value = req.Value,
            IsActive = req.IsActive,
            MaxUses = req.MaxUses,
            MinOrderTotal = req.MinOrderTotal,
            StartsAtUtc = req.StartsAtUtc,
            ExpiresAtUtc = req.ExpiresAtUtc,
            Description = req.Description,
            CreatedBy = userProvider.UserId
        };
        _db.Discounts.Add(d);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = d.DiscountId }, Map(d));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpsertDiscountRequest req, CancellationToken ct)
    {
        var d = await _db.Discounts.FirstOrDefaultAsync(
            x => x.DiscountId == id, ct);
        if (d is null) return NotFound();

        d.Code = req.Code.ToUpperInvariant();
        d.Type = req.Type;
        d.Value = req.Value;
        d.IsActive = req.IsActive;
        d.MaxUses = req.MaxUses;
        d.MinOrderTotal = req.MinOrderTotal;
        d.StartsAtUtc = req.StartsAtUtc;
        d.ExpiresAtUtc = req.ExpiresAtUtc;
        d.Description = req.Description;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var d = await _db.Discounts.FirstOrDefaultAsync(
            x => x.DiscountId == id, ct);
        if (d is null) return NotFound();

        _db.Discounts.Remove(d);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost]
    [Route(Endpoints.Discounts.ApplyToOrder)]
    public async Task<IActionResult> ApplyToOrder(
        Guid orderId, [FromBody] string code, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.OrderDiscounts)
                                    .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
        if (order is null) return NotFound("Order");

        var d = await _db.Discounts.FirstOrDefaultAsync(
            x => x.Code == code.ToUpperInvariant() &&                
                 x.IsActive &&
                 (x.StartsAtUtc == null || x.StartsAtUtc <= DateTime.UtcNow) &&
                 (x.ExpiresAtUtc == null || x.ExpiresAtUtc >= DateTime.UtcNow) &&
                 (x.MaxUses == null || x.UsesSoFar < x.MaxUses), ct);
        if (d is null) return BadRequest("Invalid or expired code.");

        if (order.GrandTotal < (d.MinOrderTotal ?? 0))
            return BadRequest("Order total below minimum for this discount.");

        bool already = order.OrderDiscounts.Any(od => od.DiscountId == d.DiscountId);
        if (already) return Conflict("Discount already applied.");

        decimal amt = d.Type switch
        {
            DiscountType.Percent => Math.Round(order.Subtotal * (d.Value / 100m), 2),
            DiscountType.FixedAmount => d.Value,
            _ => 0
        };

        order.DiscountTotal += amt;
        d.UsesSoFar += 1;

        _db.OrderDiscounts.Add(new OrderDiscount
        {
            OrderId = orderId,
            DiscountId = d.DiscountId,
            AmountApplied = amt
        });

        await _db.SaveChangesAsync(ct);
        return Ok(new { AmountApplied = amt, NewDiscountTotal = order.DiscountTotal });
    }

    [HttpDelete]
    [Route(Endpoints.Discounts.RemoveFromOrder)]
    public async Task<IActionResult> RemoveFromOrder(
        Guid orderId, string code, CancellationToken ct)
    {
        var od = await _db.OrderDiscounts
                          .Include(x => x.Discount)
                          .FirstOrDefaultAsync(od => od.OrderId == orderId &&
                                                     od.Discount.Code == code.ToUpperInvariant(), ct);
        if (od is null) return NotFound();

        var order = await _db.Orders.FirstAsync(o => o.OrderId == orderId, ct);
        order.DiscountTotal -= od.AmountApplied;
        od.Discount.UsesSoFar = Math.Max(0, od.Discount.UsesSoFar - 1);

        _db.OrderDiscounts.Remove(od);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
