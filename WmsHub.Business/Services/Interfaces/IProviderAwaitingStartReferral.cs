using System;

namespace WmsHub.Business.Services.Interfaces
{
  public interface IProviderAwaitingStartReferral
  {    
    DateTimeOffset? DateOfProviderSelection { get; set; }
    string ProviderName { get; set; }
    string ProviderUbrn { get; set; }
  }
}
