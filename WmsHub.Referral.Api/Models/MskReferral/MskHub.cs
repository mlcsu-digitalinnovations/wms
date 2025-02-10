using System;

namespace WmsHub.Referral.Api.Models.MskReferral
{
  public class MskHub
  {
    public MskHub()
    { }

    public MskHub(string name, string odsCode)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      OdsCode = odsCode ?? throw new ArgumentNullException(nameof(odsCode));
    }

    public string Name { get; set; }
    public string OdsCode { get; set; }
  }
}
