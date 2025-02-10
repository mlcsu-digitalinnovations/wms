using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Exceptions;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.ReferralsService.Models.Results;
using WmsHub.ReferralsService.Pdf;
using static WmsHub.ReferralsService.Enums;
using WmsHub.Common.Enums;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.ReferralsService;

public class ReferralProcessor
{
  private readonly IReferralsDataProvider _dataProvider;
  private readonly ISmartCardAuthentictor _smartCardAuthentictor;
  private readonly Config _config;
  private readonly ILogger _log;
  private readonly ILogger _auditLog;

  const string DEFAULTCONFIGPATH = "./appsettings.json";

  public ReferralProcessor(
    IReferralsDataProvider dataProvider,
    ISmartCardAuthentictor smartCardAuthentictor,
    Config configuration,
    ILogger log,
    ILogger auditLog
    )
  {
    _dataProvider = dataProvider;
    _smartCardAuthentictor = smartCardAuthentictor;
    _auditLog = auditLog;
    _log = log.ForContext<ReferralProcessor>();
    if (_log == null)
    {
      // Moq and mocking in general doesn't work well with serilog
      // static methods like `ForContext<Referral>`
      _log = log;
    }

    if (configuration == null)
    {
      _config = JsonConvert.DeserializeObject<Config>(
      File.ReadAllText(DEFAULTCONFIGPATH));
    }
    else
    {
      _config = configuration;

      if (_config.Data.ExcludedFiles == null)
      {
        _log.Information("No filename exclusions have been declared. " +
          "To exclude filenames with a wild card, you can define these" +
          "in the ExcludedFiles list.");
      }
      ErsReferral.FileNamesToExclude = _config.Data.ExcludedFiles;
      _log.Information("{total} filename exclusions have been set.",
        ErsReferral.FileNamesToExclude.Count);

    }
  }

  /// <summary>
  /// Runs the main process of downloading a worklist and creating
  /// new referral records onto the hub
  /// </summary>
  /// <param name="reportOnly">When TRUE, the referrals are not    
  /// processed, but a diagnotic report is produced instead.</param>
  /// <param name="reprocessUnchangedAttachment">When TRUE, existing
  /// attachments are re-processed</param> 
  /// <returns>Returns TRUE when successful</returns>
  public async Task<ProcessExecutionResult> Process(
    bool reportOnly = false,
    string ubrn = "",
    string attachmentId = null,
    bool reprocessUnchangedAttachment = false)
  {
    ProcessExecutionResult result = new()
    {
      Completed = false,
      Success = true
    };
    bool processSingleUbrn = false;
    string ubrnToProcess = "";

    if (!string.IsNullOrWhiteSpace(ubrn))
    {
      processSingleUbrn = true;
      ubrnToProcess = ubrn.Trim();
    }
    try
    {
      bool showDiagnostics = false;
      if (reportOnly == true && ubrn != "")
      {
        showDiagnostics = true;
      }

      if (reportOnly)
      {
        string reportOnlyMessage = "Running process in Report Only mode.";
        if (!string.IsNullOrWhiteSpace(ubrn))
        {
          reportOnlyMessage += $"  UBRN = {ubrn}.";
          if (attachmentId != null)
          {
            reportOnlyMessage += $"  AttachmentId = {attachmentId}.";
          }
        }
        _log.Debug(reportOnlyMessage);
      }
      //This will read the smart card and authenticate a session
      bool connect = await _smartCardAuthentictor.CreateSession();

      if (!_smartCardAuthentictor.ActiveSession.SmartCardIsAuthenticated)
      {
        _log.Fatal("Smart card was not authenticated.  Cannot continue.");
        result.Completed = false;
        result.Success = false;
        return result;
      }
      if (!_smartCardAuthentictor.ActiveSession.IsAuthenticated)
      {
        _log.Fatal("Session was not authenticated.  Cannot continue.");
        result.Completed = false;
        result.Success = false;
        return result;
      }

      ErsWorkList currentWorkList;

      //Get list from eRS
      WorkListResult workList = await _dataProvider.GetWorkListFromErs(
        _smartCardAuthentictor.ActiveSession);
      if (workList.Success == false)
      {
        _log.Error(workList.AggregateErrors);
        await _smartCardAuthentictor.TerminateSession();
        result.Completed = false;
        result.Success = false;
        return result;

      }
      currentWorkList = workList.WorkList;

      //Get worklist from Hub
      RegistrationListResult registrationList
        = await _dataProvider.GetReferralList(
          _config.Data.SendServiceIdToHubForReferralList);

      if (registrationList.Success == false)
      {
        _log.Error(registrationList.AggregateErrors);
        await _smartCardAuthentictor.TerminateSession();
        result.Completed = false;
        result.Success = false;
        return result;

      }
      RegistrationList registeredItems = registrationList.ReferralUbrnList;

      //Brings new referrals to the top of the list
      PrioritiseWorklistItems(currentWorkList, registeredItems);

      await ProcessRejectReferralAsync(
        registeredItems.Ubrns,
        currentWorkList.Entry,
        reportOnly);

      await ProcessRemovedFromWorklistWhilstInProgressAsync(
        registeredItems.Ubrns,
        currentWorkList.Entry,
        reportOnly);

      //iterate through all referrals
      if (processSingleUbrn == true)
      {
        _log.Debug("Processing UBRN {ubrn}", ubrnToProcess);
      }
      else
      {
        Log.Debug("Processing {total} items", currentWorkList.Entry.Length);
      }
      int counter = 0;
      foreach (ErsWorkListEntry workListEntry in currentWorkList.Entry)
      {
        if (processSingleUbrn == false)
        {
          counter++;
          Log.Debug("Processing Item {counter} of {total} : UBRN {ubrn}",
            counter, currentWorkList.Entry.Length, workListEntry.Item.Id);
        }
        if (processSingleUbrn == false ||
          (processSingleUbrn == true &&
          workListEntry.Item.Id == ubrnToProcess))
        {
          bool recordResult = true;
          try
          {
            //Note- The logic has been moved outside of the scope of this 
            //project. The only absolutely required field is the UBRN number.

            //Is referral from eRS new? (2)
            GetActiveUbrnResponse record =
              registeredItems.FindByUbrn(workListEntry.Item.Id);
            if (record == null)
            {
              //If it is, Send to Hub API


              recordResult =
              await ProcessNewReferral(workListEntry, reportOnly,
                showDiagnostics,
                attachmentId);

              if (recordResult == false)
              {
                result.Success = false;
              }
            }
            else
            {
              if (reportOnly == true && string.IsNullOrWhiteSpace(
                ubrnToProcess) == false)
              {
                string reportText = $"Reporting on {workListEntry.Item.Id}";
                if (attachmentId != null)
                {
                  reportText += $" with attachment Id {attachmentId}";
                }
                _log.Debug(reportText);
                bool referralProcessedSuccessfully = await ProcessExistingReferral(
                  record,
                  workListEntry,
                  reportOnly,
                  showDiagnostics,
                  attachmentId,
                  reprocessUnchangedAttachment);

                if (!referralProcessedSuccessfully)
                {
                  result.Success = false;
                }
              }
              else
              {
                switch (record.ErsReferralStatus)
                {
                  case ErsReferralStatus.InProgress:

                    _log.Debug(
                      "Status is InProgress: closing item {id}.",
                      workListEntry.Item.Id);

                    await RecordReviewOutcomeAndSetErsClosedAsync(
                      nhsNumber: workListEntry.NhsNumber,
                      outcome: Outcome.RETURN_TO_REFERRER_WITH_ADVICE,
                      reportOnly: reportOnly,
                      ubrn: record.Ubrn);

                    break;

                  case ErsReferralStatus.AwaitingUpdate:

                    _log.Debug(
                      "Status is AwaitingUpdate: updating item {id} " +
                        "with attachment Id {att}.",
                      workListEntry.Item.Id,
                      record.ReferralAttachmentId);

                    bool referralProcessedSuccessfully = await ProcessExistingReferral(
                      record,
                      workListEntry,
                      reportOnly,
                      showDiagnostics,
                      attachmentId,
                      reprocessUnchangedAttachment);

                    if (!referralProcessedSuccessfully)
                    {
                      result.Success = false;
                    }

                    break;

                  case ErsReferralStatus.OnHold:
                    _log.Debug(
                      "Status is OnHold: skipping item {id}.",
                      workListEntry.Item.Id);
                    break;

                  default:
                    _log.Warning(
                      "Status is {status}: unable to handle item {id}.",
                      record.Status,
                      workListEntry.Item.Id);
                    break;
                }
                //Check to see if a CRI document exists and create one if
                //it does not.  This will happen if a referral record
                //was originally processed as a batch
                if (!record.Status.Equals(
                  "ONHOLD",
                  StringComparison.InvariantCultureIgnoreCase))
                {
                  bool criProcessed = await CreateOrUpdateCriDocument(
                    record, workListEntry, reportOnly);
                }
              }
            }
          }
          catch (Exception ex)
          {
            _log.Error(ex, ex.Message);
            result.Success = false;
          }
        }
      }
      result.Completed = true;
    }
    catch (Exception ex)
    {
      _log.Error(ex, "Check full log to ensure process completed " +
        "successfully.");
      result.Success = false;
    }
    finally
    {
      await _smartCardAuthentictor.TerminateSession();
    }

    return result;
  }

