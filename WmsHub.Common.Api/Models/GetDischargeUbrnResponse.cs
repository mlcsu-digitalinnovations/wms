using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Common.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class GetDischargeUbrnResponse
  {
    public string Ubrn { get; set; }
    public string NhsNumber { get; set; }
    public string DischargeMessage { get; set; }
    public Guid Id { get; set; }
  }
}
