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

namespace WmsHub.Utilities.Seeds;

public class ProviderSubmissionSeeder : SeederBase<ProviderSubmission>
{
  public static readonly Guid PROVIDER_SUBMISSION_API_USER_ID =
    new("fafc7655-89b7-42a3-bdf7-c57c72cd1d41");
  public static readonly int PROVIDER_SUBMISSION_MIN_DATASET =
    int.Parse(Config["SeedSettings:ProvSubCount"]);

  public static ProviderSubmission CreateProviderSubmission(
    bool isActive = true,
    DateTimeOffset modifiedAt = default,
    Guid modifiedByUserId = default,
    int coaching = 1,
    DateTimeOffset date = default,
    int measure = 1,
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
    Stopwatch sw = new();
    sw.Start();

    Random random = new();
    List<ProviderSubmission> providersSubmissions = new(noProvSubRecords);

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
          weight: random.Next(weightVariancePoint - 10,
            weightVariancePoint + 10)
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
        new(Config.GetConnectionString("WmsHub")))
      using (SqlBulkCopy bcp = new(connection))
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

  internal static void CreateSubmissionsForReferrals(List<Referral> referrals, int noOfSubmissions)
  {
    int daysBetweenSubmissions = 91 / noOfSubmissions;
    int createdProviderSubmissions = 0;

    foreach (Referral referral in referrals)
    {
      for (int i = 0; i < noOfSubmissions; i++)
      {
        createdProviderSubmissions++;
        DateTimeOffset date =
          referral.DateOfReferral.Value.AddDays((i * daysBetweenSubmissions) + 1);

        DatabaseContext.ProviderSubmissions.Add(RandomEntityCreator.CreateProviderSubmission(
          coaching: i + 1,
          date: date,
          isActive: true,
          measure: (i + 1) * 100,
          modifiedAt: date,
          providerId: referral.ProviderId.Value,
          referralId: referral.Id,
          weight: 100 - i));
      }
    }
    DatabaseContext.SaveChanges();
    Serilog.Log.Information(
      "Created {CreatedProviderSubmissions} provider submissions.",
      createdProviderSubmissions);
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

  public async static Task DeleteTestData()
  {

    ProviderSubmission[] providerSubmissionsToDelete = await DatabaseContext
      .ProviderSubmissions
      .Where(p => p.Provider.Name.StartsWith(ProviderSeeder.SeededProviderNamePrefix))
      .ToArrayAsync();

    if (providerSubmissionsToDelete.Length > 0)
    {
      DatabaseContext.ProviderSubmissions.RemoveRange([.. providerSubmissionsToDelete]);
    }

    Serilog.Log.Information(
      "{ProviderSubmissionsToDelete} provider submissions set to be deleted.",
      providerSubmissionsToDelete.Length);
  }
}
