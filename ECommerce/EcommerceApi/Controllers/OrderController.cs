using EcommerceApi.Entities;
using EcommerceApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheared;
using Sheared.Enums;
using Sheared.Models;
using Sheared.Models.RequestModels;
using Sheared.Models.ResponseModels;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrderController(
        AppDbContext db,
        IUserProvider userProvider) : ControllerBase
{
    private readonly AppDbContext _db = db;

    private static string Slugify(string txt)
        => Regex.Replace(txt.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');

    private Guid CurrentUser =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   throw new InvalidOperationException("NameIdentifier claim missing"));

   
    private static OrderDto ToDto(Order o) => new(
        o.OrderId,
        o.OrderNumber,
        o.Status,
        o.Currency,
        o.Subtotal,
        o.DiscountTotal,
        o.ShippingTotal,
        o.TaxTotal,
        o.GrandTotal,
        o.PlacedAtUtc,
        o.CustomerId,
        o.Items.Select(i => new OrderItemDto(
            i.OrderItemId, i.ProductId, i.ProductName, i.Sku,
            i.Qty, i.UnitPrice, i.LineTotal)),
        o.Payments.Select(p => new PaymentDto(
            p.PaymentId, p.Amount, p.Currency, p.Method, p.Status, p.PaidAtUtc)),
        o.Shipments.Select(s => new ShipmentDto(
            s.ShipmentId, s.CarrierCode, s.ServiceLevel, s.TrackingNumber,
            s.Status, s.ShippedAtUtc, s.DeliveredAtUtc, s.ShippingCost)));


    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> List(CancellationToken ct)
    {
        var orders = await _db.Orders
                              .Include(o => o.Items)
                              .Include(o => o.Payments)
                              .Include(o => o.Shipments)
                              .AsNoTracking()
                              .OrderByDescending(o => o.PlacedAtUtc)
                              .ToListAsync(ct);

        return Ok(orders.Select(ToDto));
    }

    [HttpGet]
    [Route(Endpoints.Orders.ById)]
    public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken ct)
    {
        var o = await _db.Orders
                         .Include(o => o.Items)
                         .Include(o => o.Payments)
                         .Include(o => o.Shipments)
                         .FirstOrDefaultAsync(x => x.OrderId == id, ct);

        return o is null ? NotFound() : Ok(ToDto(o));
    }

    [HttpPost]
    [Route(Endpoints.Orders.Create)]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        if (!req.Items.Any())
            return BadRequest("Order must contain at least one item.");

        var prodIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
        var prods = await _db.Products
                               .Where(p => prodIds.Contains(p.ProductId))
                               .ToListAsync(ct);

        var missing = prodIds.Except(prods.Select(p => p.ProductId)).ToList();
        if (missing.Any())
            return BadRequest($"Products not found: {string.Join(',', missing)}");

        var orderId = Guid.NewGuid();
        var number = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N[..5]}";

        var items = req.Items.Select(line =>
        {
            var p = prods.First(x => x.ProductId == line.ProductId);
            return new OrderItem
            {
                OrderItemId = Guid.NewGuid(),
                OrderId = orderId,
                ProductId = p.ProductId,
                Sku = p.SKU,
                ProductName = p.Name,
                Qty = line.Qty,
                UnitPrice = p.Price,
                Discount = 0,
                Tax = 0,
                WeightKg = p.WeightKg,
                SnapshotJson = null,
                TenantId = userProvider.TenantId
            };
        }).ToList();

        var subtotal = items.Sum(i => i.UnitPrice * i.Qty);
        var order = new Order
        {
            OrderId = orderId,
            OrderNumber = number,
            TenantId = userProvider.TenantId,
            CustomerId = req.CustomerId,
            Status = OrderStatus.Pending,
            PlacedAtUtc = DateTime.UtcNow,

            Subtotal = subtotal,
            DiscountTotal = req.DiscountTotal,
            ShippingTotal = req.ShippingCost,
            TaxTotal = req.TaxTotal,
            Currency = req.Currency,

            ShippingAddress = req.Shipping,
            BillingAddress = req.Billing,

            Items = items,
            CreatedBy = CurrentUser
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = order.OrderId }, ToDto(order));
    }


    [HttpPost]
    [Route(Endpoints.Orders.ById + "/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeStatusRequest req,
        CancellationToken ct)
    {
        var o = await _db.Orders.FirstOrDefaultAsync(
            x => x.OrderId == id, ct);
        if (o is null) return NotFound();

        if (o.Status == req.NewStatus) return NoContent();

        _db.OrderStatusHistory.Add(new OrderStatusHistory
        {
            OrderId = id,
            FromStatus = o.Status,
            ToStatus = req.NewStatus,
            ChangedBy = CurrentUser
        });

        o.Status = req.NewStatus;
        o.UpdatedAtUtc = DateTime.UtcNow;
        o.UpdatedBy = CurrentUser;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }


    [HttpPost]
    [Route(Endpoints.Orders.ById + "/payments")]
    public async Task<ActionResult<OrderDto>> AddPayment(
        Guid id,
        [FromBody] AddPaymentRequest req,
        CancellationToken ct)
    {
        var o = await _db.Orders.Include(x => x.Payments)
                                .FirstOrDefaultAsync(x => x.OrderId == id, ct);
        if (o is null) return NotFound();

        var pay = new Payment
        {
            PaymentId = Guid.NewGuid(),
            OrderId = id,
            Amount = req.Amount,
            Currency = o.Currency,
            Method = req.Method,
            Status = PaymentStatus.Paid,
            PaidAtUtc = DateTime.UtcNow,
            TenantId = userProvider.TenantId
        };
        _db.Payments.Add(pay);


        var paidTotal = o.Payments.Where(p => p.Status == PaymentStatus.Paid)
                                  .Sum(p => p.Amount) + req.Amount;
        if (paidTotal >= o.GrandTotal && o.Status == OrderStatus.Pending)
            o.Status = OrderStatus.Processing;

        await _db.SaveChangesAsync(ct);

        return Ok(ToDto(await _db.Orders
                                  .Include(x => x.Items)
                                  .Include(x => x.Payments)
                                  .Include(x => x.Shipments)
                                  .FirstAsync(x => x.OrderId == id, ct)));
    }


    [HttpPost]
    [Route(Endpoints.Orders.ById + "/shipments")]
    public async Task<ActionResult<OrderDto>> AddShipment(
        Guid id,
        [FromBody] AddShipmentRequest req,
        CancellationToken ct)
    {
        var o = await _db.Orders.Include(x => x.Shipments)
                                .FirstOrDefaultAsync(x => x.OrderId == id , ct);
        if (o is null) return NotFound();

        var sh = new Shipment
        {
            ShipmentId = Guid.NewGuid(),
            OrderId = id,
            CarrierCode = req.CarrierCode,
            ServiceLevel = req.ServiceLevel,
            TrackingNumber = req.TrackingNumber,
            Status = ShipmentStatus.Shipped,
            ShippedAtUtc = DateTime.UtcNow,
            ShippingCost = req.ShippingCost,
            TenantId = userProvider.TenantId
        };
        _db.Shipments.Add(sh);

        if (o.Status == OrderStatus.Processing)
            o.Status = OrderStatus.Shipped;

        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(await _db.Orders
                                 .Include(x => x.Items)
                                 .Include(x => x.Payments)
                                 .Include(x => x.Shipments)
                                 .FirstAsync(x => x.OrderId == id, ct)));
    }

    [HttpPut]
    [Route(Endpoints.Orders.ById + "/shipments/{shipId:guid}")]
    public async Task<IActionResult> UpdateShipment(
        Guid id,
        Guid shipId,
        [FromBody] ShipmentStatus newStatus,
        CancellationToken ct)
    {
        var sh = await _db.Shipments
                          .Include(s => s.Order)
                          .FirstOrDefaultAsync(s => s.ShipmentId == shipId &&
                                                    s.OrderId == id, ct);
        if (sh is null) return NotFound();

        sh.Status = newStatus;
        if (newStatus == ShipmentStatus.Delivered)
            sh.DeliveredAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