  /// <summary>
  /// This will process a batch of eReferrals from a constructed list of
  /// Ubrn/Filename/ServiceId entries from a CSV file
  /// </summary>
  /// <param name="batch">The pre-loaded batch object</param>
  /// <returns>True if successful, False if failed</returns>
  public async Task<ProcessExecutionResult> Process(
    Batch batch,
    bool reportOnly)
  {
    ProcessExecutionResult result = new ProcessExecutionResult()
    {
      Success = true
    };
    if (batch == null)
    {
      throw new ArgumentNullException("Batch was null.");
    }
    if (batch.Items == null)
    {
      throw new ArgumentNullException("Batch contained no items.");
    }
    if (batch.Items.Count == 0)
    {
      throw new InvalidDataException("Batch was empty.");
    }
    int itemNumber = 1;
    int numberSuccessful = 0;
    _log.Debug("Processing {total} items in batch.", batch.Items.Count);
    foreach (BatchItem item in batch.Items)
    {
      _log.Debug("Processing item {itemNumber} for SericeId " +
        "'{serviceid}': UBRN {ubrn} with Attachment Id {attachmentid}",
        itemNumber, item.ServiceId, item.Ubrn, item.AttachmentId);
      if (string.IsNullOrWhiteSpace(item.AttachmentFileName))
      {
        _log.Debug("Item {itemnumber} has no filename", itemNumber);
      }
      else if (string.IsNullOrWhiteSpace(item.Ubrn))
      {
        _log.Debug("Item {itemnumber} has no UBRN", itemNumber);
      }
      else
      {
        ReferralAttachmentPdfProcessor pdf =
          new ReferralAttachmentPdfProcessor(
            _log,
            _auditLog,
            item.ServiceId,
            _config.Data.ParserConfig,
            _config.Data.AnswerMap);
        bool success = await pdf.DownloadFile(item.AttachmentFileName,
          _config.Data.InteropTemporaryFilePath,
          _config.Data.ReformatDocument,
          _config.Data.SectionHeadings,
          _config.Data.RetryAllSources,
          _config.Data.NumberOfMissingQuestionsTolerance);
        if (reportOnly == true)
        {
          if (item.AlreadyExists == true)
          {
            _log.Debug("Proccessing Update Referral in Report Mode");
            pdf.GenerateReferralUpdateObject(
                    item.Ubrn, item.AttachmentId, null);
          }
          else
          {
            _log.Debug("Proccessing New Referral in Report Mode");
            pdf.GenerateReferralCreationObject(
                    item.Ubrn, item.AttachmentId, null);
          }
          numberSuccessful++;
        }
        else
        {
          if (item.AlreadyExists == false)
          {
            CreateReferralResult createResult = null;
            if (success == false)
            {
              _log.Debug("New Referral for item {itemnumber} could not " +
                $"be loaded.", itemNumber);
            }
            else
            {
              createResult = await _dataProvider.CreateReferral(
                  pdf.GenerateReferralCreationObject(
                    item.Ubrn, item.AttachmentId, null));

            }
            if (createResult != null)
            {
              if (createResult.Success == true)
                _log.Debug("{ubrn} Successfully Created.", item.Ubrn);
              numberSuccessful++;
            }
            else
            {
              _log.Debug("{ubrn} Failed to be Created.", item.Ubrn);
            }
          }
          else
          {
            UpdateReferralResult updateResult = null;
            if (success == false)
            {
              _log.Debug("Existing Referral for item {itemnumber} " +
                "could not be loaded.", itemNumber);
            }
            else
            {
              updateResult = await _dataProvider.UpdateReferral(
                pdf.GenerateReferralUpdateObject(
                    item.Ubrn, item.AttachmentId, null), item.Ubrn);
            }
            if (updateResult != null)
            {
              if (updateResult.Success == true)
              {
                _log.Debug("{ubrn} Successfully Updated.", item.Ubrn);
                numberSuccessful++;
              }
              else
              {
                _log.Debug("{ubrn} Failed to Update.", item.Ubrn);
              }
            }
          }
        }
      }
      itemNumber++;
    }
    result.Completed = true;
    _log.Debug("Succeeded processing {successful} of {total} Items.",
      numberSuccessful, batch.Items.Count);

    if (numberSuccessful == 0)
      result.Success = false;

    return result;
  }

