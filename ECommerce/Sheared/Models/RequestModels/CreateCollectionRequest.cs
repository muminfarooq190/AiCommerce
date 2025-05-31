using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record CreateCollectionRequest(
         string Name, string? BadgeLabel, bool IsFeatured);

}
