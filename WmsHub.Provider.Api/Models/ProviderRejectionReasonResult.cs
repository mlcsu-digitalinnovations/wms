using System;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Provider.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class ProviderRejectionReasonResult
  {
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
  }
}