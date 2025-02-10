using System;
using System.Diagnostics.CodeAnalysis;
using WmsHub.Common.Api.Interfaces;

namespace WmsHub.Common.Api.Models;

[ExcludeFromCodeCoverage]
public class GetDischargeUbrnResponse : IGetDischargeUbrnResponse
{
  public string DischargeMessage { get; set; }
  public Guid Id { get; set; }
  public string NhsNumber { get; set; }
  public string Ubrn { get; set; }  
}
