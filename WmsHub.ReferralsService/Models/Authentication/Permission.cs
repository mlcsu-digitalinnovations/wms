using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Referrals.Models.Authentication
{
  [ExcludeFromCodeCoverage]
  public class Permission
  {
    public string businessFunction { get; set; }
    public string orgIdentifier { get; set; }
  }
}
