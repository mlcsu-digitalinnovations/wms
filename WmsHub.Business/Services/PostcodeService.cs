using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Common.Exceptions;

namespace WmsHub.Business.Services
{
  public class PostcodeService : IPostcodeService
  {
    private readonly PostcodeOptions _options;

    public PostcodeService(IOptions<PostcodeOptions> options, ILogger log)
    {
      _options = options.Value;
      Log.Logger = log;
    }

    public async Task<string> GetLsoa(string postcode)
    {

      if (string.IsNullOrWhiteSpace(postcode))
        throw new PostcodeNotFoundException("Postcode is null or white space.");

      // TODO: Create a database store for postcode => lsoa
      // so we don't have to hit the API everytime

      using HttpClient httpClient = new();
      try
      {
        string jsonDownload = await httpClient
          .GetStringAsync($"{_options.PostcodeIoUrl}{postcode}");

        PostcodeIoResult deserializedPostcodeResult =
          JsonConvert.DeserializeObject<PostcodeIoResult>(jsonDownload);

        string lsoa = deserializedPostcodeResult?.Result?.Codes?.Lsoa;

        if (lsoa == null)
        {
          throw new PostcodeNotFoundException(
            $"LSOA for postcode {postcode} not found.");
        }

        return lsoa;
      }
      catch (HttpRequestException ex)
      {
        if (ex.StatusCode == HttpStatusCode.NotFound)
        {
          throw new PostcodeNotFoundException(
            $"Postcode {postcode} not found.", ex);
        }
        else
          throw;
      }      
    }
  }
}