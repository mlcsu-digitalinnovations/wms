using System;

namespace WmsHub.TextMessage.Api.Tests.Models
{
  public interface IDateTimeWrapper
  {
    DateTimeOffset Now { get; }
  }
}