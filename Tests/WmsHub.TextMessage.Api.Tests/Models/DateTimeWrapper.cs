using System;

namespace WmsHub.TextMessage.Api.Tests.Models
{
  public class DateTimeWrapper : IDateTimeWrapper
  {
    private DateTimeOffset? _dateTime;

    public DateTimeWrapper()
    {
      _dateTime = null;
    }

    public DateTimeWrapper(DateTimeOffset fixedDateTime)
    {
      _dateTime = fixedDateTime;
    }

    public DateTimeOffset Now
    {
      get
      {
        return _dateTime ?? DateTimeOffset.Now;
      }
    }
  }
}