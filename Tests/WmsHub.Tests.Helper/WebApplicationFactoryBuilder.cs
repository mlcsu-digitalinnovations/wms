using System.Collections.Generic;

namespace WmsHub.Tests.Helper;
public class WebApplicationFactoryBuilder
{
  public string[] PoliciesToInclude { get; set; }
  public Dictionary<string, string> InMemoryValues { get; set; }
  public Dictionary<string, string> EnvironmentVariables { get; set; }
}
