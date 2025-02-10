using Microsoft.Extensions.Configuration;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Mlcsu.Diu.Mustard.Email;
using Mlcsu.Diu.Mustard.Logging.Serilog;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using WmsHub.ReferralService.Interop;
using WmsHub.ReferralsService.Console.Interfaces;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.ReferralsService.Models.Results;
using WmsHub.ReferralsService.Pdf;
using static WmsHub.ReferralsService.Console.Enums;

namespace WmsHub.ReferralsService.Console.Services;

public class ConsoleAppService : IConsoleAppService
{
  private Config _config;
  private IConfiguration _configuration;
  private IHttpClientFactory _httpClientFactory;
  private IProcessStatusService _processStatusService;
  private ISendEmailService _sendEmailService;
  public ILogger _auditLogger;

  public void ConfigureService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IProcessStatusService processStatusService,
    ISendEmailService sendEmailService)
  {
    _config = new Config();
    configuration.Bind(_config);
    _configuration = configuration;

    ConfigurationReaderOptions options = new()
    {
      SectionName = "SerilogAudit"
    };
    _httpClientFactory = httpClientFactory;
    _processStatusService = processStatusService;
    _sendEmailService = sendEmailService;

    _auditLogger = new LoggerConfiguration()
      .ReadFrom
      .Configuration(configuration, options)
#if DEBUG
      .WriteTo.Debug()
#endif
      .CreateLogger();
  }

  public async Task<int> PerformProcess(string[] args)
  {
    int exitCode = (int)ExitCode.Success;
    try
    {
      if (CheckFolder(_config.Data.InteropTemporaryFilePath) == true)
      {
        Log.Logger.Verbose("Temporary Folder Check Passed");
      }
      else
      {
        Log.Logger.Error("Temporary folder check failed. Process has " +
          "been cancelled.");
        exitCode = (int)ExitCode.Failure;
        return exitCode;
      }
      Log.Logger.Verbose("Loading Global Mappings");
      _config.Data.AnswerMap = new ReferralAttachmentAnswerMap();
      _config.Data.AnswerMap.Load();
      if (_config.Data.AnswerMap.DuplicateErrors)
      {
        Log.Logger.Error("Globals Mappings loaded with duplicate key " +
          "errors: {duplicates}.",
          _config.Data.AnswerMap.Duplicates);
        exitCode = (int)ExitCode.CriticalFailure;
        return exitCode;
      }
      else
      {
        Log.Logger.Debug("Global Mapping loading returned: {message}",
          _config.Data.AnswerMap.Duplicates);
      }
      Log.Logger.Debug("{total} Mappings Loaded.",
        _config.Data.AnswerMap.GlobalMap.Values.Count);

      Log.Logger.Debug("Referrals process Started");
      ISmartCardAuthentictor smartCardAuthentictor
        = new SmartCardAuthentication(
          _config,
          _httpClientFactory,
          Log.Logger,
          _auditLogger);

      IReferralsDataProvider data = new ReferralsDataProvider(
          _config,
          _httpClientFactory,
          smartCardAuthentictor,
          Log.Logger,
          _auditLogger);

      ReferralProcessor processor = new(
        data, smartCardAuthentictor,
        _config,
        Log.Logger,
        _auditLogger);

      ProcessExecutionResult success;

      if (args.Length >= 1)
      {
        switch (args[0])
        {
          case "/test_failure":
            Log.Error("Test Failure");
            _auditLogger.Error("Test Failure");
            exitCode = (int)ExitCode.Failure;
            break;

          case "/test_critical":
            Log.Error("Test Critical Failure");
            _auditLogger.Error("Test Critical Failure");
            exitCode = (int)ExitCode.CriticalFailure;
            break;

          case "/test_success":
            exitCode = (int)ExitCode.Success;
            break;

          case "/process_referrals":
            bool sendStatusToProcessStatus = true;
            try
            {
              string ubrn = "";

              if (args.Length > 1)
              {
                ubrn = args[1];
                // processing one UBRN so do not update process status
                sendStatusToProcessStatus = false;
              }

              string processAttachmentId = null;
              if (args.Length == 3)
              {
                try
                {
                  processAttachmentId = args[2];
                }
                catch (Exception)
                {
                  System.Console.WriteLine(
                    "Failed to recognise Attachment Id '{0}' as a valid numeric attachment id.",
                    args[2]);
                }
              }

              if (sendStatusToProcessStatus)
              {
                try
                {
                  await _processStatusService.StartedAsync();
                }
                catch (Exception ex)
                {
                  Log.Warning(ex, "Failed to send started to process status service");
                }
              }

              success = await processor.Process(false, ubrn, processAttachmentId);

              CleanupFolder(_config.Data.InteropTemporaryFilePath);

              if (success.Success == true)
              {
                Log.Debug("Process Succeeded.");

                try
                {
                  await _processStatusService.SuccessAsync();
                }
                catch (Exception ex)
                {
                  Log.Warning(ex, "Failed to send success to process status service");
                }
              }
              else
              {
                if (success.Completed == true)
                {
                  string msg = "Process failed but processed all records. See logs for details";

                  Log.Error(msg);
                  exitCode = (int)ExitCode.Failure;

                  if (sendStatusToProcessStatus)
                  {
                    try
                    {
                      await _processStatusService.FailureAsync(msg);
                    }
                    catch (Exception ex)
                    {
                      Log.Warning(ex, "Failed to send failure to process status service");
                    }
                  }
                }
                else
                {
                  string msg = "Process failed and was interrupted. See error logs for details";
                  Log.Error(msg);
                  exitCode = (int)ExitCode.CriticalFailure;

                  if (sendStatusToProcessStatus)
                  {
                    try
                    {
                      await _processStatusService.FailureAsync(msg);
                    }
                    catch (Exception ex)
                    {
                      Log.Warning(ex, "Failed to send failure to process status service");
                    }
                  }
                }
              }
            }
            catch (Exception ex)
            {
              if (sendStatusToProcessStatus)
              {
                try
                {
                  Log.Error(ex.Message);
                  await _processStatusService
                    .FailureAsync("Exception. See error logs for details.");
                }
                catch (Exception ex2)
                {
                  Log.Warning(ex2, "Failed to send failure to process status service");
                }
              }
              throw;
            }
            break;

          case "/reprocess_referrals":
            string reprocessUbrn = "";
            if (args.Length > 1)
            {
              reprocessUbrn = args[1];
            }

            string reProcessAttachmentId = null;
            if (args.Length == 3)
            {
              try
              {
                reProcessAttachmentId = args[2];
              }
              catch (Exception)
              {
                System.Console.WriteLine("Failed to recognise Attachment " +
                  "Id '{0}' as a valid numeric attachment id.",
                  args[2]);
              }
            }

            success = await processor.Process(
              false,
              reprocessUbrn,
              reProcessAttachmentId,
              true);

            CleanupFolder(_config.Data.InteropTemporaryFilePath);

            if (success.Success == true)
            {
              Log.Debug("Re-Process Succeeded.");
            }
            else
            {
              if (success.Completed == true)
              {
                Log.Error("Re-Process failed but processed all records.  See " +
                  "logs for details");
                exitCode = (int)ExitCode.Failure;
              }
              else
              {
                Log.Error("Re-Process failed and was interrupted before " +
                  "all records were re-processed.  See error logs for " +
                  "details");
                exitCode = (int)ExitCode.CriticalFailure;
              }
            }

            break;

          case "/report":
            string reportUbrn = "";
            if (args.Length > 1)
            {
              reportUbrn = args[1];
            }

            string reportAttachmentId = null;
            if (args.Length == 3)
            {
              try
              {
                reportAttachmentId = args[2];
              }
              catch (Exception)
              {
                System.Console.WriteLine("Failed to recognise Attachment" +
                  $" Id '{args[2]}' as a valid numeric attachment id. ");
              }
            }
            success = await processor.Process(
              true,
              reportUbrn,
              reportAttachmentId);

            CleanupFolder(_config.Data.InteropTemporaryFilePath);

            if (success.Success == true)
            {
              Log.Debug("Process Succeeded in Report Mode.");
            }
            else
            {
              if (success.Completed == true)
              {
                Log.Error("Process failed in Report Mode but processed " +
                  "all records.  See logs for details");
                exitCode = (int)ExitCode.Failure;
              }
              else
              {
                Log.Error("Process failed in Report Mode and was " +
                  "interrupted before all records were processed.  " +
                  "See error logs for details");
                exitCode = (int)ExitCode.CriticalFailure;
              }
            }
            break;

          case "/rereport":
            string reReportUbrn = "";
            if (args.Length > 1)
            {
              reReportUbrn = args[1];
            }
            string reReportAttachmentId = null;
            if (args.Length == 3)
            {
              try
              {
                reportAttachmentId = args[2];
              }
              catch (Exception)
              {
                System.Console.WriteLine("Failed to recognise Attachment " +
                  "Id '{0}' as a valid numeric attachment id.",
                  args[2]);
              }
            }

            success = await processor.Process(
              true,
              reReportUbrn,
              reReportAttachmentId,
              true);

            CleanupFolder(_config.Data.InteropTemporaryFilePath);

            if (success.Success == true)
            {
              Log.Debug("Re-Process Succeeded in Report Mode.");
            }
            else
            {
              if (success.Completed == true)
              {
                Log.Error("Re-Process failed in Report Mode but processed " +
                  "all records.  See logs for details");
                exitCode = (int)ExitCode.Failure;
              }
              else
              {
                Log.Error("Re-Process failed in Report Mode and was " +
                  "interrupted before all records were processed.  " +
                  "See error logs for details");
                exitCode = (int)ExitCode.CriticalFailure;
              }
            }
            break;

          case "/test_file":
            if (args.Length == 1)
            {
              Log.Verbose("Process failed.  No Filename Provided");
              exitCode = (int)ExitCode.Failure;
            }
            else
            {
              string filename = args[1];
              ReferralAttachmentPdfProcessor pdfProcessor =
                new(
                  Log.Logger,
                  _auditLogger,
                  "",
                  _config.Data.ParserConfig,
                  _config.Data.AnswerMap);
              if (await pdfProcessor.DownloadFile(
                filename,
                _config.Data.InteropTemporaryFilePath,
                _config.Data.ReformatDocument,
                _config.Data.SectionHeadings,
                _config.Data.RetryAllSources,
                _config.Data.NumberOfMissingQuestionsTolerance) == true)
              {
                pdfProcessor.ShowDiagnostics(filename, null);
              }
            }
            break;

          case "/create_from_csv":
            if (args.Length == 1)
            {
              Log.Verbose("Process failed.  No Filename Provided");
              exitCode = (int)ExitCode.Failure;
            }
            else
            {
              string filename = args[1];

              ReferralCSVProcessor csvProcessor = new(
                data, _config, Log.Logger);
              bool csvResult = await csvProcessor.CreateFromCSV(filename);
              if (csvResult == false)
              {
                exitCode = (int)ExitCode.Failure;
              }
            }
            break;

          case "/update_from_csv":
            if (args.Length == 1)
            {
              Log.Verbose("Process failed.  No Filename Provided");
              exitCode = (int)ExitCode.Failure;
            }
            else
            {
              string filename = args[1];

              ReferralCSVProcessor csvProcessor = new(
                data, _config, Log.Logger);
              bool csvResult = await csvProcessor.UpdateFromCSV(filename);
              if (csvResult == false)
              {
                exitCode = (int)ExitCode.Failure;
              }
            }

            break;

          case "/process_batch":
            if (args.Length == 1)
            {
              Log.Verbose("Process failed.  No Filename for batch header CSV" +
                " Provided");
              exitCode = (int)ExitCode.Failure;
            }
            else
            {
              string filename = args[1];

              ReferralCSVProcessor csvProcessor = new(
                data, _config, Log.Logger);
              Batch csvBatch = ReferralCSVProcessor.LoadBatchCSV(filename);
              if (csvBatch == null)
              {
                exitCode = (int)ExitCode.Failure;
              }

              success = await processor.Process(csvBatch, false);
              if (success.Success == true)
              {
                Log.Debug("Batch Process Succeeded.");
              }
              else
              {
                if (success.Completed == true)
                {
                  Log.Error("Batch Process failed but processed " +
                    "all records.  See logs for details");
                  exitCode = (int)ExitCode.Failure;
                }
                else
                {
                  Log.Error("Batch Process failed and was " +
                    "interrupted before all records were processed.  " +
                    "See error logs for details");
                  exitCode = (int)ExitCode.CriticalFailure;
                }
              }
            }
            break;

          case "/report_batch":
            if (args.Length == 1)
            {
              Log.Verbose("Process failed.  No Filename for batch header CSV" +
                " Provided");
              exitCode = (int)ExitCode.Failure;
            }
            else
            {
              string filename = args[1];

              ReferralCSVProcessor csvProcessor = new(
                data, _config, Log.Logger);
              Batch csvBatch = ReferralCSVProcessor.LoadBatchCSV(filename);
              if (csvBatch == null)
              {
                exitCode = (int)ExitCode.Failure;
              }

              success = await processor.Process(csvBatch, true);
              if (success.Success == true)
              {
                Log.Debug("Batch Process Succeeded in Report mode.");
              }
              else
              {
                if (success.Completed == true)
                {
                  Log.Error("Batch Process failed in Report Mode but " +
                    "processed all records.  See logs for details");
                  exitCode = (int)ExitCode.Failure;
                }
                else
                {
                  Log.Error("Batch Process failed in Report Mode and was " +
                    "interrupted before all records were processed.  " +
                    "See error logs for details");
                  exitCode = (int)ExitCode.CriticalFailure;
                }
              }
            }

            break;

          case "/download_batch":
            string fileBatch = "";
            string destinationFolder = "";
            if (args.Length == 1)
            {
              Log.Verbose("Process failed.  No Filename for batch" +
                " Provided");
              exitCode = (int)ExitCode.Failure;
              break;
            }
            if (args.Length > 1)
            {
              //Get filename of batch
              fileBatch = args[1];
              if (File.Exists(fileBatch))
              {
                Log.Verbose("Downloading from batch file '{filename}'",
                  fileBatch);
              }
              else
              {
                Log.Verbose("File {filename} not found.",
                  fileBatch);
                exitCode = (int)ExitCode.Failure;
                break;
              }
              //A custom destination folder can be provided
              if (args.Length == 3)
              {
                destinationFolder = args[2];
                if (Directory.Exists(destinationFolder))
                {
                  Log.Verbose("Downloading to custom folder '{folder}'",
                    destinationFolder);
                }
                else
                {
                  Log.Verbose("Destination folder '{folder}' not found.",
                    destinationFolder);
                  exitCode = (int)ExitCode.Failure;
                  break;
                }
              }
              else
              {
                destinationFolder = Path.Combine(
                  _config.Data.InteropTemporaryFilePath, "Download");
                if (Directory.Exists(destinationFolder) == true)
                {
                  Log.Verbose("Downloading to default folder '{folder}'",
                    destinationFolder);
                }
                else
                {
                  Log.Verbose("Creating default destination folder 'folder'",
                    destinationFolder);
                  Directory.CreateDirectory(destinationFolder);
                }
              }
            }

            success = await processor.DownloadReferralDocumentsByUbrnList(
              fileBatch, destinationFolder);
            if (success.Success == true)
            {
              Log.Verbose("Downloads Complete.");
              exitCode = (int)ExitCode.Success;
            }
            else
            {
              Log.Verbose("Downloads Complete With Errors.");
              exitCode = (int)ExitCode.Failure;
            }
            break;

          case "/list_ers":
            Log.Verbose("Creating UBRN list from Worklist.");
            if (args.Length == 1)
            {
              System.Console.WriteLine("ERROR: A filename should be " +
                "provided.");
            }
            else
            {
              try
              {
                await processor.SendErsWorklistUbrnsToFile(args[1]);
                System.Console.WriteLine("Process Complete.");
                exitCode = (int)ExitCode.Success;
              }
              catch (Exception ex)
              {
                Log.Verbose(ex, "Error Producing UBRN List");
                exitCode = (int)ExitCode.Failure;
              }
            }
            break;

          case "/upload_missing_log":
            Log.Verbose("Uploading missing log file to referral API.");
            if (args.Length == 1)
            {
              System.Console.WriteLine(
                "ERROR: The log's filename must be provided.");
            }
            else
            {
              string logFilename = args[1];
              DateTimeOffset? from = args.Length > 2
                ? DateTimeOffset.Parse(args[2])
                : null;
              DateTimeOffset? to = args.Length > 3
                ? DateTimeOffset.Parse(args[3])
                : null;
              try
              {
                UploadMissingLogFile uploadMissingLogFile = new(
                  _configuration,
                  logFilename,
                  from,
                  to);

                await uploadMissingLogFile.ProcessAsync();

                System.Console.WriteLine(
                  $"Uploaded {uploadMissingLogFile.NumberOfLogEvents} " +
                  $"from {logFilename}.");
                exitCode = (int)ExitCode.Success;
              }
              catch (Exception ex)
              {
                Log.Verbose(ex, $"Upload of log file {logFilename} failed.");
                exitCode = (int)ExitCode.Failure;
              }
            }
            break;

          default:
            System.Console.WriteLine(
              "The following switches are available:");
            System.Console.WriteLine(@"/create_from_csv <filename> : Create" +
              " referral records from a CSV file.");
            System.Console.WriteLine(@"/discharge_referrals : " +
              " Processes all referrals marked as awaiting discharge.");
            System.Console.WriteLine(@"/download_batch <filename> " +
              "[destination path] : Loads the latest valid attachments from" +
              " the UBRNs contained within a csv file and saves them to " +
              " a folder.");
            System.Console.WriteLine(@"/process_batch <filename> : Loads a" +
              " csv batch file of UBRNs and processes them");
            System.Console.WriteLine(
              @"/process_referrals [[UBRN] [AttachmentId]]: Run the Process. If UBRN " +
              "is provided, just that record will be processed.  If AttachmentId is provided," +
              " just that attachment will be processed.");
            System.Console.WriteLine(@"/report [UBRN [AttachmentId]]: " +
              "Runs the process without committing any records to the hub. " +
              "  If UBRN is provided, just that record will be processed." +
              "If AttachmentId is provided, just that attachment file will " +
              "be processed." +
              "Three diagnostic reports are produced - the final referral " +
              "object content, the complete mappings derived from the " +
              "attached file and then the content of the file unfiltered.  " +
              "This can be run several times as it is non-destructive.");
            System.Console.WriteLine(@"/report_batch <filename> : Loads a" +
              " csv batch file of UBRNs and parses them" +
              " without committing any records. This will show any issues" +
              " which can be solved by mapping.");
            System.Console.WriteLine(
              @"/reprocess_referrals [[UBRN] [AttachmentId]]: Run the Process but reprocess " +
              "existing attachments. If UBRN " +
              "is provided, just that record will be processed.  If AttachmentId is provided, " +
              "just that attachment will be processed.");
            System.Console.WriteLine(@"/rereport [UBRN [AttachmentId]]: " +
              "Runs the process without committing any records to the hub. " +
              " re-reprocessing existing attachments." +
              "  If UBRN is provided, just that record will be processed." +
              "If AttachmentId is provided, just that attachment file will " +
              "be processed." +
              "Three diagnostic reports are produced - the final referral " +
              "object content, the complete mappings derived from the " +
              "attached file and then the content of the file unfiltered.  " +
              "This can be run several times as it is non-destructive.");
            System.Console.WriteLine(@"/test_file <filename> : Parse a " +
              "local document and output an analysis to aid troubleshooting" +
              " and implementation of new templates.");
            System.Console.WriteLine(@"/test_critical : exits with a " +
              "critical failure state.  used for testing");
            System.Console.WriteLine(@"/test_fail : exits with a " +
              "failure state.  used for testing");
            System.Console.WriteLine(@"/test_success : exits with a " +
              "success state.  used for testing");
            System.Console.WriteLine(@"/update_from_csv <filename> : Update" +
              " referral records from a CSV file.");
            System.Console.WriteLine(@"/process_batch <filename> : Loads a" +
              " csv batch file of UBRNs and processes them");
            System.Console.WriteLine(@"/report_batch <filename> : Loads a" +
              " csv batch file of UBRNs and parses them" +
              " without committing any records. This will show any issues" +
              " which can be solved by mapping.");
            System.Console.WriteLine(@"/report_discharge_referrals :" +
              " Process all referrals marked as awaiting discharge " +
              "in report mode.  The eRS validation is performed, but no" +
              "records are updated.");
            System.Console.WriteLine(
              @"/upload_missing_log <filename> <from> <to>: Uploads a " +
              "log file to the referral API using the optional from " +
              "and to dates to filter the timestamps of the log rows in " +
              "the file.");

            return (int)ExitCode.Success;
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error(ex, ex.Message);
      exitCode = (int)ExitCode.CriticalFailure;
    }
    finally
    {
      switch (exitCode)
      {
        case (int)ExitCode.Success:
          Log.Logger.Debug("Referrals Process Completed Successfully");
          break;
        case (int)ExitCode.Failure:
          Log.Logger.Debug("Referrals Process Completed With Errors");
          break;
        case (int)ExitCode.CriticalFailure:
          Log.Logger.Warning("Referrals Process Completed With " +
            "Critical Errors. Check detailed log to ensure all items were " +
            "processed.");
          break;
        default:
          Log.Logger.Warning("Referrals Process Completed with code " +
            $"{exitCode}");
          break;
      }

      Log.CloseAndFlush();
      SendEmailWithLog(exitCode);
    }
    return exitCode;
  }

  private void SendEmailWithLog(int exitCode)
  {
    if (exitCode == (int)ExitCode.Success)
    {
      // don't send anything when the run was successful
      return;
    }

    string body;
    MailPriority mailPriority;
    string subject = _configuration.GetValue<string>("Mustard:Email:AppName")
      ?? "WmsHub.ReferralsService.Console";
    string[] receipents = _configuration.GetSection("Mustard:Email:Recipients").Get<string[]>()
      ?? ["mlcsu.digitalinnovations@nhs.net"];

    if (exitCode == (int)ExitCode.Failure)
    {
      body = $"{subject} completed with non-critical errors.  All referrals were processed, " +
        $"although individual referrals may have reported an error. Exit code {exitCode}";
      mailPriority = MailPriority.Normal;
      subject += " Failure";
    }
    else
    {
      body = $"{subject} completed with critical errors. The process may not have completed." +
        $" Exit code {exitCode}";
      mailPriority = MailPriority.High;
      subject += " Critical Failure";
    }

    SerilogAppSettings serilogAppSettings = new(_configuration);
    Uri logUri = serilogAppSettings.GetSerilogFileUri();

    if (logUri != null)
    {
      _sendEmailService.SendEmail(
        body,
        receipents,
        subject,
        attachmentFilenames: [logUri.AbsolutePath],
        priority: mailPriority);
    }
  }

  private static bool CheckFolder(string folderPath)
  {
    if (string.IsNullOrWhiteSpace(folderPath))
    {
      Log.Logger.Fatal("A temporary folder definition is required in the " +
        "configuration file.  Cannot continue.");
      return false;
    }
    if (Directory.Exists(folderPath))
    {
      CleanupFolder(folderPath);
    }
    else
    {
      try
      {
        Directory.CreateDirectory(folderPath);
      }
      catch (Exception ex)
      {
        Log.Logger.Error(ex, "Failed to create temporary folder " +
          $"'{folderPath}'.");
        return false;
      }
    }
    return true;
  }

  private static void CleanupFolder(string folderPath)
  {
    KillAllInstancesOfWord();
    if (string.IsNullOrWhiteSpace(folderPath))
    {
      Log.Logger.Fatal("A temporary folder definition is required in the " +
        "configuration file.  Cannot continue.");
      return;
    }
    try
    {
      //Delete existing files from ISFE folder
      DirectoryInfo diIsfe = new(folderPath);
      foreach (FileInfo file in diIsfe.GetFiles())
      {
        try
        {
          file.Delete();
        }
        catch (Exception ex)
        {
          Log.Logger.Debug(ex, "Could not remove {filename} from " +
            $"the file cache.", file.FullName);
        }
      }
    }
    catch (Exception ex)
    {
      Log.Logger.Error(ex, "An error occurred when clearing the temporary" +
        "file cache.");
    }
  }

  private static void KillAllInstancesOfWord()
  {
    new Processor(Log.Logger).CloseWordSingletonIfItExists();

    Process[] processes = Process.GetProcessesByName("WINWORD");
    int counter = 0;
    foreach (Process p in processes)
    {
      if (!string.IsNullOrEmpty(p.ProcessName))
      {
        try
        {
          p.Kill();
          counter++;
        }
        catch (Exception ex)
        {
          Log.Logger.Error(ex, "Failed to kill an instance of Word.");
        }
      }
    }

    Log.Logger.Debug("Killed {counter} of {total} instance(s) of Word.",
      counter,
      processes.Length);
  }
}
