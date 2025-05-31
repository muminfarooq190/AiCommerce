using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record DiscountDto(Guid DiscountId, string Code,
       DiscountType Type, decimal Value,
       bool IsActive, int UsesSoFar, int? MaxUses,
       DateTime? Starts, DateTime? Expires);
}
