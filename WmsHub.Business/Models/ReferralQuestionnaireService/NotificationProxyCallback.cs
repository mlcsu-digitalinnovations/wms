using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models;

public class NotificationProxyCallback
{
  [Required]
  public string Id { get; set; }
  [Required]
  public string ClientReference { get; set; }
  [Required]
  public NotificationProxyCallbackRequestStatus Status { get; set; }
  [Required]
  public DateTime StatusAt { get; set; }
}
