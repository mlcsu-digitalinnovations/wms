using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Enums
{
  public enum FailType
  {
    //Do not validate
    NoTest,
    // If on fails then fail all
    FailAllOnAny,
    //only fail individual
    FailOnly
  }
}