  /// <summary>
  /// Process a Ubrn which has been cancelled
  /// </summary>
  /// <param name="registeredUbrns">Active UBrns from the Wms system</param>
  /// <param name="ersEntries">Entries from the eReferrals system</param>
  /// <returns>The number of records processed</returns>
  protected virtual async Task<int> ProcessRejectReferralAsync(
    List<GetActiveUbrnResponse> registeredUbrns,
    ErsWorkListEntry[] ersEntries,
    bool reportOnly)
  {
    if (registeredUbrns == null
      || ersEntries == null
      || !registeredUbrns.Any()       
      || !ersEntries.Any())
    {
      return 0;
    }

    List<string> registeredUbrnList = registeredUbrns
      .Where(s => s.IsAwaitingUpdate)
      .Select(t => t.Ubrn).ToList();

    List<string> ersUbrnList = ersEntries
      .Select(t => t.Item.Id)
      .ToList();

    IEnumerable<string> ubrnsToRemove = registeredUbrnList
      .Except(ersUbrnList);

    if (!ubrnsToRemove.Any())
    {
      return 0;
    }

    int recordsProcessed = 0;

    foreach (string ubrn in ubrnsToRemove)
    {
      if (reportOnly == true)
      {
        _log.Debug("Item {ubrn} is flagged for removal.", ubrn);
      }
      else
      {
        _log.Debug("Removing item {ubrn}", ubrn);

        UpdateReferralResult result =
          await _dataProvider.UpdateReferralCancelledByEReferral(ubrn);

        if (result.Success)
        {
          recordsProcessed++;
          continue;
        }

        _log.Error(result.AggregateErrors);
      }
    }

    if (recordsProcessed != ubrnsToRemove.Count())
    {
      Log.Error("Records Removed: {recordsProcessed}, " +
        "Failed to remove: {failed}",
        recordsProcessed, ubrnsToRemove.Count() - recordsProcessed);
    }

    return recordsProcessed;
  }

  /// <summary>
  /// Process referals that are InProgress but have been removed from the eRS
  /// worklist.
  /// </summary>
  /// <param name="registeredUbrns">Active UBrns from the Wms system.</param>
  /// <param name="ersEntries">Entries from the eReferrals system.</param>
  /// <returns>The number of records processed.</returns>
  protected async Task<int> ProcessRemovedFromWorklistWhilstInProgressAsync(
    List<GetActiveUbrnResponse> registeredUbrns,
    ErsWorkListEntry[] ersEntries,
    bool reportOnly)
  {
    if (registeredUbrns == null
      || ersEntries == null
      || !registeredUbrns.Any())
    {
      return 0;
    }

    List<string> inProgressUbrns = registeredUbrns
      .Where(s => s.IsInProgress)
      .Select(t => t.Ubrn)
      .ToList();

    List<string> ersUbrnList = ersEntries.Select(t => t.Item.Id).ToList();

    IEnumerable<string> ubrnsToClose = inProgressUbrns.Except(ersUbrnList);

    if (!ubrnsToClose.Any())
    {
      return 0;
    }

    int recordsProcessed = 0;

    foreach (string ubrn in ubrnsToClose)
    {
      if (reportOnly == true)
      {
        _log.Debug("Ubrn {ubrn}: Flagged for SetErsReferralClosed.", ubrn);
      }
      else
      {
        if (await _dataProvider.SetErsReferralClosed(ubrn))
        {
          recordsProcessed++;
          continue;
        }
      }
    }

    if (recordsProcessed != ubrnsToClose.Count())
    {
      Log.Error(
        "Records Removed: {recordsProcessed}, Failed to remove: {failed}",
        recordsProcessed,
        ubrnsToClose.Count() - recordsProcessed);
    }

    return recordsProcessed;
  }

