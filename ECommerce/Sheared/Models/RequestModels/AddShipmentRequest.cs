using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record AddShipmentRequest(string CarrierCode,
                                            string ServiceLevel,
                                            string? TrackingNumber,
                                            decimal ShippingCost);

}
