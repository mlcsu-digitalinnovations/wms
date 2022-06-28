using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Utilities.Seeds;

namespace WmsHub.Utilities
{
  class Program
  {
    
    static async Task Main(string[] args)
    {
      IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false, true)
        .AddCommandLine(args)
        .AddEnvironmentVariables(prefix: "WmsHub.Utilities_")
        .AddEnvironmentVariables()
        .Build();

      Log.Logger = new LoggerConfiguration()
          .ReadFrom.Configuration(config)
          .CreateLogger();

      // This is accessible by convention from the environmental variable
      // SQLCONNSTR_WmsHub
      DbContextOptions<DatabaseContext> options =
        new DbContextOptionsBuilder<DatabaseContext>()
				.UseSqlServer(Environment.GetEnvironmentVariable("SQLCONNSTR_WmsHub"))
        .EnableDetailedErrors(true)
        .EnableSensitiveDataLogging(true)
        .Options;

      SeederBaseBase.Config = config;

      DatabaseContext databaseContext = new DatabaseContext(options);
      SeederBaseBase.DatabaseContext = databaseContext;

      if (args.Length > 0)
      {
        if (config["aduserfile"] != null)
        {
          Log.Information($"Converting AD user file {config["aduserfile"]}.");
          Converters.AdUserFileConverter.ConvertCsv(config["aduserfile"]);
        }
        else if (config["pc"] != null)
        {
          Log.Information($"Testing with postcodefile {config["pc"]}.");
          Testing.Postcodes.RunTestFile(config["pc"]);
        }
        else if (config["seed"] == "dev")
        {
          Log.Information("Seeding database with development data.");
          Seeders.ForDevelopment();
        }
        else if (config["seed"] == "status")
        {
          Log.Information("Seeding database with a referral for each status.");
          Seeders.OneReferralForEachStatus();
        }
        else if (config["seed"] == "prov")
        {
          if (config["no"] == null)
          {
            Log.Error("The /no argument is also required.");
          }
          else
          {
            if (int.TryParse(config["no"], out int noOfRecords))
            {
              Log.Information("Seeding database with referrals with provider " +
                "and submission data.");
              await Seeders.ReferralsWithProvidersAndSubmissions(
                noOfRecords, 6, 25);
            }
            else
            {
              Log.Error("The /no argument has to be numeric.");
            }
          }          
        }
        else if (config["seed"] == "qperf")
        {
          if (config["no"] == null)
          {
            Log.Error("The /no argument is also required.");
          }
          else
          {
            if (int.TryParse(config["no"], out int noOfRecords))
            {
              Log.Information("Seeding database with query performance data.");
              await Seeders.ForQueryPerformance(noOfRecords);
            }
            else
            {
              Log.Error("The /no argument has to be numeric.");
            }
          }
        }
        else if (config["seed"] == "sms")
        {
          if (config["no"] == null)
          {
            Log.Error("The /no argument is also required.");
          }
          else
          {
            if (int.TryParse(config["no"], out int noOfRecords))
            {
              Log.Information(
                $"Seeding database with {noOfRecords} text messages.");
              await Seeders.TextMessages(noOfRecords);
            }
            else
            {
              Log.Error("The /no argument has to be numeric.");
            }
          }
        }
        else if (config["seed"] == "discharge")
        {
          Log.Information("Seeding database with referral discharge data.");
          await Seeders.ForReferralDischarge();
        }
        else if (config["rmseed"] == "all")
        {
          Log.Information("Removing all seed data from database.");
          Seeders.RemoveSeedAll();
        }
        else if (config["test"] == "qperf")
        {
          Log.Information("Query performace testing.");
          await new QueryPerformanceTests(databaseContext).RunAllTests();
        }
        else
        {
          ShowHelp();
        }
      }
      else
      {
        ShowHelp();
      }
    }

    private static void ShowHelp()
    {
      Console.WriteLine(
        "Missing arguments, available arguments are:" + Environment.NewLine +
        "  Test Postcodes:" + Environment.NewLine +
        "    /pc={filename}:    Seed database with development data." +
        "  Adding Seeds:" + Environment.NewLine +
        "    /seed=dev:    Seed database with development data." +
        "    /seed=status: Seeding database with a referral for each status." +
        Environment.NewLine +
        "    /seed=qperf:  Seed database with query performance data." +
        Environment.NewLine +
        "      /no=<number>: Number of records for query performance data." +
        Environment.NewLine +
        "    /seed=prov: Seed database with referrals with providers selected" +
          " and provider submissions." +
        Environment.NewLine +
        "      /no=<number>: Number of referrals." +
        Environment.NewLine +
        "  Removing Seeds:" + Environment.NewLine +
        "    /rmseed=all: Remove all seed data from database." +
        Environment.NewLine +
        "  Query Performance Testing:" + Environment.NewLine +
        "    /test=qperf: Run the query performance tests." +
        Environment.NewLine

      );
    }
  }
}
