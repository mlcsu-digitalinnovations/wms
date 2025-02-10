using System;
using WmsHub.Business.Services.Interfaces;

namespace WmsHub.Business.Services;
public class DateTimeProvider : IDateTimeProvider
{
  public DateTimeOffset Now => DateTimeOffset.Now;
  public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
