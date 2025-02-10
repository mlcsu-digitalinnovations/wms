using System;

namespace WmsHub.Common.Api.Interfaces;
public interface IGetDischargeUbrnResponse
{
  string DischargeMessage { get; set; }
  Guid Id { get; set; }
  string NhsNumber { get; set; }
  string Ubrn { get; set; }
}