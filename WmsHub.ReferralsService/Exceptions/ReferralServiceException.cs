using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Exceptions
{
  public class ReferralServiceException : Exception
  {
    public ReferralServiceException() : base() { }

    public ReferralServiceException(string message) : base(message)
    { }

    public ReferralServiceException(string message, Exception innerException)
      : base(message, innerException)
    { }

    protected ReferralServiceException(
      SerializationInfo info, StreamingContext context) : base(info, context)
    { }
  }
}
