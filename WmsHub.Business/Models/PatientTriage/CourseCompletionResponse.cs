using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.PatientTriage
{
  public class CourseCompletionResponse: CourseCompletion
  {
    public virtual StatusType Status { get; set; } = StatusType.Valid;

    public virtual List<string> Errors { get; set; } = new List<string>();

    public virtual string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }
  }
}