  /// <summary>
  /// Process a referral entry, creating a new referral record where required.
  /// </summary>
  /// <param name="workListEntry">The eReferral item to be processed</param>
  /// <returns>Returns TRUE if the process succeeds.</returns>
  protected virtual async Task<bool> ProcessNewReferral(
    ErsWorkListEntry workListEntry,
    bool reportOnly,
    bool showDiagnostics,
    string attachmentId = null)
  {
    _log.Debug("Creating item {id}", workListEntry?.Item?.Id);

    ErsReferralResult registrationRecord =
      await _dataProvider.GetRegistration(
        workListEntry,
        null,
        attachmentId,
        null,
        _smartCardAuthentictor.ActiveSession,
        showDiagnostics);

    if (registrationRecord == null)
    {
      _log.Debug("Registration record {id} was not retrieved.",
        workListEntry?.Item?.Id);

      return false;
    }

    if (registrationRecord.Success == false)
    {
      if (!registrationRecord.WasRetrievedFromErs)
      {
        return false;
      }

      if (registrationRecord.ExportErrors)
      {
        _log.Warning(
          "There were errors exporting the file to a PDF while processing record {ubrn}. " +
            "Skipping record creation.",
          workListEntry?.Item?.Id);
        return false;
      }

      if (registrationRecord.InteropErrors)
      {
        _log.Warning(
          "There were errors with the Word Interop while processing record {ubrn}. " +
            "Skipping record creation.",
          workListEntry?.Item?.Id);
        return false;
      }

      if (registrationRecord.NoValidAttachmentFound)
      {
        if (registrationRecord.Errors?.Count > 0)
        {
          _log.Error(registrationRecord.AggregateErrors);
        }

        if (reportOnly)
        {
          _log.Debug("Skipping creation of invalid attachment record as " +
            "process is in Report Only mode.");
        }
        else
        {
          ReferralInvalidAttachmentPost invalidAttachmentPost = new()
          {
            MostRecentAttachmentDate = registrationRecord.MostRecentAttachmentDate,
            ReferralAttachmentId = registrationRecord.AttachmentId,
            ServiceId = registrationRecord.ServiceIdentifier,
            Ubrn = registrationRecord.Ubrn,
          };

          await _dataProvider.NewInvalidAttachment(invalidAttachmentPost);
        }

        return true;
      }
    }

    if (reportOnly == true)
    {
      _log.Debug("Skipping creation of record as process is in " +
        "Report Only mode.");

      if (showDiagnostics == true)
      {
        ShowDiagnostics(registrationRecord);
      }

      return true;
    }

    CreateReferralResult createReferralResult;

    if (registrationRecord.NoValidAttachmentFound)
    {
      registrationRecord.Pdf = new ReferralAttachmentPdfProcessor(
        _log, _auditLog,
        registrationRecord.ServiceIdentifier,
        _config.Data.ParserConfig,
        _config.Data.AnswerMap);

      ReferralMissingAttachmentPost missingAttachmentPost = new()
      {
        Ubrn = registrationRecord.Ubrn,
        ServiceId = registrationRecord.ServiceIdentifier
      };

      _log.Debug("Missing attachment for UBRN {ubrn} on Service Id {id}",
        registrationRecord.Ubrn, registrationRecord.ServiceIdentifier);

      await _dataProvider.NewMissingAttachment(missingAttachmentPost);

      return true;
    }

    ReferralPost referralPost =
      registrationRecord.Pdf.GenerateReferralCreationObject(
         workListEntry.Item.Id, registrationRecord.AttachmentId,
         registrationRecord.MostRecentAttachmentDate);

    if (!string.IsNullOrWhiteSpace(referralPost.NhsNumber))
    {
      NhsNumberAttribute nhsNumberValidator = new();

      ValidationResult validationResult = nhsNumberValidator
        .GetValidationResult(referralPost.NhsNumber, new(referralPost.NhsNumber));

      if (validationResult != ValidationResult.Success)
      {
        Log.Debug(
          "NHS Number in eRS referral letter is invalid for UBRN {ubrn}", 
          workListEntry.Ubrn);

        ReferralInvalidAttachmentPost request = new()
        {
          MostRecentAttachmentDate = registrationRecord.MostRecentAttachmentDate,
          ReferralAttachmentId = registrationRecord.AttachmentId,
          ServiceId = workListEntry.ServiceIdentifier,
          Ubrn = workListEntry.Ubrn,
        };

        await _dataProvider.NewInvalidAttachment(request);

        return true;
      }
      else if (workListEntry.NhsNumber != referralPost.NhsNumber)
      {
        _log.Debug("NHS Number Mismatch for UBRN {ubrn}", workListEntry.Ubrn);

        ReferralNhsNumberMismatchPost request = new()
        {
          DocumentVersion = referralPost.DocumentVersion,
          MostRecentAttachmentDate = referralPost.MostRecentAttachmentDate,
          NhsNumberAttachment = referralPost.NhsNumber,
          NhsNumberWorkList = workListEntry.NhsNumber,
          ReferralAttachmentId = referralPost.ReferralAttachmentId,
          ServiceId = workListEntry.ServiceIdentifier,
          SourceSystem = referralPost.SourceSystem,
          Ubrn = workListEntry.Ubrn,
        };

        await _dataProvider.NewNhsNumberMismatch(request);

        return true;
      }
    }

    //Download CRI document here
    GetCriResult cri = await _dataProvider.GetCriDocument(
      _smartCardAuthentictor.ActiveSession,
      referralPost.NhsNumber,
      referralPost.Ubrn);

    if (cri.Success == false)
    {
      _log.Error("Clinical information not downloaded for UBRN {ubrn}. " +
      "See logs for details.", workListEntry.Ubrn);
      referralPost.CriLastUpdated = null;
      referralPost.CriDocument = null;
    }
    else if (cri.NoCriDocumentFound)
    {
      _log.Information("No Cri Document Available for UBRN {ubrn}.",
        workListEntry.Ubrn);
      referralPost.CriLastUpdated = null;
      referralPost.CriDocument = null;
    }
    else
    {
      referralPost.CriLastUpdated = workListEntry.ClinicalInfoLastUpdated;
      referralPost.CriDocument = Convert.ToBase64String(cri.CriDocument);
    }

    // Create referral result
    createReferralResult = await _dataProvider.CreateReferral(referralPost);

    if (createReferralResult.Success == false)
    {
      _log.Error(createReferralResult.AggregateErrors);
      return false;
    }

    if (createReferralResult.CloseErsReferral)
    {
      _log.Debug("Closing ERS Referral {id}", workListEntry.Item.Id);

      await RecordReviewOutcomeAndSetErsClosedAsync(
        registrationRecord.ErsReferral,
        referralPost.NhsNumber,
        Outcome.RETURN_TO_REFERRER_WITH_ADVICE,
        reportOnly);
    }
    else
    {
      _log.Debug("CloseErsReferral is false for ERS Referral {id}",
        workListEntry.Item.Id);
    }

    return true;
  }

