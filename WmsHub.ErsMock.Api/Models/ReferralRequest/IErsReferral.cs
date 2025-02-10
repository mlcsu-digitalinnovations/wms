using WmsHub.Common.Models;

namespace WmsHub.ErsMock.Api.Models.ReferralRequest;
public interface IErsReferral
{
  List<ErsAttachment> Attachments { get; set; }
  List<AttachmentList>? Contained { get; set; }
  List<string> ExcludedFiles { get; }
  string? Id { get; set; }
  ReferralMetaData? Meta { get; set; }

  void Finalise(string supportedFileTypes);
}
