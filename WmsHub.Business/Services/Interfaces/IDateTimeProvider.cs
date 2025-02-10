using System;

namespace WmsHub.Business.Services.Interfaces;
public interface IDateTimeProvider
{
  public DateTimeOffset Now { get; }
  public DateTimeOffset UtcNow { get; }
}