  /// <summary>
  /// Process a referral which is awaiting an update
  /// </summary>
  /// <param name="wmsRecord">The record from the Wms system</param>
  /// <param name="ersRecord">The record from the eReferrals system</param>
  /// <param name="reportOnly">If true, runs in diagnostic mode</param>
  /// <param name="showDiagnostics">If true, extra diagnostic information
  /// will be sent to the console.</param>
  /// <param name="overrideAttachmentId">Forces an alternative attachment
  /// to be processed rather than the latest attachment uploaded.</param>
  /// <param name="reprocessUnchangedAttachment">Forces reprocessing
  /// of attachments which have already been attempted.</param>
  /// <returns>True if processed without errors.</returns>
  public virtual async Task<bool> ProcessExistingReferral(
    GetActiveUbrnResponse wmsRecord,
    ErsWorkListEntry ersRecord,
    bool reportOnly,
    bool showDiagnostics,
    string overrideAttachmentId,
    bool reprocessUnchangedAttachment)
  {
    if (wmsRecord == null)
    {
      throw new ArgumentNullException("Active Ubrn Record should not be null");
    }

    if (ersRecord == null)
    {
      throw new ArgumentNullException("Ers Worklist Entry should not be null");
    }

    // if we are re-processing an exisiting attachment then the attachment id and the
    // MostRecentAttachmentDate sent to GetRegistration need to be null.
    DateTimeOffset? mostRecentAttachmentDate = wmsRecord.MostRecentAttachmentDate;
    string referralAttachmentId = wmsRecord.ReferralAttachmentId;
    
    if (reprocessUnchangedAttachment)
    {
      mostRecentAttachmentDate = null;
      referralAttachmentId = null;
    }

    _log.Debug("Getting Registration Record from eRS");

    ErsReferralResult registrationRecord = await _dataProvider.GetRegistration(
      ersRecord,
      referralAttachmentId,
      overrideAttachmentId,
      mostRecentAttachmentDate,
      _smartCardAuthentictor.ActiveSession,
      showDiagnostics);

    if (registrationRecord == null)
    {
      _log.Debug("Registration {ubrn} record was not retrieved.",
        ersRecord?.Item?.Id);

      return false;
    }

    if (registrationRecord.Success == false)
    {
      if (registrationRecord.InteropErrors)
      {
        _log.Warning("There were errors with the Word Interop while " +
          "processing record '{ubrn}'. Skipping record update.",
          ersRecord?.Item?.Id);

        return false;
      }

      if (registrationRecord.Pdf == null
        || !registrationRecord.Pdf.IsValidFileExtension 
        || registrationRecord.NoValidAttachmentFound)
      {
        if (registrationRecord.NoValidAttachmentFound)
        {
          _log.Debug("Registration Record retrieval failed. No Attachment.");

          ReferralMissingAttachmentPost missingAttachmentPost = new()
          {
            ServiceId = registrationRecord.ServiceIdentifier,
            Ubrn = registrationRecord?.Ubrn ?? "Unknown UBRN"
          };

          if (registrationRecord.Errors?.Count > 0)
          {
            _log.Error(registrationRecord.AggregateErrors);
          }

          if (reportOnly)
          {
            _log.Debug("Skipping updating the hub with missing attachment" +
              " update in report mode.");
          }
          else
          {
            await _dataProvider.UpdateMissingAttachment(missingAttachmentPost);
          }
        }
        else
        {
          _log.Debug("Registration Record retrieval failed. Invalid Attachment");

          ReferralInvalidAttachmentPost invalidAttachmentPost =
            new ReferralInvalidAttachmentPost()
            {
              MostRecentAttachmentDate = registrationRecord?.MostRecentAttachmentDate,
              ReferralAttachmentId = registrationRecord?.AttachmentId ?? "Unknown AttachmentId",
              Ubrn = registrationRecord?.Ubrn ?? "Unknown UBRN"
            };

          if (registrationRecord.Errors?.Count > 0)
          {
            _log.Error(registrationRecord.AggregateErrors);
          }

          if (reportOnly)
          {
            _log.Debug("Skipping updating the hub with invalid attachment" +
              " update in report mode.");
          }
          else
          {
            await _dataProvider.UpdateInvalidAttachment(invalidAttachmentPost);
          }
        }

        return false;
      }

      _log.Error(registrationRecord.AggregateErrors);
      return false;
    }

    if (reportOnly == false)
    {
      if (reprocessUnchangedAttachment)
      {
        _log.Debug("Reprocessing exisiting attachment {attachment} " +
          "for {ubrn}.", registrationRecord.AttachmentId, ersRecord.Item.Id);
      }
      else
      {
        // Check the current attachment date on ers is newer than the one in the existing record.
        if (registrationRecord.MostRecentAttachmentDate == wmsRecord.MostRecentAttachmentDate)
        {
          _log.Debug("No changes to Referral Record on eRS.");
          _log.Debug("Did not need to update record {id} as there were " +
            "no new attachments after after attachment with Id of " +
            "{attid}", ersRecord.Item.Id, registrationRecord.AttachmentId);
          return true;
        }
      }
    }
    //if it is, send the new details to the API or report on it if the
    //method is being run in report mode.
    if (reportOnly == true)
    {
      if (showDiagnostics == true)
      {
        ShowDiagnostics(registrationRecord);
      }

      return true;
    }
    else
    {
      if (registrationRecord.NoValidAttachmentFound
        || registrationRecord.Pdf == null)
      {
        _log.Debug("Skipped updating record with UBRN {ubrn} as the " +
          "record has no new valid attachments.", registrationRecord.Ubrn);

        return true;
      }

      ReferralPut referralPut =
        registrationRecord.Pdf.GenerateReferralUpdateObject(
          ersRecord.Item.Id,
          registrationRecord.AttachmentId,
          registrationRecord.MostRecentAttachmentDate);

      if (!string.IsNullOrWhiteSpace(referralPut.NhsNumber))
      {
        NhsNumberAttribute nhsNumberValidator = new();

        ValidationResult validationResult = nhsNumberValidator
          .GetValidationResult(referralPut.NhsNumber, new (referralPut.NhsNumber));

        if (validationResult != ValidationResult.Success)
        {
          Log.Debug("NHS Number in eRS referral letter is invalid for UBRN {ubrn}", ersRecord.Ubrn);

          ReferralInvalidAttachmentPost request = new()
          {
            MostRecentAttachmentDate = registrationRecord.MostRecentAttachmentDate,
            ReferralAttachmentId = registrationRecord.AttachmentId,
            ServiceId = wmsRecord.ServiceId,
            Ubrn = ersRecord.Ubrn
          };

          await _dataProvider.UpdateInvalidAttachment(request);

          return false;
        }
        else if (referralPut.NhsNumber != ersRecord.NhsNumber)
        {
          Log.Debug("NHS Number Mismatch for UBRN {ubrn}", ersRecord.Ubrn);

          ReferralNhsNumberMismatchPost request = new ()
          {
            MostRecentAttachmentDate = registrationRecord.MostRecentAttachmentDate,
            NhsNumberAttachment = referralPut.NhsNumber,
            NhsNumberWorkList = ersRecord.NhsNumber,
            ReferralAttachmentId = registrationRecord.AttachmentId,
            ServiceId = wmsRecord.ServiceId,
            Ubrn = ersRecord.Ubrn,
          };

          await _dataProvider.UpdateNhsNumberMismatch(request);

          return false;
        }
      }

      UpdateReferralResult updateReferralResult =
        await _dataProvider.UpdateReferral(referralPut, ersRecord.Item.Id);

      if (updateReferralResult.Success == false)
      {
        _log.Error(updateReferralResult.AggregateErrors);

        return false;
      }
    }

    return true;
  }

