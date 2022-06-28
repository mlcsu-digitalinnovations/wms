using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WmsHub.Business.Entities;

namespace WmsHub.Utilities.Seeds
{
  public class Seeders
  {
    public static void ForDevelopment()
    {
      RemoveSeedAll();
      ReferralSeeder.SeedDevelopmentData();
    }

    public static void RemoveSeedAll()
    {
      new CallSeeder().DeleteSeeds();
      new ReferralSeeder().DeleteSeeds();
      new TextMessageSeeder().DeleteSeeds();
      SeederBaseBase.DatabaseContext.SaveChanges();
    }

    public async static Task ForQueryPerformance(int noOfRecords)
    {
      Stopwatch sw = new Stopwatch();
      sw.Start();

      try
      {
        List<Guid> guids = await ReferralSeeder.DeleteTestData();
        //await ProviderSeeder.DeleteTestData(guids);
        //await ProviderSubmissionSeeder.DeleteTestData(guids);
        SeederBaseBase.DatabaseContext.SaveChanges();

        Serilog.Log.Information("Start Table Data Population.");
        ReferralSeeder.SeedQueryPerformanceData(noOfRecords);
        //ProviderSeeder.SeedQueryPerformanceData(providerGuidIds);
        //ProviderSubmissionSeeder.SeedQueryPerformanceData(providerGuidIds);        
      }
      catch (Exception exc)
      {
        Serilog.Log.Information("Error - " + exc.Message);
        Serilog.Log.Information("Rectify the issue and try to repopulate.");
        Serilog.Log.Information("Press any Key to exit");
        Console.ReadKey();
        return;
      }
      sw.Stop();
      Serilog.Log.Information("Data Setup Complete - QueryPerformance- took " +
        "{seconds}s to add all test data.",
        sw.Elapsed.TotalSeconds,
        noOfRecords);
      
      Exit();
    }

    internal static void OneReferralForEachStatus()
    {
      Serilog.Log.Information("Deleting referral seeds.");
      new ReferralSeeder().DeleteSeeds();
      Serilog.Log.Information("Start Table Data Population.");

      int numReferralsCreated = ReferralSeeder.CreateOneReferralForEachStatus();

      Serilog.Log.Information($"Created {numReferralsCreated} referrals.");

      Exit();
    }

    internal async static Task ReferralsWithProvidersAndSubmissions(
      int noOfRecords, int noOfProviders, int noOfSubmissions)
    {
      try
      {
        List<Guid> guids = await ReferralSeeder.DeleteTestData();
        await ProviderSeeder.DeleteTestData(guids);
        await ProviderSubmissionSeeder.DeleteTestData(guids);

        Serilog.Log.Information("Start Table Data Population.");

        List<Provider> providers = ProviderSeeder
          .CreateProviders(noOfProviders);

        List<Referral> referrals = ReferralSeeder
          .CreateReferralsWithProviderStartedStatus(noOfRecords, providers);

        ProviderSubmissionSeeder.CreateSubmissionsForReferrals(
          referrals, noOfSubmissions);
      }
      catch (Exception exc)
      {
        Serilog.Log.Information("Error - " + exc.Message);
        if (exc.InnerException != null)
        {
          Serilog.Log.Information("Error - " + exc.InnerException.Message);
        }
        Serilog.Log.Information("Rectify the issue and try to repopulate.");
        Serilog.Log.Information("Press any Key to exit");
        Console.ReadKey();
        return;
      }

      Serilog.Log.Information("Data Setup Complete. Created {noOfRecords} " +
        "referrals. ",
        noOfRecords);

      Exit();
    }

    internal async static Task ForReferralDischarge()
    {
      int noOfReferrals = 0;
      try
      {
        List<Guid> guids = await ReferralSeeder.DeleteTestData();
        await ProviderSeeder.DeleteTestData(guids);
        await ProviderSubmissionSeeder.DeleteTestData(guids);

        Serilog.Log.Information("Start Table Data Population.");

        List<Provider> providers = ProviderSeeder.CreateProviders(1);

        noOfReferrals = ReferralSeeder.CreateDischargeReferrals(providers[0]);

      }
      catch (Exception exc)
      {
        HandleException(exc);
      }

      Serilog.Log.Information("Data Setup Complete. Created {noOfReferrals} " +
        "referrals. ",
        noOfReferrals);
    }

    private static void HandleException(Exception exc)
    {
      Serilog.Log.Information("Error - " + exc.Message);
      if (exc.InnerException != null)
      {
        Serilog.Log.Information("Error - " + exc.InnerException.Message);
      }
      Serilog.Log.Information("Rectify the issue and try to repopulate.");
      Serilog.Log.Information("Press any Key to exit");
      Console.ReadKey();
      return;
    }

    internal static async Task TextMessages(int noOfRecords)
    {
      try
      {
        Serilog.Log.Information("Deleting text message test data.");
        TextMessageSeeder.DeleteTestData();

        Serilog.Log.Information("Starting text message population.");
        await TextMessageSeeder.CreateTextMessages(noOfRecords);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
      Serilog.Log.Information("Data Setup Complete. Created {noOfReferrals} " +
        "text messages. ",
        noOfRecords);
    }

    private static void Exit()
    {
      Serilog.Log.Information("Press Key to exit");
      Console.ReadKey();
    }
  }
}