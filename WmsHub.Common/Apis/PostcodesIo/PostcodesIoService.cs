using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Common.Apis.PostcodesIo;

namespace WmsHub.Common.Apis.Ods.PostcodesIo;

public class PostcodesIoService : IPostcodesIoService
{
  private const string ENGLAND = "ENGLAND";
  private readonly HttpClient _httpClient;
  private readonly PostcodesIoServiceOptions _options;
  private PostcodesIoLookupPostcode _lookupPostcode;

  public PostcodesIoService(
    HttpClient httpClient,
    IOptions<PostcodesIoServiceOptions> options)
  {
    _httpClient = httpClient;
    _options = options.Value;
  }

  public async Task<string> GetLsoaAsync(string postcode)
  {
    // TODO: Create a database store for postcode => lsoa
    // so we don't have to hit the API everytime.
    await GetPostcodeIoResultAsync(postcode);

    string lsoa = _lookupPostcode?.Result?.Codes?.Lsoa;

    return lsoa;
  }

  public async Task<bool> IsEnglishPostcodeAsync(string postcode)
  {
    await GetPostcodeIoResultAsync(postcode);

    bool isEnglish = _lookupPostcode?.Result?.Country?.ToUpper() == ENGLAND;

    return isEnglish;
  }

  public async Task<bool> IsUkOutwardCodeAsync(string outwardCode)
  {
    bool isUkOutwardCode = await DoesOutwardCodeExistAsync(outwardCode);

    return isUkOutwardCode;
  }

  public async Task<bool> IsUkPostcodeAsync(string postcode)
  {
    bool isUkPostcode = await DoesPostcodeExistAsync(postcode);

    return isUkPostcode;
  }

  private async Task<bool> DoesOutwardCodeExistAsync(string outwardCode)
  {
    if (string.IsNullOrWhiteSpace(outwardCode))
    {
      return false;
    }

    try
    {
      string json = await _httpClient.GetStringAsync(
        _options.GetLookupOutwardCodeUrl(outwardCode));

      PostcodesIoLookupOutwardCode result = JsonSerializer
        .Deserialize<PostcodesIoLookupOutwardCode>(
          json,
          new JsonSerializerOptions()
          {
            PropertyNameCaseInsensitive = true
          });

      return result?.Status == StatusCodes.Status200OK;
    }
    catch (HttpRequestException ex)
    {
      if (ex.StatusCode == HttpStatusCode.NotFound)
      {
        return false;
      }
      else
      {
        throw;
      }
    }
  }

  private async Task<bool> DoesPostcodeExistAsync(string postcode)
  {
    if (string.IsNullOrWhiteSpace(postcode))
    {
      return false;
    }

    string json = await _httpClient.GetStringAsync(
      _options.GetValidatePostcodeUrl(postcode));

    PostcodesIoValidatePostcode result = JsonSerializer
      .Deserialize<PostcodesIoValidatePostcode>(
        json,
        new JsonSerializerOptions()
        {
          PropertyNameCaseInsensitive = true
        });

    return result.Result;
  }

  private async Task<PostcodesIoLookupPostcode> GetPostcodeIoResultAsync(
    string postcode)
  {
    if (string.IsNullOrWhiteSpace(postcode))
    {
      return null;
    }

    try
    {
      string json = await _httpClient.GetStringAsync(
        _options.GetLookupPostcodeUrl(postcode));

      _lookupPostcode = JsonSerializer.Deserialize<PostcodesIoLookupPostcode>(
        json,
        new JsonSerializerOptions()
        {
          PropertyNameCaseInsensitive = true
        });

      return _lookupPostcode;
    }
    catch (HttpRequestException ex)
    {
      _lookupPostcode = null;
      if (ex.StatusCode == HttpStatusCode.NotFound)
      {
        return null;
      }
      else
      {
        throw;
      }
    }
  }
}