  private async Task<bool> CreateOrUpdateCriDocument(
    GetActiveUbrnResponse wmsRecord,
    ErsWorkListEntry ersRecord,
    bool reportOnly)
  {
    bool result = false;
    if (wmsRecord.CriLastUpdated == null)
    {
      //Create CRI Document
      _log.Debug("CRI Document was missing from {ubrn}.", ersRecord.Ubrn);

      //Get cri document
      GetCriResult cri = await _dataProvider.GetCriDocument(
        _smartCardAuthentictor.ActiveSession,
        ersRecord.NhsNumber,
        ersRecord.Ubrn);

      if (cri.Success == false)
      {
        _log.Error(cri.AggregateErrors);
      }
      else if (cri.NoCriDocumentFound)
      {
        _log.Information("No Cri Document Found for UBRN {ubrn}.",
          ersRecord.Ubrn);
      }
      else
      {
        string documentAsBase64 = Convert.ToBase64String(cri.CriDocument);
        //Send to Hub API
        if (reportOnly == false)
        {
          CriCreateRequest request = new CriCreateRequest()
          {
            ClinicalInfoLastUpdated = ersRecord.ClinicalInfoLastUpdated,
            CriDocument = documentAsBase64,
            Ubrn = wmsRecord.Ubrn
          };
          CreateCriRecordResult createCriResult =
            await _dataProvider.CreateCriRecord(request);
          if (createCriResult.Success == false)
          {
            _log.Error(createCriResult.AggregateErrors);
            result = false;
          }
          else
          {
            result = true;
          }
        }
        else
        {
          result = true; //Always return true in report mode
        }
      }
    }
    else
    {
      if (wmsRecord.CriLastUpdated < ersRecord.ClinicalInfoLastUpdated)
      {
        //Update Cri Record

        _log.Debug("CRI Document was out of date for UBRN {ubrn}.",
          ersRecord.Ubrn);

        //Get cri document
        GetCriResult cri = await _dataProvider.GetCriDocument(
          _smartCardAuthentictor.ActiveSession,
          ersRecord.NhsNumber,
          ersRecord.Ubrn);

        if (cri.Success == false)
        {
          _log.Error(cri.AggregateErrors);
        }
        else if (cri.NoCriDocumentFound)
        {
          _log.Information("Cri Document Not Found for UBRN {ubrn}."
            , ersRecord.Ubrn);
        }
        else
        {
          if (reportOnly == false)
          {
            string documentAsBase64 = Convert.ToBase64String(cri.CriDocument);
            //Send to Hub API
            CriUpdateRequest request = new CriUpdateRequest()
            {
              ClinicalInfoLastUpdated = ersRecord.ClinicalInfoLastUpdated,
              CriDocument = documentAsBase64
            };
            UpdateCriRecordResult createCriResult =
              await _dataProvider.UpdateCriRecord(request, wmsRecord.Ubrn);
            if (createCriResult.Success == false)
            {
              _log.Error(cri.AggregateErrors);
            }
            else
            {
              result = true;
            }
          }
          else
          {
            result = true; //always return true in report mode
          }
        }
      }
    }
    return result;
  }

