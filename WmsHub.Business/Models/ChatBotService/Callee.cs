using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ChatBotService
{
  /// <summary>
  /// An individual to call. Additional properties may be included and passed
  /// through to the Contact Flow. Additional properties must be strings.
  /// </summary>
  public class Callee : ICallee
  {
    public Callee()
    {
      //Blank for unit tests
    }

    [Required]
    public string CallAttempt { get; set; }

    [Required]
    public string Id { get; set; }

    [Required]
    public string PrimaryPhone { get; set; }

    // TODO - Possibly add secondary phone to Arcus chat bot call list
    public string SecondaryPhone { get; set; } = "";

    [Required]
    public string ServiceUserName { get; set; }
  }
}
