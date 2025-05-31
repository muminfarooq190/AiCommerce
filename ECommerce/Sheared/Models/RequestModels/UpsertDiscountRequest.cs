using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record UpsertDiscountRequest(
        string Code, DiscountType Type, decimal Value,
        bool IsActive, int? MaxUses, decimal? MinOrderTotal,
        DateTime? StartsAtUtc, DateTime? ExpiresAtUtc,
        string? Description);
}
