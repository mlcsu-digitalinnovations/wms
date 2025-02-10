using System;

namespace WmsHub.Business.Models.Tracing;
public class TraceIssueReferral
{
  public Guid Id { get; set; }
  public string ReferringGpPracticeNumber { get; set; }
}
