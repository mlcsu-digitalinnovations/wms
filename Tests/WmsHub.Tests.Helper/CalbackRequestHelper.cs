using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Tests.Helper
{
  public static class CallbackRequestHelper
  {
    public static class CallbackStatus
    {
      public const string Delivered = "delivered";
      public const string Failure = "failure";
      public const string PermFailure = "permanent-failure";
      public const string TempFailure = "temporary-failure";
      public const string TechFailure = "technical-failure";
    }

    public static class NotificationType
    {
      public const string Email = "email";
      public const string TextMessage = "sms";
    }
  }
}
