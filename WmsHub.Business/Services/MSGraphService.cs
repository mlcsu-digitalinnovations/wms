using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.MSGraph;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services;
public class MSGraphService : ServiceBase<Referral>, IMSGraphService
{
  private string _accessToken;
  private readonly ILogger _logger;
  private readonly HttpClient _httpClient;
  private readonly MsGraphOptions _options;
  private DateTimeOffset _tokenExpiry;

  public MSGraphService(DatabaseContext context,
    HttpClient httpClient,
    ILogger logger,
    IOptions<MsGraphOptions> options) : base(context)
  {
    if (_context == null)
    {
      throw new ArgumentNullException($"{nameof(context)} is null.");
    }

    _logger = logger
      ?? throw new ArgumentNullException($"{nameof(logger)} is null.");
    _httpClient = httpClient
      ?? throw new ArgumentNullException($"{nameof(HttpClient)} is null.");
    _options = options == null
      ? throw new ArgumentNullException(
        $"{nameof(IOptions<MsGraphOptions>)} is null.")
      : options.Value ?? throw new ArgumentNullException(
          $"{nameof(MsGraphOptions)} is null.");

    ValidateModelResult optionValidation =
      Validators.ValidateModel(options.Value);

    if (!optionValidation.IsValid)
    {
      throw new ValidationException(
        $"{nameof(IMessageOptions)}: {optionValidation.GetErrorMessage()}");
    }

    _tokenExpiry = DateTimeOffset.UtcNow;
  }

  /// <inheritdoc/>
  public async Task<ElectiveCareUser> CreateElectiveCareUserAsync(
    string bearerToken,
    CreateUser user)
  {
    if (user == null)
    {
      throw new ArgumentNullException(nameof(user));
    }

    try
    {
      if (string.IsNullOrWhiteSpace(bearerToken))
      {
        throw new ArgumentNullException(nameof(bearerToken));
      }

      _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
          Constants.HttpMethod.BEARER,
          bearerToken);
      string json = JsonConvert.SerializeObject(user);
      StringContent content = new(
        json,
        Encoding.UTF8,
        MediaTypeNames.Application.Json);
      string url = string.Format(
        CreateUser.ENDPOINT,
        _options.Endpoint,
        _options.ApiVersion);
      HttpResponseMessage response = await _httpClient.PostAsync(
        url,
        content);

      string result = await response.Content.ReadAsStringAsync();
      return JsonConvert.DeserializeObject<ElectiveCareUser>(result);
    }
    catch (Exception ex)
    {
      _logger.Error(ex, ex.Message);
      return null;
    }
  }

  /// <inheritdoc/>
  public async Task<bool> DeleteUserByIdAsync(
    string bearerToken,
    DeleteUser user)
  {
    if (user == null)
    {
      throw new ArgumentNullException(nameof(user));
    }

    try
    {
      if (string.IsNullOrWhiteSpace(bearerToken))
      {
        throw new ArgumentNullException(nameof(bearerToken));
      }

      _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
          Constants.HttpMethod.BEARER,
          bearerToken);

      HttpResponseMessage response = await _httpClient.DeleteAsync(
        string.Format(
          DeleteUser.ENDPOINT,
          _options.Endpoint,
          _options.ApiVersion,
          user.Id.ToString()));

      string result = await response.Content.ReadAsStringAsync();
      if (response.StatusCode == HttpStatusCode.NoContent
        || response.StatusCode == HttpStatusCode.OK)
      {
        return true;
      }

      throw new HttpRequestException($"{response.StatusCode}: {result} When " +
        $"deleting user using ID {user.Id}.");
    }
    catch (Exception ex)
    {
      _logger.Error(ex, ex.Message);
      throw;
    }
  }

  /// <inheritdoc/>
  public async Task<string> GetBearerTokenAsync()
  {
    if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry)
    {
      return _accessToken;
    }

    FormUrlEncodedContent content = new(new[]
    {
      new KeyValuePair<string, string>(
        Constants.HttpMethod.CLIENT_ID,
        _options.ClientId),
      new KeyValuePair<string, string>(
        Constants.HttpMethod.SCOPE,
        _options.Scope),
      new KeyValuePair<string, string>(
        Constants.HttpMethod.CLIENT_SECRET,
        _options.ClientSecret),
      new KeyValuePair<string, string>(
        Constants.HttpMethod.GRANT_TYPE,
        "client_credentials")
    });

    content.Headers.Clear();
    content.Headers.Add(
      Constants.HttpMethod.CONTENT_TYPE,
      Constants.HttpMethod.APPLICATIONURLENCODED);

    try
    {
      HttpResponseMessage response = await _httpClient.PostAsync(
        _options.TokenEndpointUrl,
        content);
      response.EnsureSuccessStatusCode();

      string responseBody = await response.Content.ReadAsStringAsync();
      JsonDocument jsonResponse = JsonDocument.Parse(responseBody);

      _accessToken = jsonResponse.RootElement
        .GetProperty(Constants.HttpMethod.ACCESS_TOKEN)
        .GetString();
      int expires_in = jsonResponse.RootElement
        .GetProperty(Constants.HttpMethod.EXPIRES_IN)
        .GetInt32();
      _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expires_in - 60);
    }
    catch (HttpRequestException ex)
    {
      _logger.Error("Error acquiring access token: " + ex.Message);
      throw new MsGraphBearerTokenRequestFailureException(
        "MsGraph bearer token request failed.",
        ex);
    }

    return _accessToken;
  }

  /// <inheritdoc/>
  public async Task<List<FilteredUser>> GetUsersByEmailAsync(
    string bearerToken,
    string email,
    string issuer)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new ArgumentNullException(nameof(email));
    }

    int maxRetries = 3;
    int delayMs = 1000;

    try
    {
      if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
      {
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
      }

      _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
          Constants.HttpMethod.BEARER,
          bearerToken);

      HttpResponseMessage response = null;
      bool shouldRetry = false;
      string url = "";
      for (int retry = 0; retry <= maxRetries; retry++)
      {
        try
        {
          url = string.Format(
            FilteredUser.ENDPOINT,
            _options.Endpoint,
            _options.ApiVersion,
            _options.UserSearchObjects,
            email,
            issuer);
          _logger.Debug($"Calling {url}");
          response = await _httpClient.GetAsync(url);

          shouldRetry = response.StatusCode == HttpStatusCode.TooManyRequests;
        }
        catch (Exception ex)
        {
          _logger.Error(ex, ex.Message);
          shouldRetry = true;
        }

        if (shouldRetry)
        {
          await Task.Delay(delayMs);
          delayMs *= 2;
        }
        else
        {
          break;
        }
      }

      string result = "";
      if (response == null)
      {
        throw new Exception($"No response received from endpoint when " +
          $"calling GET:{url}.");
      }

      if (response.Content != null)
      {
        result = await response.Content.ReadAsStringAsync();
      }

      if (response.StatusCode != HttpStatusCode.OK)
      {
        throw new ArgumentException($"{nameof(response)} returned message" +
          $" {result}.");
      }

      ElectiveCareGraphResponse responseObjects =
        JsonConvert.DeserializeObject<ElectiveCareGraphResponse>(result);

      return responseObjects.ElectiveCareUsers;

    }
    catch (Exception ex)
    {
      _logger.Error(ex, ex.Message);
      throw;
    }
  }
}
