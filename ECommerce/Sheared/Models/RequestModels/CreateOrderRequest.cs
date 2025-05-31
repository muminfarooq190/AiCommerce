using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record CreateOrderRequest(
    Guid CustomerId,
      Address Shipping,
      Address Billing,
      IEnumerable<CreateLineItem> Items,
      decimal ShippingCost,
      decimal DiscountTotal,
      decimal TaxTotal,
      string Currency);
}
