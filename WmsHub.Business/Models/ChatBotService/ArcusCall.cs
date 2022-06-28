using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ChatBotService
{
  public class ArcusCall : IArcusCall
  {
    public ArcusCall()
    {
      //Blank constructor for testing
    }
    public ArcusCall(ArcusOptions settings)
    {
      ContactFlowName = settings.ContactFlowName;
    }

    [Required]
    /// The name of the contact flow to add the call list to.
    public string ContactFlowName { get; set; }

    [Required]
    public string Mode { get; set; } = ModeType.Replace.ToString();

    [Required, MinLength(1), MaxLength(1000)]
    public virtual IEnumerable<ICallee> Callees { get; set; }

    [JsonIgnore]
    public virtual int NumberOfCallsToMake => Callees.Count();

  }
}
