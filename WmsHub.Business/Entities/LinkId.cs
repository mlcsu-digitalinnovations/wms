using WmsHub.Business.Entities.Interfaces;

namespace WmsHub.Business.Entities;
public class LinkId : ILinkId
{
  public string Id { get; set; }
  public bool IsUsed { get; set; }
}
