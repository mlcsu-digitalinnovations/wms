using IdentityAgentApi;
using Microsoft.Extensions.Configuration;
using NLog;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Automation;
using Winium.Cruciatus.Core;
using Winium.Cruciatus.Elements;
using WmsHub.ReferralsService.SmartCard.Configuration;

namespace WmsHub.ReferralsService.Rpa
{
  class Program
  {
    const string CONFIGPATH = "./appsettings.json";

    enum ExitCode : int
    {
      Success = 0,
      Failure = 1
    }

    static async Task<int> Main()
    {
      int exitCode = (int)ExitCode.Success;
      try
      {
        Config _config = Configure();
        int seconds = (int)(1000f * _config.RpaSettings.TimeDelayMultiplier);

        //Get the number of IE instances at the start in case one is open from
        //a previous iteration
        Process[] ps = Process.GetProcessesByName("IEXPLORE");
        int initialNumberOfIeInstances = ps.Length;

        //In order to keep the ticket fresh for the maximum time after
        //the process has run, log out first
        Log.Logger.Information("Smart Card Login Process Started");
        SetupGaTicket();
        LogOutSmartCard();

        await Task.Delay(5 * seconds);

        //Run the login process in the background
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(() => GetSmartcardTicket());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        //log into the smart card
        AutomateSmartCardLogin(
          _config.RpaSettings.SmartCardEmailAddress,
          _config.RpaSettings.SmartCardPassword,
          seconds, _config.RpaSettings.NumberOfConnectionAttempts,
          _config.RpaSettings.IsosecIoIdentityAgentName
          );
        int timeToWaitForLogin = 
          (int)(10 * seconds * _config.RpaSettings.TimeDelayMultiplier);
        await Task.Delay(timeToWaitForLogin);

        //GaTicket opens various windows in Internet Explorer.  These will 
        //vary, from a Terms and Conditions page to surveys, messages etc.
        //To leave the system as it was found, all instances of
        //Internet Explorer will be killed.
        if (Kill_IE(initialNumberOfIeInstances) == true)
        {
          Log.Logger.Information("Smart Card Login Process Complete");
        }
        else
        {
          Log.Logger.Error("Smart Card Login Process Failed.");
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
        if (exitCode == (int)ExitCode.Success)
        {
          Log.Logger.Information("Referrals Process Completed Successfully");
        }
        else
        {
          Log.Logger.Warning("Referrals Process Completed With Errors");
        }

        Log.CloseAndFlush();
      }

      return exitCode;
    }

    static void SetupGaTicket()
    {
      GaTicket.Initialize();
    }

    async static Task<string> GetSmartcardTicket()
    {
      
      string result;
      GaTicket.GetTicket(out result);

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
    static void AutomateSmartCardLogin(string userName, string password, 
      int timeMultiplierSeconds, float attemptsToFindControls, string path)
    {
      //By default, Winium produces logs containing sensitive information
      //(all text entered into text boxes). To avoid passwords being
      //shown in plain text, the logging is removed for production
      var config = new NLog.Config.LoggingConfiguration();
#if DEBUG
      var logconsole = new NLog.Targets.ConsoleTarget();
      config.AddTarget("logconsole", logconsole);
      config.LoggingRules.Add(
        new NLog.Config.LoggingRule("*", LogLevel.Info, logconsole));
#endif
      LogManager.Configuration = config;

      const string APPLICATION_WINDOW_NAME = "iO Identity Agent";
      const string TEXT_EMAIL = "Email:";
      const string TEXT_PASSWORD = "Passcode:";
      const string BUTTON_SUBMIT = "Submit";

      var app = new Winium.Cruciatus.Application(path);
      var winFinder = By.Name(APPLICATION_WINDOW_NAME)
        .AndType(ControlType.Window);

      CruciatusElement win = null;

      
      //The window will always be found unless the software has never run
      win = Winium.Cruciatus.CruciatusFactory.Root.FindElement(winFinder);
      if (win == null)
      {
        Log.Logger.Error("Smart card login window was not found.");
        return;
      }

      //Finding the first control will indicate whether the Isosec login window
      //is actually visible on-screen so multiple attempts will be made to find
      //it
      CruciatusElement emailTextBox = null;
      for (int i = 0; i < attemptsToFindControls; i++)
      {
        Log.Logger.Debug("Attempting to find window controls...");
        emailTextBox = win.FindElement(By.Name(TEXT_EMAIL)
          .AndType(ControlType.Edit));
        if (emailTextBox != null)
        {
          Log.Logger.Debug("Window controls found.");
          break;
        }
        Log.Logger.Debug($"Attempt {i+1} failed.");
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

    static void LogOutSmartCard()
    {
      GaTicket.DestroyTicket();
    }

    static Config Configure()
    {
      Config result = new Config();
      IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile(CONFIGPATH)
        .AddEnvironmentVariables("WmsHub.ReferralService_")
        .Build();
      configuration.Bind(result);
      Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
#if DEBUG
        .WriteTo.Debug()
#endif 
        .CreateLogger();

      return result;
    }

    static bool Kill_IE(int expectedIEInstances)
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
