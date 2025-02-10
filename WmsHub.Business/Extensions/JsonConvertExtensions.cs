using Newtonsoft.Json;
namespace WmsHub.Business.Extensions;
public static class JsonConvertExtensions
{
  public static bool TryDeserializeObject<T>(
    this string jsonString, 
    out T result)
  {
    result = default;

    try
    {
      result = JsonConvert.DeserializeObject<T>(jsonString);
      return true;
    }
    catch (JsonException)
    {
      return false;
    }
  }
}