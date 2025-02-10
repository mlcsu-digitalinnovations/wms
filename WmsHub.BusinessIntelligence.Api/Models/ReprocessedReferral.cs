using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class ReprocessedReferral:IReprocessedReferral
  { 
    public string Ubrn { get; set; }
    public string InitialStatus { get; set; }
    public string InitialStatusReason { get; set; }
    public bool Reprocessed { get; set; }
    public bool SuccessfullyReprocessed { get; set; }
    public bool Uncancelled { get; set; }
    public bool CurrentlyCancelled { get; set; }
    public string CurrentlyCancelledStatusReason { get; set; }
    public DateTimeOffset? DateOfReferral { get; set; }
    public string ReferringGpPracticeCode { get; set; }
  }
}
