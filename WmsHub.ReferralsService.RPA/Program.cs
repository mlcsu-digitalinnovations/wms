using AwiSxRobotLib;
using IdentityAgentApi;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WmsHub.ReferralsService.Rpa.Configuration;

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

        //In order to keep the ticket fresh for the maximum time after
        //the process has run, log out first
        Log.Logger.Information("Smart Card Login Process Started");
        SetupGaTicket();
        LogOutSmartCard();

        await Task.Delay(5 * seconds);
        
        //Run the login process in the background and wait in order
        //to give the smart card login window time to appear
        Task.Run(() => GetSmartcardTicket());
        await Task.Delay(5 * seconds);

        //log into the smart card
        AutomateSmartCardLogin(
          _config.RpaSettings.SmartCardEmailAddress,
          _config.RpaSettings.SmartCardPassword,
          seconds
          );
        await Task.Delay(5 * seconds);
        
        //Confirm the login was completed correctly
        string getTicketTask = await GetSmartcardTicket();
        await Task.Delay(5 * seconds);

        //GaTicket opens various windows in Internet Explorer.  These will 
        //vary, from a Terms and Conditions page to surveys, messages etc.
        //To leave the system as it was found, all instances of
        //Internet Explorer will be killed.
        Kill_IE();

        if (string.IsNullOrWhiteSpace(getTicketTask))
        {
          //Ticket was not returned - throw error
          throw new Exception("Ticket was not returned from smart card.");
        } else
        {
          Log.Logger.Information("Smart Card Ticket Successfully Retrieved.");
          Console.WriteLine($"Ticket: {getTicketTask}");
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

    static void AutomateSmartCardLogin(string userName, string password, 
      int timeMultiplierSeconds)
    {
      const string APPLICATION_LOGIN = "iOAgent";
      const string PROFILE_WINDOW = "iO_Identity_Agent";
      const string TEXT_EMAIL = "edtEmail";
      const string TEXT_PASSWORD = "edtPassCode";
      const string BUTTON_SUBMIT = "btnSubmit";


      //RPA Process to detect login window
      short ret;
      Robot sx = new RobotClass();

      sx.Init();
      ret = sx.SetApplication(APPLICATION_LOGIN);
      var win = sx.Attach(PROFILE_WINDOW);

      sx.WaitForWindow(5*timeMultiplierSeconds, false);

      sx.RequestFocus(TEXT_EMAIL);
      sx.SetNamedValue(TEXT_EMAIL, userName);
      sx.SetNamedValue(TEXT_PASSWORD, password);

      sx.PressNamedButton(BUTTON_SUBMIT);

      sx.Wait(10*timeMultiplierSeconds);
      sx.UnloadApplication();
      Console.WriteLine($"{sx.GetErrorText()}");

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

     static void Kill_IE()
    {
      Process[] ps = Process.GetProcessesByName("IEXPLORE");

      foreach (Process p in ps)
      {
        p.Kill();
      }
    }
  }
}
