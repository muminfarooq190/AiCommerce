using Sheared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheared.Models.RequestModels
{
    public sealed record UpsertValueRequest(
      AttributeDataType DataType,
      string? ValueString,
      decimal? ValueNumber,
      bool? ValueBool);
}
