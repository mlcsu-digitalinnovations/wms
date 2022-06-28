using System;

namespace WmsHub.Business.Models.Notify
{
  public interface ISmsTemplate
  {
    Guid Id { get; set; }
    string Name { get; set; }
  }
}