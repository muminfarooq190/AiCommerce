using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    //cart request
    public sealed record AddItemRequest(Guid ProductId, int Qty = 1);
}
