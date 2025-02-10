
using System.Diagnostics.CodeAnalysis;
using WmsHub.Common.Models;

namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

[ExcludeFromCodeCoverage]
public class AttachmentContainer
{
  public ErsAttachment? Attachment { get; set; }
}
