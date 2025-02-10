using System;
using WmsHub.Business.Services.Interfaces;

namespace WmsHub.Business.Models;

public class ProviderAwaitingStartReferral : IProviderAwaitingStartReferral
{
  public DateTimeOffset? DateOfProviderSelection { get; set; }  
  public string ProviderName { get; set; }
  public string ProviderUbrn { get; set; }
}
