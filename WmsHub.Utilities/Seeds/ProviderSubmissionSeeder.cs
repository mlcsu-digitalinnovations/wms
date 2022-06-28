using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Helpers;
using WmsHub.Common.Extensions;

namespace WmsHub.Utilities.Seeds
{
  public class ProviderSubmissionSeeder : SeederBase<ProviderSubmission>
  {
    public static readonly Guid PROVIDER_SUBMISSION_API_USER_ID = 
      new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41");
    public static readonly int PROVIDER_SUBMISSION_MIN_DATASET = 
      int.Parse(Config["SeedSettings:ProvSubCount"]);

    public static ProviderSubmission CreateProviderSubmission(
      bool isActive = true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      int coaching = 1,
      DateTimeOffset date = default,
      int measure= 1,
      decimal weight = 78
      )
    {
      return new ProviderSubmission()
      {
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? PROVIDER_SUBMISSION_API_USER_ID
          : modifiedByUserId,
        Coaching = coaching,
        Date = date,
        Measure = measure,
        Weight = weight
      };
    }

    internal static void SeedQueryPerformanceData(List<Guid> guids)
    {
      int provGuidCount = guids.Count;
      int noProvSubRecords = PROVIDER_SUBMISSION_MIN_DATASET;
      Stopwatch sw = new Stopwatch();
      sw.Start();

      Random random = new Random();
      List<ProviderSubmission> providersSubmissions = new 
        List<ProviderSubmission>(noProvSubRecords);

      //ProviderGuid list loop - each loop add ProviderSubmissions
      for (int guidloopCount = 0; guidloopCount < provGuidCount; 
        guidloopCount++)
      {
        //clear any old records
        providersSubmissions.Clear();
        int weightVariancePoint = random.Next(60, 160);

        //each loop add new  set number of ProviderSubmissions
        for (int provSubCount = 0; provSubCount < noProvSubRecords; 
          provSubCount++)
        {
          int dateDays = -200 + guidloopCount;
          ProviderSubmission providerSubmission = CreateProviderSubmission(
            coaching: random.Next(1, 6),
            date: DateTime.Now.AddDays(dateDays),
            measure: random.Next(0, 100),
            weight: (decimal)random.Next((weightVariancePoint - 10), 
              (weightVariancePoint + 10))
          );

          providerSubmission.Id = Guid.NewGuid();
          providerSubmission.ProviderId = guids[guidloopCount];
          providersSubmissions.Add(providerSubmission);
        }

        string[] columns = new string[] { "Id", "IsActive", "ModifiedAt",
        "ModifiedByUserId", "ProviderId", "Coaching", "Date",
        "Measure", "Weight"};

        using (IDataReader reader = ObjectReader.Create(providersSubmissions, 
          columns))
        using (SqlConnection connection =
          new SqlConnection(Config.GetConnectionString("WmsHub")))
        using (SqlBulkCopy bcp = new SqlBulkCopy(connection))
        {
          connection.Open();
          bcp.BulkCopyTimeout = 0;
          bcp.BatchSize = 1000;
          bcp.DestinationTableName = "[ProviderSubmissions]";
          bcp.WriteToServer(reader);
          connection.Close();
        }

        sw.Stop();
      }

      Serilog.Log.Information("Added [{0}] Provider Submissions. Took {1}s.",
        noProvSubRecords,
        sw.Elapsed.TotalSeconds);
    }

    internal static void CreateSubmissionsForReferrals(
      List<Referral> referrals, int noOfSubmissions)
    {
      foreach (Referral referral in referrals)
      {
        for (int i = 0; i < noOfSubmissions; i++)
        {
          DatabaseContext.ProviderSubmissions.Add(
            RandomEntityCreator.CreateProviderSubmission(
              date: referral.DateOfReferral.Value,
              providerId: referral.ProviderId.Value,
              referralId: referral.Id));
        }
      }
      DatabaseContext.SaveChanges();
    }

    internal static void CreateSubmissionsForReferralDischarge(
      Referral referral)
    {
      DatabaseContext.ProviderSubmissions.Add(
        RandomEntityCreator.CreateProviderSubmission(
          coaching: 1,
          date: referral.DateOfProviderSelection.Value.AddDays(1),
          providerId: referral.ProviderId.Value,
          referralId: referral.Id,
          measure: 0,
          weight: 95));

      DatabaseContext.SaveChanges();
    }

    public async static Task DeleteTestData(List<Guid> providerGuids)
    {
      List<ProviderSubmission> providerSubsToDelete = 
        new List<ProviderSubmission>();

      try
      {
        foreach (Guid providerGuid in providerGuids)
        {
          var providerSub = 
            await SeederBaseBase.DatabaseContext.ProviderSubmissions
            .AsNoTracking()
            .Where(p => p.ProviderId == providerGuid)
            .FirstOrDefaultAsync();

          if(providerSub != null)
            providerSubsToDelete.Add(providerSub);
        }

        if (providerSubsToDelete.Count > 0)
        {
          SeederBaseBase.DatabaseContext.ProviderSubmissions.
            RemoveRange(providerSubsToDelete.ToArray());
        }
      }
      catch(Exception)
      {
        throw;
      }

      Serilog.Log.Information("Deleted Provider Submissions");
    }
  }
}
