using System;

namespace WmsHub.Business.Entities;

public interface IElectiveCareReferral
{
  int Id { get; set; }
  Guid ReferralId { get; set; }
  string Ubrn { get; }
}