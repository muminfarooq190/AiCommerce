using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record OrderDto(
       Guid OrderId,
       string OrderNumber,
       OrderStatus Status,
       string Currency,
       decimal Subtotal,
       decimal Discount,
       decimal Shipping,
       decimal Tax,
       decimal GrandTotal,
       DateTime PlacedAtUtc,
       Guid CustomerId,
       IEnumerable<OrderItemDto> Items,
       IEnumerable<PaymentDto> Payments,
       IEnumerable<ShipmentDto> Shipments);
}
