using Microsoft.Extensions.Configuration;
using Serilog;
using System;

namespace IdentityAgentApi.Console
{
  class Program
  {
    const string DEFAULTCONFIGPATH = "./appsettings.json";

    enum ExitCode : int
    {
      Success = 0,
      Failure = 1
    }

    static int Main(string[] args)
    {

      Configure();
      ExitCode exitCode = ExitCode.Success;

      try
      {
        GaTicket.Initialize(false);
        
        string ticket;
        int result = GaTicket.GetTicket(out ticket);
        string resultAsString = GaTicket.ReturnCodeAsString(result);

        Log.Information($"Ticket Retrieval Test");
        Log.Information(resultAsString);
        if (args.Length == 1) {
          if (args[0] == "/log_ticket")
          {
            string outputValue = "";
            if (ticket.Length > 27)
            {
              outputValue = $"...{ticket.Substring(11, 16)}...";
            }
            else
            {
              if (ticket.Length > 0)
              {
                outputValue = $"Possibly malformed ticket with a length " +
                  $"of {ticket.Length} characters.";
              }
            }
            Log.Information($"Ticket: {outputValue}");
          } 
        } 
        System.Console.WriteLine($"Ticket: {ticket}");

        if (result != 0) exitCode = ExitCode.Failure;
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "Exception");
        exitCode = ExitCode.Failure;
      }
      Log.CloseAndFlush();
      return (int)exitCode;
    }

    static void Configure()
    {
      IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile(DEFAULTCONFIGPATH)
        .Build();
      Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
#if DEBUG
        .WriteTo.Debug()
#endif 
        .CreateLogger();

    }
  }
}
