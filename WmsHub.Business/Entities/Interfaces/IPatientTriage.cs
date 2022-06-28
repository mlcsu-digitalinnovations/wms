using System;
using System.Collections.Generic;

namespace WmsHub.Business.Entities
{
  public interface IPatientTriage
  {
    string TriageSection { get; set; }
    string Key { get; set;}
    string Descriptions { get; set; }
    int Value { get; set; }
    int CheckSum { get; set; }
    Guid Id { get; set; }
    bool IsActive { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    Guid ModifiedByUserId { get; set; }
  }
}