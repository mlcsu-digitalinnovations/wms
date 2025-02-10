using System.Diagnostics.CodeAnalysis;

namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

[ExcludeFromCodeCoverage]
public class AttachmentList
{
  public List<AttachmentContainer>? Content { get; set; }
  public DateTimeOffset? Indexed { get; set; }
  public string? Status { get; set; }
}
