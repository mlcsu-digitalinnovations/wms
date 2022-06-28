using Hl7.Fhir.Rest;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Models;
using WmsHub.Referral.Api.Models;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.ReferralsService.Models.Results;
using WmsHub.ReferralsService.Pdf;
using static WmsHub.Common.Enums;
using static WmsHub.ReferralsService.Enums;

namespace WmsHub.ReferralsService
{
  public class ReferralsDataProvider : IReferralsDataProvider
  {
    private readonly Config _config;
    private readonly ISmartCardAuthentictor _auth;
    private readonly ILogger _log;
    private readonly ILogger _auditlog;

    public string ServiceIdentifier { get; set; }

    /// <summary>
    /// Create a new instance of the Referrals Service Data Provider for eRS
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <param name="smartCardAuthentication">Smart Card Authentication</param>
    /// <param name="log">primary logger</param>
    /// <param name="auditLog">Logs for API audit logs</param>
    public ReferralsDataProvider(
      Config configuration,
      ISmartCardAuthentictor smartCardAuthentication,
      ILogger log, ILogger auditLog = null)
    {
      _config = configuration;
      _auth = smartCardAuthentication;
      _log = log;
      _auditlog = auditLog??log;
    }

    /// <summary>
    /// This is used to create a new CRI document record where one does not
    /// exist for some reason (such as the Referral record was created using
    /// a CSV batch rather than a download from eReferrals.
    /// </summary>
    /// <param name="criRecord"></param>
    /// <returns></returns>
    public async Task<CreateCriRecordResult> CreateCriRecord(
      CriCreateRequest criRecord)
    {
      CreateCriRecordResult result = new CreateCriRecordResult();

      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
          .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
        HttpResponseMessage response =
          await client.PutAsJsonAsync($"CriPut/{criRecord.Ubrn}", criRecord);
        if (response.IsSuccessStatusCode)
        {
          result.Success = true;
          _log.Debug("UBRN {ubrn}: Cri Record Created",
            criRecord?.Ubrn);
        }
        else
        {
          result.Success = false;
          _log.Warning("UBRN {ubrn}: CRI Record was not created. " +
              "Status: {statuscode} : {reason}",
            criRecord?.Ubrn,
            (int)response.StatusCode,
            response.ReasonPhrase);
        }
      }
      return result;
    }

