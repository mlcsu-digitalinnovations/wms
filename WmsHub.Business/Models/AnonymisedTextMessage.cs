using System;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models
{
  public class AnonymisedTextMessage
  {
    private string _number;

    public string Number { set => _number = value; }
    public string AnonymisedNumber
    {
      get => _number.Mask('*', 3);
    }
    public DateTimeOffset Sent { get; set; }
    public string Outcome { get; set; }
  }
}