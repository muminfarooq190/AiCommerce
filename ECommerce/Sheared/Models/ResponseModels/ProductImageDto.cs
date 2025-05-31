using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record ProductImageDto(Guid MediaFileId, string Uri, int SortOrder);
}
