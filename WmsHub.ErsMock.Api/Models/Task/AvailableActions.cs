namespace WmsHub.ErsMock.Api.Models.Task;

public class AvailableActions
{
  public List<AvailableActionEntry>? Entry { get; set; }
}

public class AvailableActionEntry
{
  public AvailableActionResource? Resource { get; set; }
}

public class AvailableActionResource
{
  public AvailableActionCode? Code { get; set; }
}

public class AvailableActionCode
{
  public List<AvailableActionCodingItem>? Coding { get; set; }
}

public class AvailableActionCodingItem
{
  public string? Code { get; set; }
}