  public async Task<ProcessExecutionResult> DownloadReferralDocumentsByUbrnList(
    string ubrnListFileName,
    string destinationFolderPath)
  {
    ProcessExecutionResult result = new ProcessExecutionResult()
    {
      Success = true,
      Completed = false
    };
    if (string.IsNullOrEmpty(ubrnListFileName) == true)
    {
      _log.Verbose("DownloadReferralDocumentsByUbrnList requires " +
        "a Source file name.");
      result.Success = false;
      return result;
    }
    if (string.IsNullOrEmpty(destinationFolderPath) == true)
    {
      _log.Verbose("DownloadReferralDocumentsByUbrnList requires " +
        "a destination folder path.");
      result.Success = false;
      return result;
    }
    if (File.Exists(ubrnListFileName))
    {
      _log.Verbose("Download from file '{filename}'.", ubrnListFileName);
    }
    else
    {
      _log.Verbose("UBRN list file '{fileName}' does not exist.",
        ubrnListFileName);
      result.Success = false;
      return result;
    }
    if (Directory.Exists(destinationFolderPath))
    {
      _log.Verbose("Downloading to folder '{destination}'.",
        destinationFolderPath);
    }
    else
    {
      _log.Verbose("Destination folder '{destination}' does not exist.",
        destinationFolderPath);
      result.Success = false;
      return result;
    }

    try
    {
      //Open the ubrn list
      string[] ubrnLines = File.ReadAllLines(ubrnListFileName);
      if (ubrnLines.Length == 0)
      {
        _log.Verbose("Nothing to download.");
        result.Completed = true;
        return result;
      }
      else
      {
        _log.Verbose("Processing {lines} lines...", ubrnLines.Length);
      }

      //Download eReferrals worklist
      bool connect = await _smartCardAuthentictor.CreateSession();

      if (!_smartCardAuthentictor.ActiveSession.SmartCardIsAuthenticated)
      {
        _log.Verbose("Smart card was not authenticated.  " +
          "Cannot continue.");
        result.Success = false;
        return result;

      }
      if (!_smartCardAuthentictor.ActiveSession.IsAuthenticated)
      {
        _log.Verbose("Session was not authenticated.  " +
          "Cannot continue.");
        result.Success = false;
        return result;

      }

      _log.Verbose("Downloading worklist from eRS...");
      //Get list from eRS
      WorkListResult workList = await _dataProvider.GetWorkListFromErs(
        _smartCardAuthentictor.ActiveSession);
      if (workList.Success == false)
      {
        _log.Verbose(workList.AggregateErrors);
        await _smartCardAuthentictor.TerminateSession();
        result.Success = false;
        return result;
      }
      ErsWorkList currentWorkList = workList.WorkList;
      _log.Verbose("Downloaded {length} records.",
        currentWorkList.Entry.Length);

      int recordNumber = 0;
      string actualUbrn;
      foreach (string ubrn in ubrnLines)
      {
        actualUbrn = "";
        recordNumber++;
        if (string.IsNullOrWhiteSpace(ubrn) == true)
        {
          _log.Verbose("Ignored empty line {record}.", recordNumber);
        }
        else
        {
          //Normalise the UBRN number
          long ubrnNumber;
          bool fileExists = false;
          if (long.TryParse(ubrn, out ubrnNumber) == true)
          {
            actualUbrn = $"{ubrnNumber:000000000000}";
            _log.Verbose("Processing UBRN {actualUbrn} on line " +
              "{recordNumber}", actualUbrn, recordNumber);
            string[] files = Directory.GetFiles(destinationFolderPath);
            fileExists = files.Any(f => f.Contains(actualUbrn));

            if (fileExists)
            {
              _log.Verbose("Skipping UBRN {ubrn} as a file already exists.",
                actualUbrn);
            }
            else
            {
              //Look for the UBRN in the worklist
              ErsWorkListEntry ersRecord = currentWorkList.Entry
                .Where(r => r.Ubrn == actualUbrn).FirstOrDefault();
              if (ersRecord == null)
              {
                //If the UBRN isn't there, ignore it
                _log.Verbose("UBRN {ubrn} did not exist on the eRS work " +
                  "list.",
                  actualUbrn);
              }
              else
              {
                // Download the latest valid attachment into memory and save.
                ErsReferralResult registrationRecord = await _dataProvider.GetRegistration(
                  ersRecord,
                  null,
                  null,
                  null,
                  _smartCardAuthentictor.ActiveSession,
                  false);

                if (registrationRecord.Success == true)
                {
                  if (registrationRecord.NoValidAttachmentFound == true)
                  {
                    _log.Verbose("No valid attachment found for {ubrn}",
                      actualUbrn);
                  }
                  else
                  {
                    //Save the file
                    string fileName = Path.Combine(destinationFolderPath,
                      $"{actualUbrn}." +
                      $"{registrationRecord.Pdf.OriginalFileExtension}");
                    File.WriteAllBytes(fileName,
                      registrationRecord.Pdf.OriginalAttachmentDocument);
                  }
                }
              }
              //Next line
            }
          }
          else
          {
            _log.Verbose("Ignoring '{ubrn}' from line {record} " +
              "as the number is invalid.", ubrn, recordNumber);
          }
        }

      }
      result.Completed = true;
    }
    catch (Exception ex)
    {
      _log.Verbose(ex, "Error processing download.");
      result.Success = false;
    }
    finally
    {
      await _smartCardAuthentictor?.TerminateSession();
    }
    return result;
  }

  /// Sends diagnostic information about the referral to the log
  /// </summary>
  /// <param name="ersReferral">The result of the conversion
  /// process</param>
  private void ShowDiagnostics(ErsReferralResult ersReferral)
  {
    try
    {
      if (ersReferral.NoValidAttachmentFound)
      {
        _log.Verbose("No attachment for UBRN {ubrn}", ersReferral.Ubrn);
      }
      else
      {
        if (ersReferral.Pdf == null)
        {
          _log.Verbose("There was no valid PDF file to show");
        }
        else
        {
          ersReferral.Pdf.ShowDiagnostics(
            ersReferral.Ubrn,
            ersReferral.AttachmentId);
        }
      }
    }
    catch (Exception ex)
    {
      _log.Verbose(ex, "Error showing Diagnostics");
    }
  }

