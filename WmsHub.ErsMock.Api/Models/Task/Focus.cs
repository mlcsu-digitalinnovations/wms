namespace WmsHub.ErsMock.Api.Models.Task;

public class Focus
{
  private const string FIRST_SEGMENT = "ReferralRequest";
  private const string THIRD_SEGMENT = "_history";

  public Focus(string value)
  {
    string[] splits = (value ?? "")
      .Split('/', StringSplitOptions.RemoveEmptyEntries);

    if (splits.Length != 4)
    {
      throw new ArgumentException("Must have 4 segments.", nameof(value));
    }

    if (splits[0] != FIRST_SEGMENT)
    {
      throw new ArgumentException(
        $"First segment must be {FIRST_SEGMENT}.",
        nameof(value));
    }

    Ubrn = splits[1];

    if (splits[2] != THIRD_SEGMENT)
    {
      throw new ArgumentException(
        $"Third segment must be {THIRD_SEGMENT}.",
        nameof(value));
    }
    
    Version = splits[3];
  }

  public string Ubrn { get; private set; }
  public string Version { get; private set; }
}
