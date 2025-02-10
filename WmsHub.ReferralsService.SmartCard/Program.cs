using IdentityAgentApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Mlcsu.Diu.Mustard.Email;
using Mlcsu.Diu.Mustard.Logging.Serilog;
using NLog;
using NLog.Config;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Automation;
using Winium.Cruciatus;
using Winium.Cruciatus.Core;
using Winium.Cruciatus.Elements;
using WmsHub.ReferralsService.SmartCard.Configuration;
using WmsHub.ReferralsService.SmartCard.Properties;

namespace WmsHub.ReferralsService.Rpa
{
  public class Program
  {
    private const string CONFIGPATH = "./appsettings.json";

    private readonly static Config s_config = new Config();
    private static IConfiguration s_configuration;
    private readonly static ProcessStatusServiceOptions s_processStatusServiceOptions =
      new ProcessStatusServiceOptions();
    private static ILogger<ProcessStatusService> s_microsoftLogger;
    private static IProcessStatusService s_processStatusService;

    // static ISendEmailService emailService;
    private enum ExitCode : int
    {
      Success = 0,
      Failure = 1,
      CertificateUpdateRequired = 2
    }

    public static async Task<int> Main(string[] args)
    {
      int exitCode = 1;
      int attemptsToLogin = 1;
      try
      {
        Configure();

        if (args.Length == 1 && args[0].ToLower() == "/test_failure")
        {
          throw new InvalidOperationException("Test Failure");
        }

        try
        {
          await s_processStatusService.StartedAsync();
        }
        catch (Exception ex)
        {
          Log.Error(ex, "Failed to set Dashboard message to 'Started'");
        }

        for (attemptsToLogin = 1; attemptsToLogin <= s_config.AttemptsToLogin; attemptsToLogin++)
        {
          exitCode = await LoginToSmartCardAndGetGaTicket();
          if (exitCode == 0)
          {
            break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Smartcard login failed.");
      }
      finally
      {
        if (exitCode == 0)
        {
          Log.Information("Successfully logged after {Attempts} attempts.", attemptsToLogin);

          try
          {
            await s_processStatusService.SuccessAsync();
          }
          catch(Exception ex)
          {
            Log.Error(ex, "Failed to set Dashboard message to 'Success'");
          }
        }
        else
        {
          Log.Error(
            "Failed to login after {Attempts} attempts. Exit code {ExitCode}.",
            attemptsToLogin,
            exitCode);
          try
          {
            await s_processStatusService.FailureAsync(
              $"Failed to login after {attemptsToLogin} attempts. Exit code {exitCode}.");
          }
          catch (Exception ex)
          {
            Log.Error(ex, "Failed to set Dashboard message to 'Failure'");
          }
        }

        Log.CloseAndFlush();
        SendEmailWithLog(exitCode);
      }

      return exitCode;
    }

    private static async Task<int> LoginToSmartCardAndGetGaTicket()
    {
      int exitCode = (int)ExitCode.Failure;
      try
      {
        Log.Logger.Information("Smart Card Login Process Started");
        
        HandleServerUnavailableDialog(s_config.RpaSettings.BlockingDialogServerUnavailable);

        int seconds = (int)(1000f * s_config.RpaSettings.TimeDelayMultiplier);

        //Get the number of IE instances at the start in case one is open from
        //a previous iteration
        Process[] ps = Process.GetProcessesByName("IEXPLORE");
        int initialNumberOfIeInstances = ps.Length;

        //In order to keep the ticket fresh for the maximum time after
        //the process has run, log out first        
        SetupGaTicket();
        LogOutSmartCard();

        await Task.Delay(5 * seconds);

        //Run the login process in the background
#pragma warning disable CS4014 // Because this call is not awaited, 
        //execution of the current method continues before the call is
        //completed
        Task.Run(() => GetSmartcardTicket());
#pragma warning restore CS4014 // Because this call is not awaited, 
        //execution of the current method continues before the call is
        //completed

        //log into the smart card
        AutomateSmartCardLogin(
          s_config.RpaSettings.SmartCardEmailAddress,
          s_config.RpaSettings.SmartCardPassword,
          seconds, s_config.RpaSettings.NumberOfConnectionAttempts,
          s_config.RpaSettings.IsosecIoIdentityAgentName
          );
        int timeToWaitForLogin =
          (int)(10 * seconds * s_config.RpaSettings.TimeDelayMultiplier);
        await Task.Delay(timeToWaitForLogin);

        exitCode = HandleCertificatePopup();

        await Task.Delay(timeToWaitForLogin);

        //GaTicket opens various windows in Internet Explorer.  These will 
        //vary, from a Terms and Conditions page to surveys, messages etc.
        //To leave the system as it was found, all instances of
        //Internet Explorer will be killed.
        if (Kill_IE(initialNumberOfIeInstances) == true)
        {
          exitCode = (int)ExitCode.Success;
        }
        else
        {
          exitCode = (int)ExitCode.Failure;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, ex.Message);
        exitCode = (int)ExitCode.Failure;
      }
      finally
      {
        HandleServerUnavailableDialog(s_config.RpaSettings.BlockingDialogServerUnavailable);
        if (exitCode == (int)ExitCode.Success)
        {
          Log.Logger.Information("Smart Card Login Process Complete");
        }
        else
        {
          Log.Logger.Error("Smart Card Login Process Failed.");
        }
      }

      return exitCode;
    }

    private static void SendEmailWithLog(int exitCode)
    {
      if (exitCode == (int)ExitCode.Success)
      {
        // don't send anything when the run was successful
        return;
      }

      string body;
      MailPriority mailPriority;
      string subject = s_configuration.GetValue<string>("Mustard:Email:AppName")
        ?? "WmsHub.ReferralsService.SmartCard.Every3Hours";
      string[] receipents = s_configuration.GetSection("Mustard:Email:Recipients").Get<string[]>()
        ?? new string[] { "mlcsu.digitalinnovations@nhs.net" };

      body = $"{subject} Failed. Exit code {exitCode}.";
      mailPriority = MailPriority.High;
      subject += " Failure";

      SerilogAppSettings serilogAppSettings = new SerilogAppSettings(s_configuration);
      Uri logUri = serilogAppSettings.GetSerilogFileUri();

      if (logUri != null)
      {
        new SendEmailService(s_configuration).SendEmail(
          body,
          receipents,
          subject,
          attachmentFilenames: new string[] { logUri.AbsolutePath },
          priority: mailPriority);
      }
    }

    private static void SetupGaTicket() => GaTicket.Initialize();

    // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CS1998
    private static async Task<string> GetSmartcardTicket()
#pragma warning restore CS1998
    // Async method lacks 'await' operators and will run synchronously
    {
      GaTicket.GetTicket(out string result);

      return result;
    }

    /// <summary>
    /// RPA process for logging into the smart card
    /// </summary>
    /// <param name="userName">User name entry.</param>
    /// <param name="password">Passcode entry.</param>
    /// <param name="timeMultiplierSeconds">Time multiplier to allow for
    /// extra configurable delays.</param>
    /// <param name="attemptsToFindControls">Number of attempts before
    /// abandoning the search for the login controls.</param>
    /// <param name="path">The path of the Isosec application.</param>
    private static void AutomateSmartCardLogin(string userName, string password,
      int timeMultiplierSeconds, float attemptsToFindControls, string path)
    {
      const string APPLICATION_WINDOW_NAME = "iO Identity Agent";
      const string TEXT_EMAIL = "Email:";
      const string TEXT_PASSWORD = "Passcode:";
      const string BUTTON_SUBMIT = "Submit";

      Application app = new Application(path);
      ByProperty winFinder = By.Name(APPLICATION_WINDOW_NAME)
        .AndType(ControlType.Window);

      CruciatusElement win = null;

      LoggingConfiguration config = new NLog.Config.LoggingConfiguration();

#if DEBUG
      NLog.Targets.ConsoleTarget logconsole = new NLog.Targets.ConsoleTarget();
      config.AddTarget("logconsole", logconsole);
      config.LoggingRules.Add(
        new NLog.Config.LoggingRule("*", NLog.LogLevel.Info, logconsole));
#endif

      LogManager.Configuration = config;
            
      //Finding the first control will indicate whether the Isosec login window
      //is actually visible on-screen so multiple attempts will be made to find
      //it
      CruciatusElement emailTextBox = null;
      for (int i = 0; i < attemptsToFindControls; i++)
      {
        //The window will always be found unless the software has never run
        win = CruciatusFactory.Root.FindElement(winFinder);
        if (win == null)
        {
          Log.Logger.Error("Smart card login window was not found.");
        }
        else
        {
          Log.Logger.Debug("Attempting to find window controls...");
          emailTextBox = win.FindElement(By.Name(TEXT_EMAIL)
            .AndType(ControlType.Edit));
          if (emailTextBox != null)
          {
            Log.Logger.Debug("Window controls found.");
            break;
          }
        }

        Log.Logger.Debug($"Attempt {i + 1} failed.");
      }

      if (emailTextBox == null)
      {
        Log.Logger.Error("Email text box control was not found. The Login" +
          " window may not have opened correctly.");
        return;
      }

      emailTextBox.SetText(userName);

      CruciatusElement passTextBox = win.FindElement(By.Name(TEXT_PASSWORD)
        .AndType(ControlType.Edit));
      if (passTextBox == null)
      {
        Log.Logger.Error("Passcode text box control was not found.");
        return;
      }

      passTextBox.SetText(password);

      CruciatusElement submitButton = win.FindElement(By.Name(BUTTON_SUBMIT)
        .AndType(ControlType.Button));
      if (submitButton == null)
      {
        Log.Logger.Error("Submit button control was not found.");
        return;
      }

      submitButton.Click();
    }

    private static int HandleCertificatePopup()
    {
      string message = string.Empty;
      AutomationElement rootElement = AutomationElement.RootElement;

      AutomationElement dialogElement =
        rootElement.FindFirst(
          TreeScope.Children,
          new PropertyCondition(
            AutomationElement.ClassNameProperty,
            Resources.DIALOG_POPUP));

      if (dialogElement != null)
      {
        AutomationElement textElement =
          dialogElement.FindFirst(
            TreeScope.Children,
            new PropertyCondition(
              AutomationElement.AutomationIdProperty,
              Resources.TEXT_FRAME));
        AutomationElement buttonElement =
          dialogElement.FindFirst(
            TreeScope.Children,
            new PropertyCondition(
              AutomationElement.AutomationIdProperty,
              Resources.NO_BUTTON));

        if (textElement != null)
        {
          message = textElement.Current.Name;
          Console.WriteLine($"Name: <<{message}>>");
          string[] textSplit = textElement.Current.Name.Split(' ');
          foreach (string text in textSplit)
          {
            if (DateTime.TryParse(text, out DateTime d))
            {
              Console.WriteLine($"{d:D}");
            }
          }
        }

        if (textElement != null && buttonElement != null)
        {
          CruciatusElement button =
            CruciatusFactory.Root.FindElementByUid(
              buttonElement.Current.AutomationId);
          button.Click();

          return (int)ExitCode.CertificateUpdateRequired;
        }
      }

      return (int)ExitCode.Success;
    }

    private static void HandleServerUnavailableDialog(DialogDetails dialogDetails)
    {
      if (dialogDetails == null)
      {
        Log.Information("Server Unavailable popup has not been defined in the configuration.  " +
          "Server unavailable popup was not handled.");
        return;
      }

      if (string.IsNullOrEmpty(dialogDetails.Title) || 
          string.IsNullOrEmpty(dialogDetails.ButtonToClickText))
      {
        Log.Information("Configuration for Server Unavailable popup is not complete. Dialog " +
          "title is set to '{title}' and button to click text is set to '{buttontext}'.  " +
          "Server unavailable popup was not handled.",
          dialogDetails.Title, dialogDetails.ButtonToClickText);
        return;
      }

      try
      {
        CruciatusElement popupWindow = 
          CruciatusFactory.Root.FindElementByName(dialogDetails.Title);

        if (popupWindow != null)
        {
          Log.Warning("Server Unavailable popup has been found.");
          CruciatusElement button = popupWindow.FindElementByName(dialogDetails.ButtonToClickText);

          if (button != null)
          {
            popupWindow.SetFocus();
            button.SetFocus();
            button.Click();
            Log.Information("Server Unavailable dialog box button has been clicked.");
          }
        }
      }
      catch (Exception ex) 
      {
        //An exception here may not be critical so we will just log the exception here.
        Log.Logger.Error(ex, "Exception while attempting to handle the Server Unavailable dialog");
      }
    }

    private static void LogOutSmartCard() => GaTicket.DestroyTicket();

    private static void Configure()
    {
      s_configuration = new ConfigurationBuilder()
        .AddJsonFile(CONFIGPATH)
        .AddEnvironmentVariables("WmsHub.ReferralService.SmartCard_")
        .Build();

      s_configuration.Bind(s_config);

      Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(s_configuration)
        .CreateLogger();

      s_microsoftLogger = new SerilogLoggerFactory(Log.Logger).CreateLogger<ProcessStatusService>();

      s_configuration
        .Bind(ProcessStatusServiceOptions.ConfigSectionPath, s_processStatusServiceOptions);

      s_processStatusService = new ProcessStatusService(
        new System.Net.Http.HttpClient(),
        s_microsoftLogger,
        Options.Create(s_processStatusServiceOptions));
    }

    private static bool Kill_IE(int expectedIEInstances)
    {
      bool result = true;
      Process[] ps = Process.GetProcessesByName("IEXPLORE");
      if (ps.Length > 0)
      {

        if (ps.Length == expectedIEInstances)
        {
          //No new instances of IE were found so report as a fail but kill the
          //instances anyway
          result = false;
        }

        foreach (Process p in ps)
        {
          try
          {
            p.Kill();
          }
          catch (Exception ex)
          {
            Log.Logger.Warning(ex, "Could not kill IE Instance");
          }
        }
      }
      else
      {
        result = false;
      }

      return result;
    }
  }
}
