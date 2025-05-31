using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record CollectionDto(
        Guid CollectionId, string Name, string Slug,
        string? Badge, bool IsFeatured,
        Guid? HeroImageId, IEnumerable<Guid> ProductIds);
}
