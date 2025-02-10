// Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS8618
namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

public abstract class ARequestBase
{
  public string ResourceType { get; set; }
  public Parameter[] Parameter { get; set; }
}

public class Parameter
{
  public string Name { get; set; }
  public ValueCoding? ValueCoding { get; set; }
  public ValueIdentifier? ValueIdentifier { get; set; }
  public string? ValueString { get; set; }
}

public class ValueCoding
{
  public string Code { get; set; }
  public string System { get; set; }
}


public class ValueIdentifier
{
  public string System { get; set; }
  public string Value { get; set; }
}
