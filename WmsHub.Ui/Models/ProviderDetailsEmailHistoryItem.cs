using System;

namespace WmsHub.Ui.Models;

public class ProviderDetailsEmailHistoryItem
{
  public DateTimeOffset Created { get; set; }
  public DateTimeOffset Delivered { get; set; }
  public string Email { get; set; }
  public DateTimeOffset Sending { get; set; }
  public string Status { get; set; }
}
