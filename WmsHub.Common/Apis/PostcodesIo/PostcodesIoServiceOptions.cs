namespace WmsHub.Common.Apis.Ods.PostcodesIo;

public class PostcodesIoServiceOptions
{
  public const string SectionKey = "Postcode";

  internal const string BASE_URL = "https://api.postcodes.io/";

  private const string OUTCODES_PATH = "outcodes/";
  private const string POSTCODES_PATH = "postcodes/";
  private const string VALIDATE_PATH = "/validate";

  public string BaseUrl
  {
    get => _baseUrl;
    set => _baseUrl = value.EndsWith("/") ? value : value + "/";
  }

  public string OutcodesPath
  {
    get => _outcodesPath;
    set => _outcodesPath = value.EndsWith("/") ? value : value + "/";
  }

  public string PostcodesPath
  {
    get => _postcodesPath;
    set => _postcodesPath = value.EndsWith("/") ? value : value + "/";
  }

  public string ValidatePath
  {
    get => _validatePath;
    set => _validatePath = value.StartsWith("/") ? value : "/" + value;
  }

  public string GetLookupOutwardCodeUrl(string postcode)
  {
    return $"{BaseUrl}{OutcodesPath}{postcode}";
  }

  public string GetLookupPostcodeUrl(string postcode)
  {
    return $"{BaseUrl}{PostcodesPath}{postcode}";
  }

  public string GetValidatePostcodeUrl(string postcode)
  {
    return $"{BaseUrl}{PostcodesPath}{postcode}{ValidatePath}";
  }

  private string _baseUrl = BASE_URL;
  private string _outcodesPath = OUTCODES_PATH;
  private string _postcodesPath = POSTCODES_PATH;
  private string _validatePath = VALIDATE_PATH;
}
