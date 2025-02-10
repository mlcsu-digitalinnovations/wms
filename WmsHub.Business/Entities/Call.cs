using System.Text.Json.Serialization;

namespace WmsHub.Business.Entities
{
  public class Call : CallBase, ICall
  {
    [JsonIgnore]
    public virtual Referral Referral { get; set; }
  }
}