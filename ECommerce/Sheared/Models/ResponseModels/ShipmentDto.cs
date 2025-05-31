using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record ShipmentDto(Guid ShipmentId, string Carrier, string Service, string? Tracking,
                                     ShipmentStatus Status,
                                     DateTime? ShippedAtUtc, DateTime? DeliveredAtUtc,
                                     decimal ShippingCost);
}
