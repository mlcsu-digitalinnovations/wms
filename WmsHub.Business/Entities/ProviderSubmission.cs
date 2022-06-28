using System.Text.Json.Serialization;

namespace WmsHub.Business.Entities
{
  public class ProviderSubmission : ProviderSubmissionBase, IProviderSubmission
  {
    [JsonIgnore]
    public virtual Provider Provider { get; set; }
    [JsonIgnore]
    public virtual Referral Referral { get; set; }
  }
}