  /// <summary>
  /// Changes the order of items in a worklist so unregistered items are
  /// at the top
  /// </summary>
  /// <param name="worklist"></param>
  /// <param name="registeredItemsList"></param>
  public void PrioritiseWorklistItems(
    ErsWorkList worklist,
    RegistrationList registeredItemsList)
  {
    List<ErsWorkListEntry> newList = new();
    List<ErsWorkListEntry> existingList = new();

    foreach (ErsWorkListEntry ersEntry in worklist.Entry)
    {
      if (registeredItemsList.Ubrns.Any(r => r.Ubrn == ersEntry.Ubrn))
      {
        existingList.Add(ersEntry);
      }
      else
      {
        newList.Add(ersEntry);
      }
    }

    newList.AddRange(existingList);
    worklist.Entry = newList.ToArray();
  }


  public async Task SendErsWorklistUbrnsToFile(string outputFileName)
  {
    if (string.IsNullOrWhiteSpace(outputFileName))
    {
      throw new ArgumentException("Output Filename was not provided.");
    }

    string dir = Path.GetDirectoryName(outputFileName);
    string fileName = Path.GetFileName(outputFileName);
    if (string.IsNullOrWhiteSpace(fileName))
    {
      throw new ArgumentException("Output Filename was not provided.");
    }
    if (string.IsNullOrWhiteSpace(dir))
    {
      throw new ArgumentException("Directory was not provided.");
    }
    if (Directory.Exists(dir))
    {
      Console.WriteLine("Creating file to {directory}", dir);
    }
    else
    {
      throw new ArgumentException("Directory '{directory}' does not exist.",
        dir);
    }

    //This will read the smart card and authenticate a session
    await _smartCardAuthentictor.CreateSession();

    if (!_smartCardAuthentictor.ActiveSession.SmartCardIsAuthenticated)
    {
      throw new SmartCardException("Smart card was not authenticated.");
    }
    if (!_smartCardAuthentictor.ActiveSession.IsAuthenticated)
    {
      throw new SmartCardException("Ers Session was not authenticated.");
    }
    try
    {
      //Get list from eRS
      WorkListResult ersResult = await _dataProvider.GetWorkListFromErs(
        _smartCardAuthentictor.ActiveSession);
      if (!ersResult.Success)
      {
        throw new ReferralServiceException(ersResult.AggregateErrors);
      }

      //Output UBRN entries in the worklist to the file
      using (StreamWriter file = new StreamWriter(outputFileName, false))
      {
        foreach (ErsWorkListEntry entry in ersResult.WorkList.Entry)
        {
          file.WriteLine(entry.Ubrn);
        }
      }
    }
    finally
    {
      await _smartCardAuthentictor.TerminateSession();
    }
  }
  protected async Task<bool> CanRecordReviewOutcomeAsync(
    IErsReferral ersReferral,
    string nhsNumber)
  {
    bool canRecordReviewOutcome;

    _log.Debug($"Check {ReferralAction.RECORD_REVIEW_OUTCOME} action is " +
      $"available before setting a review comment.");

    AvailableActionResult availableActions = await _dataProvider
      .GetAvailableActions(
        _smartCardAuthentictor.ActiveSession,
        ersReferral,
        nhsNumber);

    if (availableActions != null && availableActions.Success)
    {
      if (availableActions.HasAction(ReferralAction.RECORD_REVIEW_OUTCOME))
      {
        _log.Debug(
          $"{ReferralAction.RECORD_REVIEW_OUTCOME} action is available.");

        canRecordReviewOutcome = true;
      }
      else
      {
        _log.Debug("Skipped Updating eRS Record for UBRN {ubrn} as the " +
          $"{ReferralAction.RECORD_REVIEW_OUTCOME} action was not " +
          $"available. WMS record still updated.",
          ersReferral.Id);

        canRecordReviewOutcome = false;
      }
    }
    else
    {
      _log.Debug(
        "There was an error getting the available actions, for UBRN {ubrn}.",
        ersReferral.Id);

      canRecordReviewOutcome = false;
    }

    return canRecordReviewOutcome;
  }


  protected async Task RecordReviewOutcomeAndSetErsClosedAsync(
    string nhsNumber,
    Outcome outcome,
    bool reportOnly,
    string ubrn)
  {
    IErsReferral ersReferral = await _dataProvider.GetErsReferralByUbrn(
      _smartCardAuthentictor.ActiveSession,
      ubrn,
      nhsNumber);

    if (ersReferral.WasRetrievedFromErs)
    {
      await RecordReviewOutcomeAndSetErsClosedAsync(
        ersReferral,
        nhsNumber,
        outcome,
        reportOnly);
    }
    else
    {
      _log.Error("UBRN {ubrn}: Cannot record outcome as the referral" +
        " record could not be retrieved from eRS.",
        ubrn);

    }
  }

  protected async Task RecordReviewOutcomeAndSetErsClosedAsync(
    IErsReferral ersReferral,
    string nhsNumber,
    Outcome outcome,
    bool reportOnly)
  {
    if (await CanRecordReviewOutcomeAsync(ersReferral, nhsNumber))
    {
      if (reportOnly)
      {
        _log.Debug("ReportOnly: Skipping eRS record update for UBRN {ubrn}.",
          ersReferral.Id);
      }
      else
      {
        _log.Debug(
          "Updating eRS Record for UBRN {ubrn} with version {version}.",
          ersReferral.Id,
          ersReferral.Meta.versionId);

        IReviewCommentResult reviewCommentResult = await _dataProvider
          .RecordOutcome(
            Constants.ErsOutcomeMessage,
            ersReferral,
            nhsNumber,
            outcome,
            _smartCardAuthentictor.ActiveSession);

        if (reviewCommentResult.Success)
        {
          await _dataProvider.SetErsReferralClosed(ersReferral.Id);
        }
        else
        {
          _log.Error(reviewCommentResult.AggregateErrors);
        }
      }
    }
  }
}
