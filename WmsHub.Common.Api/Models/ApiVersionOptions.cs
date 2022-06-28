namespace WmsHub.Common.Api.Models
{
  public class ApiVersionOptions
  {
    private int _defaultMajor = 1;
    private int _defaultMinor = 0;

    public int DefaultMajor
    {
      get => _defaultMajor;
      set
      {
        if (value <= 0) value = 1;
        _defaultMajor = value;
      }
    }
    public int DefaultMinor
    {
      get => _defaultMinor;
      set
      {
        if (value < 0) value = 0;
        _defaultMinor = value;
      }
    }
  }
}
