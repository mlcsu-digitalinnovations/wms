using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Models.Results
{
  public class ProcessExecutionResult
  {
    /// <summary>
    /// Indicates that there were no errors
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// Indicates that the process ran to the end and was not interrupted
    /// </summary>
    public bool Completed { get; set; }
  }
}
