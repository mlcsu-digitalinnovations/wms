using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Console
{
  public static class Enums
  {
    public enum ExitCode : int
    {
      Success = 0,
      Failure = 1,
      CriticalFailure = 2
    }

  }
}
