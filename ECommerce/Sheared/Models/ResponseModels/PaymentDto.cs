using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record PaymentDto(Guid PaymentId, decimal Amount, string Currency,
                                     PaymentMethod Method, PaymentStatus Status,
                                     DateTime? PaidAtUtc);

}