    public async Task<CreateReferralResult> CreateReferral(
      ReferralPost referral)
    {
      CreateReferralResult result = new CreateReferralResult();

      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
          .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
        HttpResponseMessage response =
          await client.PostAsJsonAsync("", referral);
        if (response.IsSuccessStatusCode)
        {
          result.Success = true;
          _log.Debug("UBRN {ubrn}: Created Referral.",
            referral?.Ubrn);
        }
        else
        {
          result.Success = false;
          _log.Warning("UBRN {ubrn}: Error Creating Referral. " +
              "Status: {statuscode} : {reason}",
            referral?.Ubrn,
            (int)response.StatusCode,
            response.ReasonPhrase);
        }
      }
      return result;
    }

    public async Task<UpdateCriRecordResult> UpdateCriRecord(
      CriUpdateRequest criRecord,
      string ubrn)
    {
      UpdateCriRecordResult result = new UpdateCriRecordResult();

      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
          .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
        HttpResponseMessage response =
          await client.PutAsJsonAsync($"CriPut/{ubrn}", criRecord);
        if (response.IsSuccessStatusCode)
        {
          result.Success = true;
          _log.Debug("UBRN {ubrn}: Updated CRI Record", 
            ubrn);
        }
        else
        {
          result.Success = false;
          _log.Warning("UBRN {ubrn}: CRI Record was not updated. " +
              "Status: {statuscode} : {reason}",
            ubrn,
            (int)response.StatusCode,
            response.ReasonPhrase);
        }
      }
      return result;
    }

    public async Task<UpdateReferralResult> UpdateReferral(
      ReferralPut referral,
      string ubrn)
    {
      UpdateReferralResult result = new UpdateReferralResult();

      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
          .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
        HttpResponseMessage response =
          await client.PutAsJsonAsync($"{ubrn}", referral);

        if (response.IsSuccessStatusCode)
        {
          result.Success = true;
          _log.Debug("UBRN {ubrn}: Updated Referral.",
              ubrn);
        }
        else
        {
          result.Success = false;
          _log.Warning("UBRN {ubrn}: Referral Record was not updated. " +
              "Status: {statuscode} : {reason}",
            ubrn,
            (int)response.StatusCode,
            response.ReasonPhrase);
        }
      }
      return result;
    }

    public async Task<UpdateReferralResult>
      UpdateReferralCancelledByEReferral(string ubrn)
    {
      UpdateReferralResult result = new UpdateReferralResult();

      using (HttpClient client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
         .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
        HttpResponseMessage response =
          await client.DeleteAsync($"{ubrn}");
        if (response.IsSuccessStatusCode)
        {
          result.Success = true;
          _log.Warning("UBRN {ubrn}: Updated Referral Record Cancelled by " +
              "eReferrals",
            ubrn);
        }
        else
        {
         
          result.Success = false;
          _log.Warning("UBRN {ubrn}: Referral Record Cancelled by eReferrals" +
              "was not updated.  Status: {statuscode} : {reason}",
            ubrn,
            (int)response.StatusCode,
            response.ReasonPhrase);
        }
      }

      return result;
    }

    public async Task NewNhsNumberMismatch(
      ReferralNhsNumberMismatchPost request)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.BaseAddress =
            new Uri(_config.Data.HubRegistrationExceptionAPIPath);
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
          HttpResponseMessage response =
            await client.PostAsJsonAsync("NhsNumberMismatch",
              request);
          if (response.IsSuccessStatusCode)
          {
            _log.Debug("UBRN {ubrn}: NHS Number Mismatch Record Created",
              request.Ubrn);
          }
          else
          {
            _log.Warning("UBRN {ubrn}: NHS Number Mismatch Record was not " +
                "created.  Status: {statuscode} : {reason}",
              request.Ubrn,
              (int)response.StatusCode,
              response.ReasonPhrase);
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex, ex.Message);
      }
    }

    public async Task UpdateNhsNumberMismatch(
      ReferralNhsNumberMismatchPost request)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.BaseAddress =
            new Uri(_config.Data.HubRegistrationExceptionAPIPath);
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
          HttpResponseMessage response =
            await client.PutAsJsonAsync("NhsNumberMismatch", request);
          if (response.IsSuccessStatusCode)
          {
            _log.Debug("UBRN {ubrn}: Record updated for NHS Number Mismatch",
              request.Ubrn);
          }
          else
          {
            _log.Warning("UBRN {ubrn}: NHS Number Mismatch Record was not " +
                "updated.  Status: {statuscode} : {reason}",
              request.Ubrn,
              (int)response.StatusCode,
              response.ReasonPhrase);
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex, ex.Message);
      }
    }

    public async Task NewMissingAttachment(
      ReferralMissingAttachmentPost request)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.BaseAddress =
            new Uri(_config.Data.HubRegistrationExceptionAPIPath);
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
          HttpResponseMessage response =
            await client.PostAsJsonAsync("MissingAttachment", request);

          if (response.IsSuccessStatusCode)
          {
            _log.Debug("UBRN {ubrn}: Missing Attachment Record created.",
              request.Ubrn);
          }
          else
          {
            _log.Warning("UBRN {ubrn}: Missing Attachment Record was not " +
                "created.  Status: {statuscode} : {reason}",
              request.Ubrn,
              (int)response.StatusCode,
              response.ReasonPhrase);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Warning(ex.Message);
      }
    }

    public async Task UpdateMissingAttachment(
      ReferralMissingAttachmentPost request)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.BaseAddress =
            new Uri(_config.Data.HubRegistrationExceptionAPIPath);
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
          HttpResponseMessage response =
            await client.PutAsync($"MissingAttachment/{request.Ubrn}", null);
          if (response.IsSuccessStatusCode)
          {
            _log.Debug("UBRN {ubrn}: Missing Attachment Record Updated.",
              request.Ubrn);
          }
          else
          {
            _log.Warning("UBRN {ubrn}: Missing Attachment Record was not " +
                "updated.  Status: {statuscode} : {reason}",
              request.Ubrn,
              (int)response.StatusCode,
              response.ReasonPhrase);
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex, ex.Message);
      }
    }

    public async Task NewInvalidAttachment(
      ReferralInvalidAttachmentPost invalidAttachmentPost)
    {
      try
      {
        using (var client = new HttpClient())
        {
          client.BaseAddress =
            new Uri(_config.Data.HubRegistrationExceptionAPIPath);
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
          HttpResponseMessage response =
            await client.PostAsJsonAsync("InvalidAttachment",
              invalidAttachmentPost);
          if (response.IsSuccessStatusCode)
          {
            _log.Debug("UBRN {ubrn}: Invalid Attachment Record created",
              invalidAttachmentPost.Ubrn);
          }
          else
          {
            _log.Warning("UBRN {ubrn}: Invalid Attachment Record was not " +
                "created.  Status: {statuscode} : {reason}",
              invalidAttachmentPost.Ubrn,
              (int)response.StatusCode,
              response.ReasonPhrase);
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex, ex.Message);
      }
    }

    public async Task UpdateInvalidAttachment(
      ReferralInvalidAttachmentPost invalidAttachmentPost)
    {
      try
      {
        _log.Debug("Updating HUB with invalid attachment for record " +
          $"{invalidAttachmentPost.Ubrn}");
        using (var client = new HttpClient())
        {
          client.BaseAddress =
            new Uri(_config.Data.HubRegistrationExceptionAPIPath);
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders
            .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
          HttpResponseMessage response =
            await client.PutAsync(
              $"InvalidAttachment/{invalidAttachmentPost.Ubrn}", null);
          if (response.IsSuccessStatusCode)
          {
            _log.Debug("UBRN {ubrn}: Invalid Attachment Record Updated",
              invalidAttachmentPost.Ubrn);
          }
          else
          {
            _log.Warning("UBRN {ubrn}: Invalid Attachment Record was not " +
                "updated.  Status: {statuscode} : {reason}",
              invalidAttachmentPost.Ubrn,
              (int)response.StatusCode,
              response.ReasonPhrase);
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex, ex.Message);
      }
    }

    /// <summary>
    /// Get the referral list from WMS Hub
    /// </summary>
    /// <param name="useServiceId">When FALSE, all referrals will be requested
    /// from the hub in one call</param>
    /// <returns>A list of active UBRNs</returns>
    public async Task<RegistrationListResult> GetReferralList(bool useServiceId)
    {
      RegistrationListResult result = new RegistrationListResult();
      result.ReferralUbrnList = new RegistrationList();

      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
          .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);

        result.Success = true;
        if (useServiceId)
        {
          foreach (string serviceId in _config.Data.ServiceIdentifiers)
          {
            List<GetActiveUbrnResponse> ubrnList = await GetReferralList(
              client,
              serviceId);

            if (ubrnList == null)
            {
              result.Success = false;
              break;
            }
            else
            {
              result.ReferralUbrnList.Ubrns.AddRange(ubrnList);
            }
          }
        }
        else
        {
          List<GetActiveUbrnResponse> ubrnList = await GetReferralList(client);
          if (ubrnList == null)
          {
            result.Success = false;
          }
          else
          {
            result.ReferralUbrnList.Ubrns.AddRange(ubrnList);
          }
        }
      }
      if (!result.Success)
      {
        _log.Error("Failed with to get referral list from WMS.");
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    private async Task<List<GetActiveUbrnResponse>> GetReferralList(
      HttpClient client, 
      string serviceId)
    {
      List<GetActiveUbrnResponse> result;

      _log.Debug("ServiceId {serviceid}: Retrieving referral list.",
        serviceId);

      HttpResponseMessage response = await client.GetAsync($"ubrns/{serviceId}");
      if (response.IsSuccessStatusCode)
      {
        List<GetActiveUbrnResponse> ubrnList = await response
          .Content.ReadFromJsonAsync<List<GetActiveUbrnResponse>>();

        result = ubrnList;
        _log.Debug("ServiceId {serviceid}: Retrieved {count} records from WMS.",
          serviceId,
          ubrnList.Count);
      }
      else
      {
        result = null;
        _log.Fatal("ServiceId {serviceid}: Failed to get referral list from " +
            "WMS.  Status: {statuscode} : {reason}",
          serviceId,
          (int)response.StatusCode,
          response.ReasonPhrase);
      }
      return result;
    }

    private async Task<List<GetActiveUbrnResponse>> GetReferralList(
      HttpClient client)
    {
      List<GetActiveUbrnResponse> result;

      _log.Debug("Retrieving referral list for all services.");
      string url = "ubrns";
      HttpResponseMessage response = await client.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        List<GetActiveUbrnResponse> ubrnList = await response
          .Content.ReadFromJsonAsync<List<GetActiveUbrnResponse>>();
        result = ubrnList;
        _log.Debug("Retrieved {count} records from WMS.",
          ubrnList.Count);
      }
      else
      {
        _log.Fatal("Failed to get referral list from WMS.  " +
            "Status: {statuscode} : {reason}",
          (int)response.StatusCode,
          response.ReasonPhrase);
        result = null;
      }
      return result;
    }

    public async Task<ErsReferralResult> GetRegistration(
      ErsWorkListEntry ersWorkListEntry,
      long? attachmentId,
      long? overrideAttachmentId,
      ErsSession activeSession,
      bool showDiagnostics = false)
    {
      if (ersWorkListEntry is null)
      {
        throw new ArgumentNullException(nameof(ersWorkListEntry));
      }

      if (activeSession is null)
      {
        throw new ArgumentNullException(nameof(activeSession));
      }

      ErsReferral data;
      var client = GetClient();

      ErsReferralResult result = new ErsReferralResult();
      result.Ubrn = ersWorkListEntry.Ubrn;
      result.ServiceIdentifier = ersWorkListEntry.ServiceIdentifier;

      data = await GetErsReferralByUbrn(
        activeSession, 
        ersWorkListEntry.Ubrn, 
        ersWorkListEntry.NhsNumber);
      if (data == null)
      {
        result.Success = false;
        result.Errors.Add("Failed to get referral from eReferrals system. ");
      }
      else
      {
        if (data.Attachments.Count == 0)
        {
          _log.Debug("UBRN {ubrn}: Record contains no attachments.",
            ersWorkListEntry?.Ubrn);
          result.Success = true;
          result.Pdf = new ReferralAttachmentPdfProcessor(
            _log,
            _auditlog,
            ersWorkListEntry.ServiceIdentifier,
            _config.Data.ParserConfig,
            _config.Data.AnswerMap);
          result.Pdf.Source = SourceSystem.Unidentified;
          result.NoValidAttachmentFound = true;
        }
        else
        {
          if (showDiagnostics)
          {
            _log.Verbose("Attachments:");
            foreach (ErsAttachment attachment in data.Attachments)
            {
              string template = "{index} : Creation Date={creationDate}; " +
                "AttachmentId={attachmentId}; Filename='{fileName}'";
              _log.Verbose(template,
                data?.Attachments?.IndexOf(attachment),
                attachment?.Creation,
                attachment?.Id,
                attachment?.Title);
            }
          }
          //Handle overridden attachment downloads
          if (overrideAttachmentId != null && overrideAttachmentId != 0)
          {
            //Check the attachment exists
            ErsAttachment attachmentToLoad = null;
            result.AttachmentId = overrideAttachmentId;
            foreach (ErsAttachment attachment in data.Attachments)
            {
              if (Convert.ToInt32(attachment.Id) == overrideAttachmentId)
              {
                attachmentToLoad = attachment;

                break;
              }
            }

            if (attachmentToLoad == null)
            {
              result.Success = false;
              _log.Error("UBRN {ubrn}: Attachment Id {attachmentid} not found",
                data?.id,
                overrideAttachmentId);
            }
            else
            {
              result.Success = true;

              Thread.Sleep(
                  (int)_config.Data.MinimumAttachmentDownloadTimeSeconds
                  * 1000);

              //Download the attachment
              ReferralAttachmentPdfProcessor referralPdf =
                new ReferralAttachmentPdfProcessor(
                  _log,
                  _auditlog,
                  ersWorkListEntry.ServiceIdentifier,
                  _config.Data.ParserConfig,
                  _config.Data.AnswerMap);
              try
              {
                bool attachmentResult =
                  await referralPdf.DownloadFile(
                    _config.Data.AttachmentPath,
                    client,
                    attachmentToLoad,
                    _config.Data.InteropTemporaryFilePath,
                    ersWorkListEntry,
                    activeSession,
                    _config.Data.AccreditedSystemsID,
                    _config.Data.Fqdn,
                    _config.Data.ReformatDocument,
                    _config.Data.SectionHeadings,
                    _config.Data.RetryAllSources,
                    _config.Data.NumberOfMissingQuestionsTolerance);
              }
              catch (IncorrectFileTypeException ex)
              {
                Log.Warning(ex, ex.Message);
                result = new ErsReferralResult()
                {
                  AttachmentId = Convert.ToInt64(attachmentToLoad.Id),
                  Ubrn = ersWorkListEntry.Ubrn,
                  Success = false
                };
                result.Errors.Add(ex.Message);
                return result;
              }

              //If the attachment could not be identified, then log it and go
              //onto the next one
              if (referralPdf.Source == SourceSystem.Unidentified)
              {
                //create log entry
                _log.Warning("UBRN {ubrn}: The source system for " +
                  "attachment {attachmentid} could not be identified.",
                  referralPdf?.Ubrn,
                  attachmentToLoad?.Id);
                result = new ErsReferralResult()
                {
                  AttachmentId = Convert.ToInt64(attachmentToLoad.Id),
                  Pdf = referralPdf,
                  Ubrn = ersWorkListEntry.Ubrn,
                  Success = false
                };
                result.Errors.Add("Attachment was not identified as a " +
                  "referral document");
                return result;
              }
              else //Document was identified as a referral document
              {
                result = new ErsReferralResult()
                {
                  AttachmentId = Convert.ToInt64(attachmentToLoad.Id),
                  Pdf = referralPdf,
                  Ubrn = ersWorkListEntry.Ubrn,
                  Success = true
                };
                return result;
              }
            }
          }
          else
          {
            // Determine which attachment to get in the event there are 
            // more than one.
            // Attachment attachmentToLoad = null;
            // Pick the attachment with the highest Id
            long? mostRecentAttachmentId = null;
            bool hasAttachmentExportError = false;
            bool hasAttachmentInteropError = false;
            
            foreach (ErsAttachment attachmentToLoad in data.Attachments)
            {
              bool excluded = false;
              if (_config.Data.ExcludedFiles != null)
              {
                excluded = _config.Data.ExcludedFiles.Exists(
                  n => RegexUtilities.IsWildcardMatch(n, attachmentToLoad.Title));
              }
              if (excluded == true)
              {
                _log.Debug("UBRN {ubrn}: Attachment with id {attachmentid} " +
                  "was skipped as the filename '{filename}' is in the " +
                  "exclusion list.",
                  ersWorkListEntry?.Ubrn,
                  attachmentToLoad?.Id,
                  attachmentToLoad?.Title);
              }
              else
              {
                Console.WriteLine($"Processing attachment " +
                  $"'{attachmentToLoad.Title}'");
                long thisAttachmentId = Convert.ToInt64(attachmentToLoad.Id);

                if (mostRecentAttachmentId == null ||
                  thisAttachmentId > mostRecentAttachmentId)
                {
                  mostRecentAttachmentId = thisAttachmentId;
                }

                if (attachmentId != null)
                {
                  if (thisAttachmentId <= attachmentId)
                  {
                    //The attachment has already been processed so there is
                    //nothing to do
                    result.AttachmentId = thisAttachmentId;
                    result.Success = true;
                    result.MostRecentAttachmentId = mostRecentAttachmentId;
                    return result;
                  }
                }

                //There is a limit on the number of attachment download calls
                //so this is a delay built in and the time read from the
                //configuration.
                await Task.Delay(
                  (int)(_config.Data.MinimumAttachmentDownloadTimeSeconds
                  * 1000));

                //Download the attachment
                ReferralAttachmentPdfProcessor referralPdf =
                  new ReferralAttachmentPdfProcessor(
                    _log,
                    _auditlog,
                    ersWorkListEntry.ServiceIdentifier,
                    _config.Data.ParserConfig,
                    _config.Data.AnswerMap);
                bool attachmentResult =
                  await referralPdf.DownloadFile(
                    _config.Data.AttachmentPath,
                    client,
                    attachmentToLoad,
                    _config.Data.InteropTemporaryFilePath,
                    ersWorkListEntry,
                    activeSession,
                    _config.Data.AccreditedSystemsID,
                    _config.Data.Fqdn,
                    _config.Data.ReformatDocument,
                    _config.Data.SectionHeadings,
                    _config.Data.RetryAllSources,
                    _config.Data.NumberOfMissingQuestionsTolerance
                    );

                //If the attachment could not be identified, then log it and go
                //onto the next one
                if (referralPdf.ExportFailed)
                {
                  //create log entry
                  _log.Debug("UBRN {ubrn}: Failed to export attachment " +
                    "'{filename}'", 
                    referralPdf?.Ubrn,
                    attachmentToLoad?.Title);
                  hasAttachmentExportError = true;
                }
                else if (referralPdf.InteropFailed)
                {
                  _log.Debug("Interop failed to process attachment " +
                    $"'{attachmentToLoad.Title}'");
                  hasAttachmentInteropError = true;
                }
                else if (referralPdf.Source == SourceSystem.Unidentified)
                {
                  //create log entry
                  _log.Debug("UBRN {ubrn}: Ignored attachment {attachmentId} " +
                    "as the source system could not be identified.",
                    referralPdf?.Ubrn,
                    thisAttachmentId);
                }
                else //Document was identified as a referral document
                {
                  result = new ErsReferralResult()
                  {
                    AttachmentId = Convert.ToInt64(attachmentToLoad.Id),
                    MostRecentAttachmentId = mostRecentAttachmentId,
                    Pdf = referralPdf,
                    Ubrn = ersWorkListEntry.Ubrn,
                    Success = true
                  };
                  return result;
                }
              }
            }

            if (!result.Success)
            {
              if (hasAttachmentExportError || hasAttachmentInteropError)
              {
                result.NoValidAttachmentFound = false;
                result.AttachmentId = 0;
                result.ExportErrors = hasAttachmentExportError;
                result.InteropErrors = hasAttachmentInteropError;
              }
              else if (data.Attachments.Count > 0)
              {
                result.NoValidAttachmentFound = true;
                result.AttachmentId = 0;
                result.ExportErrors = hasAttachmentExportError;
              }
            }          }
        }
      }

      return result;
    }

    /// <summary>
    /// Retrieves worklist(s) for all services listed in the config and
    /// combines them into one list.
    /// </summary>
    /// <param name="activeSession">Current active Ers Session</param>
    /// <returns>List of eRS headers for all worklists</returns>
    public async Task<WorkListResult> GetWorkListFromErs(
      ErsSession activeSession)
    {
      //A008 query
      const string A008REQUESTBODYPATH = @".\Files\A008RequestBody.txt";

      WorkListResult result = new();
      result.Success = true;
      result.WorkList = new();

      List<ErsWorkListEntry> finalWorkList = new();
      foreach (string serviceId in _config.Data.ServiceIdentifiers)
      {
        _log.Debug("ServiceId {serviceid}: Retrieving work list",
          serviceId);
        //Load request body from file.
        string requestBody = File.ReadAllText(A008REQUESTBODYPATH);

        requestBody = requestBody.Replace(
          "#SERVICEIDENTIFIER", serviceId);
        using (var client = GetFhirJsonClient())
        {
          var request = new HttpRequestMessage(
              HttpMethod.Post,
              _config.Data.RetrieveWorklistPath)
          {
            Content = new StringContent(requestBody, Encoding.UTF8,
              "application/fhir+json")
          };

          string msgTemplate = 
            "EventType=API Retrieve Request List;" +
            "ActionType=POST;" +
            "ResourceType=Worklist;" +
            "UUID ={uuid};" +
            "SessionID={sessionId};" +
            "OrgName={orgname};" +
            "BusinessFunction={businessFunction};" +
            "ASID={asid};" +
            "FQDN={fqdn};" +
            "ApiMethod={apiMethod};";

          _log.Debug($"A008:Downloading work list for Service {serviceId}");
          _auditlog.Information(msgTemplate,
            activeSession?.User?.Identifier ?? "Unknown",
            activeSession?.Id ?? "Unknown",
            activeSession?.Permission?.OrgName ?? "Unknown",
            activeSession?.Permission?.BusinessFunction ?? "Unknown",
            _config.Data.AccreditedSystemsID,
            _config.Data.Fqdn,
            "A008-Start");

          HttpResponseMessage httpResponse = await client.SendAsync(request);
          if (httpResponse.IsSuccessStatusCode)
          {
            //TODO: Use a proper HL7+fhir object for this
            string workListJson = await httpResponse.Content.ReadAsStringAsync();
            ErsWorkList workList =
              JsonConvert.DeserializeObject<ErsWorkList>(workListJson);

            foreach (ErsWorkListEntry entry in workList.Entry)
            {
              entry.ServiceIdentifier = serviceId;
            }
            _log.Debug($"Retrieved {workList.Entry?.Length} records.");
            finalWorkList.AddRange(workList.Entry);

            _auditlog.Information(msgTemplate,
              activeSession?.User?.Identifier ?? "Unknown",
              activeSession?.Id ?? "Unknown",
              activeSession?.Permission?.OrgName ?? "Unknown",
              activeSession?.Permission?.BusinessFunction ?? "Unknown",
              _config.Data.AccreditedSystemsID,
              _config.Data.Fqdn,
              "A008-End");
          }
          else
          {
            result.Success = false;
            //If one of the lists fails to download, we don't want the process
            //to continue as it may create the impression that some referrals
            //are no longer registered on e-RS.
            _log.Fatal("ServiceId {serviceid}: Failed to get referral list " +
                "from WMS.  Status: {statuscode} : {reason}",
              serviceId,
              (int)httpResponse.StatusCode,
              httpResponse.ReasonPhrase); 
            break;
          }
        }
      } //Service identifiers
      if (result.Success)
      {
        result.WorkList.Entry = finalWorkList.ToArray();
      }
      return result;
    }

    public HttpClient GetClient()
    {
      HttpClient client = new HttpClient(GetHandler())
      {
        BaseAddress = new Uri(_config.Data.BaseUrl)
      };
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders
        .Add("XAPI_ASID", _config.Data.AccreditedSystemsID);
      client.DefaultRequestHeaders
        .Add("HTTP_X_SESSION_KEY", _auth.ActiveSession.Id);
      client.Timeout =
        TimeSpan.FromSeconds(_config.Data.TimeoutAttachmentDownloadTimeSeconds);
      if (client.Timeout.TotalSeconds < 100 ||
        client.Timeout.TotalSeconds > 300)
      {
        _log.Debug("Timeout has been set to {timeout} seconds",
          client.Timeout.TotalSeconds);
      }
      return client;
    }

    public HttpClient GetFhirJsonClient()
    {
      HttpClient client = GetClient();
      client.DefaultRequestHeaders.Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/fhir+json"));

      return client;
    }

    public FhirClient GetFhirClient()
    {
      FhirClientSettings settings = new FhirClientSettings()
      {
        PreferredReturn = Prefer.ReturnRepresentation,
        PreferredFormat = ResourceFormat.Json,
      };

      FhirClient client = new FhirClient(
        _config.Data.BaseUrl,
        settings,
        GetHandler());

      client.RequestHeaders.Accept.Clear();
      client.RequestHeaders
        .Add("XAPI_ASID", _config.Data.AccreditedSystemsID);
      client.RequestHeaders
        .Add("HTTP_X_SESSION_KEY", _auth.ActiveSession.Id);

      return client;
    }

    private HttpClientHandler GetHandler()
    {
      HttpClientHandler handler = new HttpClientHandler
      {
        SslProtocols = SslProtocols.Tls12
      };
      X509Certificate2 cert = Certificates.LoadCertificateFromFile(
        _config.Data.ClientCertificateFilePath,
        _config.Data.ClientCertificatePassword);

      handler.ClientCertificates.Add(cert);
      handler.ClientCertificateOptions = ClientCertificateOption.Manual;

      // this is required because the server ceritifcate from eReferrals is
      // not in a valid chain -- needs further investigation
      handler.ServerCertificateCustomValidationCallback = (a, b, c, d) =>
      { return true; };

      return handler;
    }

    public async Task<GetCriResult> GetCriDocument(
      string ubrn,
      string nhsNumber,
      ErsSession activeSession)
    {
      GetCriResult result = new GetCriResult();

      //There is a limit on the number of attachment download calls
      //so this is a delay built in and the time read from the
      //configuration.
      await Task.Delay(
        (int)(_config.Data.MinimumAttachmentDownloadTimeSeconds * 1000));
      using (var client = GetClient())
      {
        string path = string.Format(
          _config.Data.RetrieveClinicalInformationPath, ubrn);
        var request = new HttpRequestMessage(HttpMethod.Post, path);

        string msgTemplate = 
          "EventType=API Retrieve Clinical Information;" +
          "ActionType=POST;" +
          "ResourceType=Appointment request;" +
          "UUID={uuid};" +
          "NHSNumber={nhsNumber};" +
          "UBRN={ubrn};" +
          "SessionID={sessionId};" +
          "OrgName={orgname};" +
          "BusinessFunction={businessFunction};" +
          "ASID={asid};" +
          "FQDN={fqdn};" +
          "ApiMethod={apiMethod};";

        _log.Debug($"A007:Downloading eRS record for UBRN {ubrn}");
        _auditlog.Information(msgTemplate,
          activeSession?.User?.Identifier ?? "Unknown",
          nhsNumber,
          ubrn,
          activeSession?.Id ?? "Unknown",
          activeSession?.Permission?.OrgName ?? "Unknown",
          activeSession?.Permission?.BusinessFunction ?? "Unknown",
          _config.Data.AccreditedSystemsID,
          _config.Data.Fqdn,
          "A007-Start");

        HttpResponseMessage httpResponse = await client.SendAsync(request);

        if (httpResponse.IsSuccessStatusCode)
        {
          byte[] returnValue =
            await httpResponse.Content.ReadAsByteArrayAsync();
          result.Success = true;
          result.CriDocument = returnValue;
          result.NoCriDocumentFound = false;

          _auditlog.Information(msgTemplate,
           activeSession?.User?.Identifier ?? "Unknown",
           nhsNumber,
           ubrn,
           activeSession?.Id ?? "Unknown",
           activeSession?.Permission?.OrgName ?? "Unknown",
           activeSession?.Permission?.BusinessFunction ?? "Unknown",
           _config.Data.AccreditedSystemsID,
           _config.Data.Fqdn,
           "A007-End");
        }
        else
        {
          if (httpResponse.StatusCode == HttpStatusCode.NotFound)
          {
            result.Success = true;
            result.NoCriDocumentFound = true;
            _log.Debug("UBRN {ubrn}: CRI Document does not exist on eRS.",
              ubrn);
          }
          else
          {
            result.Success = false;
            result.NoCriDocumentFound = false;
            _log.Warning("UBRN {ubrn}: Failed to get CRI Document.  " +
                "Status: {status} : {reason}",
              ubrn,
              (int)httpResponse.StatusCode,
              httpResponse.ReasonPhrase);
          }
        }
      }

      return result;
    }

    public async Task<ErsReferral> GetErsReferralByUbrn(
      ErsSession session,
      string ubrn,
      string nhsNumber)
    {
      ErsReferral result = null;
      if (string.IsNullOrWhiteSpace(nhsNumber))
      {
        nhsNumber = "Unknown";
      }

      //Get the referral request from the eReferrals system
      using var client = GetClient();
      var request = new HttpRequestMessage(
          HttpMethod.Get,
          $"{_config.Data.RegistrationPath}{ubrn}");

      string msgTemplate = 
        "EventType=API Retrieve Request Summary;" +
        "ActionType=GET;" +
        "ResourceType=Referral Request;" +
        "UUID={uuid};" +
        "NHSNumber={nhsNumber};" +
        "UBRN={ubrn};" +
        "SessionID={sessionId};OrgName={orgname};" +
        "BusinessFunction={businessFunction};" +
        "FileName={fileName};" +
        "ASID={asid};" +
        "FQDN={fqdn};" +
        "ApiMethod={apiMethod};";

      _log.Debug($"A005:Downloading request summary for UBRN {ubrn}");
      _auditlog.Information(msgTemplate,
        session?.User?.Identifier ?? "Unknown",
        nhsNumber,
        ubrn,
        session?.Id ?? "Unknown",
        session?.Permission?.OrgName ?? "Unknown",
        session?.Permission?.BusinessFunction ?? "Unknown",
        "Unknown", // There is no file name at this stage
        _config.Data.AccreditedSystemsID,
        _config.Data.Fqdn,
        "A005-Start");

      HttpResponseMessage httpResponse = await client.SendAsync(request);
      if (httpResponse.IsSuccessStatusCode)
      {
        string referralRequest = await httpResponse.Content.ReadAsStringAsync();
        result = JsonConvert.DeserializeObject<ErsReferral>(referralRequest);
        result.Finalise(_config.Data.SupportedAttachmentFileTypes);

        _auditlog.Information(msgTemplate,
          session?.User?.Identifier ?? "Unknown",
          nhsNumber,
          ubrn,
          session?.Id ?? "Unknown",
          session?.Permission?.OrgName ?? "Unknown",
          session?.Permission?.BusinessFunction ?? "Unknown",
          "Unknown", //There is no file name at this stage
          _config.Data.AccreditedSystemsID,
          _config.Data.Fqdn,
          "A005-End");
        _log.Debug("UBRN {ubrn}: Record retrieved from eRS",
          ubrn);
      }
      else //http response returned invalid code
      {
        result = null;
        _log.Warning("UBRN {ubrn}: Failed to get record from eRS.  " +
            "Status: {status} : {reason}",
          ubrn,
          (int)httpResponse.StatusCode,
          httpResponse.ReasonPhrase);
      }
      return result;

    }

    public async Task<ReviewCommentResult> RecordOutcome(
      ErsSession session,
      string ubrn,
      string nhsNumber,
      Outcome outcome,
      string comment)
    {
      ErsReferral ersReferral = await GetErsReferralByUbrn(
        session, 
        ubrn, 
        nhsNumber);

      if (ersReferral == null)
      {
        _log.Error("UBRN {ubrn}: Cannot record outcome as the referral" +
          " record could not be retrieved from eRS.",
          ubrn);
        return new ReviewCommentResult()
        {
          Success = false
        };
      }
      else
      {
        return await 
          RecordOutcome(session, ersReferral, nhsNumber, outcome, comment);
      }
    }

    public async Task<ReviewCommentResult> RecordOutcome(
      ErsSession session,
      ErsReferral ersReferral,
      string nhsNumber,
      Outcome outcome,
      string comment)
    {
      ReviewCommentResult result = new ReviewCommentResult()
      {
        Success = true
      };

      string version = $"W/\"{ersReferral.meta.versionId}\"";

      const string A028REQUESTBODYPATH = @".\Files\A028RequestBody.txt";

      //Load request body from file.
      string requestBody = File.ReadAllText(A028REQUESTBODYPATH);

      requestBody = requestBody.Replace(
        "#REVIEW_COMMENT", comment);
      requestBody = requestBody.Replace(
        "#REVIEW_OUTCOME", $"{outcome}");

      using (var client = GetFhirJsonClient())
      {
        client.DefaultRequestHeaders
          .Add("If-Match", version);
        string url = string.Format(_config.Data.RecordReviewOutcomePath,
          ersReferral.id);
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            url)
        {
          Content = new StringContent(requestBody, Encoding.UTF8,
            "application/fhir+json")
        };

        string msgTemplate = 
          "EventType=Record Review Outcome;" +
         "ActionType=POST;" +
         "ResourceType=Reject Referral Request;" +
         "UUID={uuid};" +
         "NHSNumber={nhsNumber};" +
         "UBRN={ubrn};" +
         "SessionID={sessionId};" +
         "OrgName={orgname};" +
         "BusinessFunction={businessFunction};" +
         "ASID={asid};" +
         "FQDN={fqdn};" +
         "ApiMethod={apiMethod};";

        _log.Debug($"A028:Recording Outcome for UBRN {ersReferral.id}");
        _auditlog.Information(msgTemplate,
          session?.User?.Identifier ?? "Unknown",
          nhsNumber ?? "Unknown",
          ersReferral.id,
          session?.Id ?? "Unknown",
          session?.Permission?.OrgName ?? "Unknown",
          session?.Permission?.BusinessFunction ?? "Unknown",
          _config.Data.AccreditedSystemsID,
          _config.Data.Fqdn,
          "A028-Start");

        HttpResponseMessage httpResponse = await client.SendAsync(request);

        if (httpResponse.IsSuccessStatusCode)
        {
          _auditlog.Information(msgTemplate,
          session?.User?.Identifier ?? "Unknown",
          nhsNumber ?? "Unknown",
          ersReferral.id,
          session?.Id ?? "Unknown",
          session?.Permission?.OrgName ?? "Unknown",
          session?.Permission?.BusinessFunction ?? "Unknown",
          _config.Data.AccreditedSystemsID,
          _config.Data.Fqdn,
          "A028-End");
        }
        else //http response returned invalid code
        {
          _log.Warning("UBRN {ubrn}: Failed to set review comment on eRS.  " +
            "Status: {statuscode} : {reason}",
            ersReferral?.id,
            (int)httpResponse.StatusCode,
            httpResponse.ReasonPhrase);
          result.Success = false;
        }
        return result;
      }
    }

    public async Task<GetDischargeListResult> GetDischarges()
    {
      GetDischargeListResult result = new GetDischargeListResult();

      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
          .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
        HttpResponseMessage response = await client.GetAsync("Discharge");

        if (response.IsSuccessStatusCode)
        {
          if (response.StatusCode == HttpStatusCode.NoContent)
          {
            result.DischargeList = null;
          }
          else
          {
            List<GetDischargeUbrnResponse> ubrnList = await response
              .Content.ReadFromJsonAsync<List<GetDischargeUbrnResponse>>();

            result.DischargeList = ubrnList;
          }
          result.Success = true;
        }
        else
        {
          result.Success = false;
          _log.Warning("Failed to get the discharge list from WMS.  " +
            "Status: {statuscode} : {reason}",
            (int)response.StatusCode,
            response.ReasonPhrase);
        }
      }
      return result;
    }

    public async Task<bool> CompleteDischarge(Guid id)
    {
      bool result;
      using (var client = new HttpClient())
      {
        client.BaseAddress = new Uri(_config.Data.HubRegistrationAPIPath);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders
          .Add("X-API-KEY", _config.Data.HubRegistrationAPIKey);
        HttpResponseMessage response = await client.PatchAsync($"{id}", null);

        if (response.IsSuccessStatusCode)
        {
          _log.Debug("Discharge Completed on WMS.");
          result = true;
        }
        else
        {
          result = false;
          _log.Warning("Id {id}: Failed to Complete Discharge on WMS.  " +
              "Status: {statuscode} : {reason}",
            id,
            (int)response.StatusCode,
            response.ReasonPhrase);
        }
      }
      return result;
    }

    public async Task<AvailableActionResult> GetAvailableActions(
      ErsSession activeSession,
      ErsReferral ersReferral,
      string nhsNumber)
    {
      AvailableActionResult result = new();


      result.Success = true;
      result.Actions = new();

      using (var client = GetFhirJsonClient())
      {
        string uri = string.Format(_config.Data.AvailableActionsPath,
          ersReferral.id, ersReferral.meta.versionId);

        string msgTemplate = 
          "EventType=Available Actions For User List;" +
          "ActionType=GET;" +
          "ResourceType=AvailableActions;" +
          "UUID={uuid};" +
          "SessionID={sessionId};" +
          "UBRN={ubrn};" +
          "NHSNumber={nhsNumber};" +
          "OrgName={orgname};" +
          "BusinessFunction={businessFunction};" +
          "ASID={asid};" +
          "FQDN={fqdn};" +
          "ApiMethod={apiMethod};";

        _log.Debug($"A029:Getting Available Actions for UBRN {ersReferral.id}");
        _auditlog.Information(msgTemplate,
          activeSession?.User?.Identifier ?? "Unknown",
          activeSession?.Id ?? "Unknown",
          ersReferral.id,
          nhsNumber,
          activeSession?.Permission?.OrgName ?? "Unknown",
          activeSession?.Permission?.BusinessFunction ?? "Unknown",
          _config.Data.AccreditedSystemsID,
          _config.Data.Fqdn,
          "A029-Start");

        HttpResponseMessage httpResponse = await client.GetAsync(uri);

        _auditlog.Information(msgTemplate,
          activeSession?.User?.Identifier ?? "Unknown",
          activeSession?.Id ?? "Unknown",
          ersReferral.id,
          nhsNumber,
          activeSession?.Permission?.OrgName ?? "Unknown",
          activeSession?.Permission?.BusinessFunction ?? "Unknown",
          _config.Data.AccreditedSystemsID,
          _config.Data.Fqdn,
          "A029-End");

        if (httpResponse.IsSuccessStatusCode)
        {
          string actionsJson = await httpResponse.Content.ReadAsStringAsync();
          result.Actions =
            JsonConvert.DeserializeObject<AvailableActions>(actionsJson);
          _log.Debug("UBRN {ubrn}: Available Actions retrieved from eRS",
            ersReferral?.id);
        }
        else
        {
          //If one of the lists fails to download, we don't want the process
          //to continue as it may create the impression that some referrals
          //are no longer registered on e-RS.
          _log.Warning("UBRN {ubrn}: Failed to get available actions from" +
              "eRS.  Status: {statuscode} : {reason}",
            ersReferral?.id,
            (int)httpResponse.StatusCode,
            httpResponse.ReasonPhrase);
          result.Success = false;
        }
      }

      return result;
    }

  }
}

