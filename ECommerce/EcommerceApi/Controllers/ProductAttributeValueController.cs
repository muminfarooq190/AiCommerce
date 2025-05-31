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
[Route("api/productattributevalue")]
public sealed class ProductAttributeValueController(
        AppDbContext db,
        IUserProvider userProvider) : ControllerBase
{

    private readonly AppDbContext _db = db;


    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttributeValueDto>>> List(
        Guid productId, CancellationToken ct)
    {
        var rows = await _db.ProductAttributeValues
                            .Include(v => v.Attribute)
                            .Where(v => v.ProductId == productId)
                            .AsNoTracking()
                            .ToListAsync(ct);

        return Ok(rows.Select(v => new AttributeValueDto(
            v.AttributeId, v.Attribute.Name, v.Attribute.UnitLabel,
            v.Attribute.DataType, v.ValueString, v.ValueNumber, v.ValueBool)));
    }

    [HttpPut]
    [Route(Endpoints.ProductAttributes.ByAttr)]
    public async Task<IActionResult> Upsert(
        Guid productId, Guid attributeId,
        [FromBody] UpsertValueRequest req, CancellationToken ct)
    {
        bool foundProduct = await _db.Products
                                     .AnyAsync(p => p.ProductId == productId, ct);
        if (!foundProduct) return NotFound("Product");

        var attr = await _db.ProductAttributes
                            .FirstOrDefaultAsync(a => a.AttributeId == attributeId , ct);
        if (attr is null) return NotFound("Attribute");

        if (attr.DataType != req.DataType)
            return BadRequest("DataType mismatch with attribute definition.");

        bool valid = req.DataType switch
        {
            AttributeDataType.String => !string.IsNullOrWhiteSpace(req.ValueString),
            AttributeDataType.Number => req.ValueNumber is not null,
            AttributeDataType.Boolean => req.ValueBool is not null,
            _ => false
        };
        if (!valid) return BadRequest("Provide a value that matches DataType.");

        var row = await _db.ProductAttributeValues
                           .FirstOrDefaultAsync(v => v.ProductId == productId &&
                                                     v.AttributeId == attributeId, ct);

        if (row is null)
        {
            row = new ProductAttributeValue
            {
                ProductId = productId,
                AttributeId = attributeId,
                TenantId = userProvider.TenantId
            };
            _db.ProductAttributeValues.Add(row);
        }

        row.ValueString = req.DataType == AttributeDataType.String ? req.ValueString : null;
        row.ValueNumber = req.DataType == AttributeDataType.Number ? req.ValueNumber : null;
        row.ValueBool = req.DataType == AttributeDataType.Boolean ? req.ValueBool : null;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
