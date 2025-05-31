using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.ResponseModels
{
    public sealed record AttributeValueDto(
          Guid AttributeId, string Name, string? UnitLabel,
          AttributeDataType DataType,
          string? ValueString, decimal? ValueNumber, bool? ValueBool);
}
