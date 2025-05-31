using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record OrderItemDto(Guid OrderItemId, Guid? ProductId, string Name, string SKU,
                                      int Qty, decimal UnitPrice, decimal LineTotal);
}
