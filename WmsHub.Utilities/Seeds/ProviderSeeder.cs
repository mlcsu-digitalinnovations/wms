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
using WmsHub.Common.Helpers;

namespace WmsHub.Utilities.Seeds
{
  public class ProviderSeeder : SeederBase<Provider>
  {
    public static readonly Guid PROVIDER_API_USER_ID =
       new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41");

    public static Provider CreateProvider(
      bool isActive= true,
      DateTimeOffset modifiedAt = default,
      Guid modifiedByUserId = default,
      string name = "Provider-Name",
      string summary = "Summary",
      string website = "www.Provider-Name.co.uk",
      string logo = "Logo Name",
      bool level1 = true,
      bool level2 = false,
      bool level3 = false)
    {
      return new Provider()
      {
        IsActive = isActive,
        ModifiedAt = modifiedAt == default
          ? DateTimeOffset.Now
          : modifiedAt,
        ModifiedByUserId = modifiedByUserId == default
          ? PROVIDER_API_USER_ID
          : modifiedByUserId,
        Name = name,
        Summary= summary,
        Website = website,
        Logo = logo,
        Level1 = level1,
        Level2 = level2,
        Level3 = level3,
      };
    }

    internal static void SeedQueryPerformanceData(
      List<Guid> listProviderGuids)
    {
      int noOfRecords = listProviderGuids.Count;
      Stopwatch sw = new Stopwatch();
      sw.Start();

      Random random = new Random();
      List<Provider> providers = new List<Provider>(noOfRecords);
      
      for (
        int provLoopCount = 0; provLoopCount < noOfRecords; provLoopCount++)
      {
        Provider provider = CreateProvider(
          name: Generators.GenerateName(random, 6),
          summary: Generators.GenerateName(random, 6),
          website: Generators.GenerateName(random, 6),
          logo: Generators.GenerateName(random, 6),
          level1: true,
          level2: false,
          level3: false
          );
          
        provider.Id = listProviderGuids[provLoopCount];
        providers.Add(provider);
      }

      string[] columns = new string[] { "Id", "IsActive", "ModifiedAt",
        "ModifiedByUserId", "Name", "Summary", "Website",
        "Logo", "Level1", "Level2", "Level3" };

      using (IDataReader reader = ObjectReader.Create(providers, columns))
      using (SqlConnection connection =
        new SqlConnection(Config.GetConnectionString("WmsHub")))
      using (SqlBulkCopy bcp = new SqlBulkCopy(connection))
      {
        connection.Open();
        bcp.BulkCopyTimeout = 0;
        bcp.BatchSize = 1000;
        bcp.DestinationTableName = "[Providers]";
        bcp.WriteToServer(reader);
        connection.Close();
      }
      sw.Stop();
      Serilog.Log.Information("Added {count} Providers - took {seconds}s.",
        noOfRecords,
        sw.Elapsed.TotalSeconds);
    }

    internal static List<Provider> CreateProviders(int numberOfcreate)
    {
      List<Provider> providers = new(numberOfcreate);
      for(int i = 0; i < numberOfcreate; i++)
      {
        providers.Add(RandomEntityCreator.CreateRandomProvider());
      }

      DatabaseContext.Providers.AddRange(providers);
      DatabaseContext.SaveChanges();

      return providers;
    }

    public async static Task DeleteTestData(List<Guid> providerGuids)
    {
      List<Provider> providersToDelete = new List<Provider>();
      try 
      { 
        foreach (Guid providerGuid in providerGuids)
        {
          var provider = await SeederBaseBase.DatabaseContext.Providers
            .AsNoTracking()
            .Where(p => p.Id == providerGuid)
            .FirstOrDefaultAsync();

          if (provider != null)
            providersToDelete.Add(provider);
        }

        if (providersToDelete.Count > 0)
        {
          SeederBaseBase.DatabaseContext.Providers.
             RemoveRange(providersToDelete.ToArray());
        };
      }
      catch(Exception)
      {
        throw;
      }

      Serilog.Log.Information("Deleted Providers");
    }
  }
}