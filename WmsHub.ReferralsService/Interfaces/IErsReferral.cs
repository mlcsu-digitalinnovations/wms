using System.Collections.Generic;
using System.Net;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Models;
using WmsHub.ReferralsService.Models.Fhir;

namespace WmsHub.ReferralsService.Interfaces;
public interface IErsReferral
{
  List<ErsAttachment> Attachments { get; set; }
  List<AttachmentList> Contained { get; set; }
  List<string> ExcludedFiles { get; }
  public HttpStatusCode ErsResponseStatus { get; set; }
  public bool WasRetrievedFromErs { get; }
  string Id { get; set; }
  ReferralMetaData Meta { get; set; }

  void Finalise(string supportedFileTypes);
  /// <summary>
  /// Get the most recent ErsAttachment object of this referral.
  /// </summary>
  /// <returns>The most recent ErsAttachment or null if the referral has no attachments.</returns>
  ErsAttachment GetMostRecentAttachment();
}
