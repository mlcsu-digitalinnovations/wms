namespace WmsHub.Common.Extensions
{
  public static class SerilogExtensions
  {
    public static string LevelCodeToName(this string code)
    {
      if (string.IsNullOrWhiteSpace(code))
      {
        return code;
      }

      return code.ToUpper() switch
      {
        "VRB" => "Verbose",
        "DBG" => "Debug,",
        "INF" => "Information",
        "WRN" => "Warning",
        "ERR" => "Error",
        _ => "Fatal",
      };
    }
  }
}