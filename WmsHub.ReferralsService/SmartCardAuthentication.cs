using IdentityAgentApi;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Referrals.Models.Authentication;
using WmsHub.Common.Helpers;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.Common.Models;

namespace WmsHub.ReferralsService
{
  public class SmartCardAuthentication : ISmartCardAuthentictor
  {
    private readonly Config _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ILogger _auditLogger;

    public ErsSession ActiveSession { get; set; }


    public SmartCardAuthentication(
      Config configuration,
      IHttpClientFactory httpClientFactory,
      ILogger logger,
      ILogger auditLogger = null)
    {
      _config = configuration;
      _httpClientFactory = httpClientFactory;
      _logger = logger;
      _auditLogger = auditLogger ?? logger;
    }

    public bool IsAuthenticated
    {
      get
      {
        return ActiveSession.IsAuthenticated;
      }
    }

    /// <summary>
    /// Connect to a smart cart and retrieve the token
    /// </summary>
    /// <returns>The current Smart Card Token</returns>
    public async void ConnectToSmartCard()
    {
      if (ActiveSession != null)
      {
        if (ActiveSession.IsAuthenticated)
        {
          _logger.Information("Terminating previous session");
          await TerminateSession();
        }
      }


      ActiveSession = new ErsSession();

      GaTicket.Initialize();
      string result;
      string safeToLogTicket = "None";
      int returnCode = GaTicket.GetTicket(out result);
      if (returnCode == 0)
      {
        _logger.Debug("Connect to Smart Card: " +
          $"{GaTicket.ReturnCodeAsString(returnCode)}");
        if (result.Length > 27)
        {
          safeToLogTicket = $"...{result.Substring(11, 16)}...";
        }
        else
        {
          safeToLogTicket = $"A malformed ticket with a length of " +
            $"{result.Length} characters";
        }
      }
      else
      {
        _logger.Error("Connect to Smart Card: " +
         $"{GaTicket.ReturnCodeAsString(returnCode)}");
      }
      _logger.Debug($"Ticket: {safeToLogTicket}");
      ActiveSession.SmartCardToken = result;
    }

    public async Task<bool> CreateSession()
    {
      try
      {
        if (string.IsNullOrWhiteSpace(_config.Data.OverrideGaTicket))
        {
          if (ActiveSession == null)
          {
            ConnectToSmartCard();
          }
          else if (!ActiveSession.SmartCardIsAuthenticated)
          {
            ConnectToSmartCard();
          }
        }
        else
        {
          ActiveSession = new ErsSession()
          {
            SmartCardToken = _config.Data.OverrideGaTicket
          };
          _logger.Information("Did not connect to smart card, as an override "
            + "token was found.");
        }
        if (ActiveSession.SmartCardIsAuthenticated == false)
        {
          _logger.Error("Smart card not authenticated");
        }
        else
        {
          string smartCardToken = ActiveSession.SmartCardToken;

          HttpClient client = _httpClientFactory
            .CreateClient(Config.HttpClientWithClientCertificate);

          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("XAPI_ASID", _config.Data.AccreditedSystemsID);
          client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

          //Message Body
          RequestBody requestBody = new RequestBody()
          {
            token = ActiveSession.SmartCardToken,
            typeInfo = "uk.nhs.ers.xapi.dto.v1.session.ProfessionalSession"
          };
          string json = JsonConvert.SerializeObject(requestBody);

          // Refactored to use SendAsync because PostAsync won't send the
          // default request headers
          var request = new HttpRequestMessage(
            HttpMethod.Post,
            _config.Data.CreateProfessionalSessionPath)
          {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
          };

          string msgTemplate =
            "EventType=API Establish Session;" +
            "ActionType=POST;" +
            "UUID={uuid};" +
            "SessionID={sessionId};" +
            "ASID={asid};" +
            "FQDN={fqdn};" +
            "ApiMethod={apiMethod};";

          _logger.Debug($"A001:Establish Session");
          _auditLogger.Information(msgTemplate,
            null,
            null,
            _config?.Data?.AccreditedSystemsID ?? "Unknown",
            _config?.Data?.Fqdn ?? "Unknown",
            "A001-Start");

          HttpResponseMessage httpResponse = await client.SendAsync(request);

          if (httpResponse.IsSuccessStatusCode)
          {
            ActiveSession =
              await httpResponse.Content.ReadFromJsonAsync<ErsSession>();
            ActiveSession.SmartCardToken = smartCardToken;

            _auditLogger.Information(msgTemplate,
              ActiveSession?.User?.Identifier ?? "Unknown",
              ActiveSession?.Id ?? "Unknown",
              _config.Data.AccreditedSystemsID,
              _config.Data.Fqdn,
              "A001-End");

            //Select Professional Role
            client.DefaultRequestHeaders
              .Add("HTTP_X_SESSION_KEY", ActiveSession.Id);
            requestBody.permission = new Permission()
            {
              businessFunction = _config.Data.BusinessFunction,
              orgIdentifier = _config.Data.OrgIdentifier
            };
            json = JsonConvert.SerializeObject(requestBody);

            // Refactored to use SendAsync because PostAsync won't send the
            // default request headers
            request = new HttpRequestMessage(
              HttpMethod.Put,
              $"{_config.Data.ProfessionalSessionSelectRolePath}"
              + $"{ActiveSession.Id}")
            {
              Content = new StringContent(
                json, Encoding.UTF8, "application/json")
            };

            msgTemplate =
              "EventType=API Profession login attempt;" +
              "ActionType=PUT;" +
              "UUID={uuid};" +
              "SessionID={sessionId};" +
              "OrgName={orgName};" +
              "BusinessFunction={businessFunction};" +
              "ASID={asid};" +
              "FQDN={fqdn};" +
              "ApiMethod={apiMethod};";

            _logger.Debug("A002: Professional Login Attempt");
            _auditLogger.Information(msgTemplate,
              ActiveSession?.User?.Identifier ?? "Unknown",
              ActiveSession?.Id ?? "Unknown",
              ActiveSession?.Permission?.OrgName ?? "Unknown",
              ActiveSession?.Permission?.BusinessFunction ?? "Unknown",
              _config.Data.AccreditedSystemsID,
              _config.Data.Fqdn,
              "A002-Start");

            httpResponse = await client.SendAsync(request);

            ActiveSession =
              await httpResponse.Content.ReadFromJsonAsync<ErsSession>();
            ActiveSession.SmartCardToken = smartCardToken;

            _auditLogger.Information(msgTemplate,
              ActiveSession?.User?.Identifier ?? "Unknown",
              ActiveSession?.Id ?? "Unknown",
              ActiveSession?.Permission?.OrgName ?? "Unknown",
              ActiveSession?.Permission?.BusinessFunction ?? "Unknown",
              _config.Data.AccreditedSystemsID,
              _config.Data.Fqdn,
              "A002-End");
          }
          else
          {
            _logger.Error($"Failed to authenticate the new professional " +
              $"session on the eReferrals system. Status code " +
              $"{(int)httpResponse.StatusCode} : {httpResponse.ReasonPhrase}");
            DisconnectSmartCard();

            return false;
          }

        }

      }
      catch (Exception ex)
      {
        _logger.Error(ex.Message);
        DisconnectSmartCard();
        return false;
      }
      DisconnectSmartCard();
      return true;
    }

