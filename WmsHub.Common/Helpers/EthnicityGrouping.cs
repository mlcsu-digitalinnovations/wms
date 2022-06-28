namespace WmsHub.Common.Helpers
{
  public class EthnicityGrouping
  {
    public string Triage { get; private set; }
    public string Group { get; private set; }
    public string Display { get; private set; }

    public string Ethnicity => Triage;
    public string ServiceUserEthnicity => Display;
    public string ServiceUserEthnicityGroup => Group;

    public EthnicityGrouping(string triage, string group, string display)
    {
      Triage = triage;
      Group = group;
      Display = display;
    }
  }
}