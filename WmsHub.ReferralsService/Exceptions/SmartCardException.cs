using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Exceptions
{
  public class SmartCardException:Exception
  {
    public SmartCardException() : base() { }

    public SmartCardException(string message) : base(message)
    { }

    public SmartCardException(string message, Exception innerException)
      : base(message, innerException)
    { }

    protected SmartCardException(
      SerializationInfo info, StreamingContext context) : base(info, context)
    { }
  }
}