    public async Task<bool> TerminateSession()
    {
      try
      {
        _logger.Debug("Terminating Session.");
        if (ActiveSession == null)
        {
          _logger.Debug("No Session to Terminate");
          return false;
        }
        if (ActiveSession.IsAuthenticated == true)
        {
          HttpClient client = _httpClientFactory.
            CreateClient(Config.HttpClientWithClientCertificate);

          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("XAPI_ASID", _config.Data.AccreditedSystemsID);
          client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
          client.DefaultRequestHeaders
            .Add("HTTP_X_SESSION_KEY", ActiveSession.Id);

          //Message Body
          RequestBody requestBody = new RequestBody()
          {
            token = ActiveSession.SmartCardToken,
            typeInfo = "uk.nhs.ers.xapi.dto.v1.session.ProfessionalSession"
          };
          string json = JsonConvert.SerializeObject(requestBody);

          // Refactored to use SendAsync because PostAsync won't send the
          // default request headers
          var request = new HttpRequestMessage(
              HttpMethod.Delete,
              $"{_config.Data.ProfessionalSessionSelectRolePath}"
              + $"{ActiveSession.Id}")
          {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
          };

          string msgTemplate =
            "EventType=API Close Session;" +
            "ActionType=DELETE;" +
            "UUID={uuid};" +
            "SessionID={sessionId};" +
            "OrgName={orgname};" +
            "BusinessFunction={businessFunction};" +
            "ASID={asid};" +
            "FQDN={fqdn};" +
            "APIMethod={apiMethod};";

          _logger.Debug("A003:Close Session");
          _auditLogger.Information(msgTemplate,
            ActiveSession?.User?.Identifier ?? "Unknown",
            ActiveSession?.Id ?? "Unknown",
            ActiveSession?.Permission?.OrgName ?? "Unknown",
            ActiveSession?.Permission?.BusinessFunction ?? "Unknown",
            _config.Data.AccreditedSystemsID,
            _config.Data.Fqdn,
            "A003-Start");

          HttpResponseMessage httpResponse = await client.SendAsync(request);
          if (httpResponse.IsSuccessStatusCode)
          {
            ActiveSession = null;

            _auditLogger.Information(msgTemplate,
              null,
              null,
              null,
              null,
              _config.Data.AccreditedSystemsID,
              _config.Data.Fqdn,
              "A003-End");

            return true;
          }
          else
          {
            _logger.Error($"Failed to terminate the session on the " +
              $"eReferrals system. Terminate Session request returned status " +
              $"code {(int)httpResponse.StatusCode} : " +
              $"{httpResponse.ReasonPhrase}");
            return false;
          }
        }
        else
        {
          _logger.Error("Session could not terminate as it was never started.");
          return false;
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Failed to TerminateSession.");
        return false;
      }
    }

    public void DisconnectSmartCard()
    {
      if (ActiveSession != null)
      {
        if (_config.Data.DisconnectSmartCardAfterUse)
        {
          _logger.Information("Smart Card is being disconnected automatically.  " +
            "To change this behaviour, set the 'DisconnectSmartCardAfterUse' " +
            "flag in the configuration to FALSE.");
          GaTicket.DestroyTicket();
        }
        else
        {
          _logger.Information("Smart Card session is being retained.  To change " +
            "this behaviour, set the 'DisconnectSmartCardAfterUse' flag in the " +
            "configuration to TRUE.");
          GaTicket.DestroyTicket();
        }
      }
    }
  }
}