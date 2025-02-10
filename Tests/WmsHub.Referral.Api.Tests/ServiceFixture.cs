using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests;

[CollectionDefinition("Service collection")]
public class ServiceCollection : ICollectionFixture<ServiceFixture>
{
  // This class has no code, and is never created. Its purpose is simply
  // to be the place to apply [CollectionDefinition] and all the
  // ICollectionFixture<> interfaces.
}

public class ServiceFixture : AServiceFixtureBase
{
  public IMapper Mapper { get; set; }

  public DbContextOptions<DatabaseContext> Options { get; private set; }

  public ServiceFixture()
  {
    MapperConfiguration mapperConfiguration = new(cfg =>
      cfg.AddMaps(new[] {
        "WmsHub.Business",
        "WmsHub.Referral.Api"
      })
    );

    Mapper = mapperConfiguration.CreateMapper();

    EnvironmentVariableConfigurator
      .ConfigureEnvironmentVariablesForAlwaysEncrypted();

    Options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(databaseName: "WmsHub_Referral")
      .Options;

    CreateDatabase();
  }

  private void CreateDatabase()
  {
    using DatabaseContext dbContext = new(Options);

    PopulatePatientTriageService(dbContext);
    PopulateEthnicities(dbContext);
  }
  public void PopulatePatientTriageService(DatabaseContext ctx)
  {
    if (ctx.PatientTriages.Any())
    {
      return;
    }

    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age65to69",
      Descriptions = "Age group 65-69",
      Value = 1,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MaximumPossibleScoreCompletion",
      Descriptions = "Maximum Possible Score Completion",
      Value = 24,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MediumCategoryLowScoreWeight",
      Descriptions = "Medium Category Low Score Weight",
      Value = 15,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age65to69",
      Descriptions = "Age group 65-69",
      Value = 1,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationWeightData",
      Key = "IMD4",
      Descriptions = "IMD4",
      Value = 2,
      CheckSum = 17
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age70to74",
      Descriptions = "Age group 70-74",
      Value = 2,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MediumCategoryHighScoreCompletion",
      Descriptions = "Medium Category High Score Completion",
      Value = 16,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age60to64",
      Descriptions = "Age group 60-64",
      Value = 4,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age40to44",
      Descriptions = "Age group 40-44",
      Value = 10,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationCompletionData",
      Key = "IMD2",
      Descriptions = "IMD2",
      Value = 2,
      CheckSum = 11
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age70to74",
      Descriptions = "Age group 70-74",
      Value = 1,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age50to54",
      Descriptions = "Age group 50-54",
      Value = 6,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationWeightData",
      Key = "IMD3",
      Descriptions = "IMD3",
      Value = 4,
      CheckSum = 17
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityCompletionData",
      Key = "Mixed",
      Descriptions = "Mixed",
      Value = 4,
      CheckSum = 16
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityWeightData",
      Key = "Asian",
      Descriptions = "Asian",
      Value = 13,
      CheckSum = 33
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MinimumPossibleScoreCompletion",
      Descriptions = "Minimum Possible Score Completion",
      Value = 4,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationCompletionData",
      Key = "IMD3",
      Descriptions = "IMD3",
      Value = 2,
      CheckSum = 11
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexCompletionData",
      Key = "Female",
      Descriptions = "Female",
      Value = 1,
      CheckSum = 2
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age45to49",
      Descriptions = "Age group 45-49",
      Value = 10,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationCompletionData",
      Key = "IMD5",
      Descriptions = "IMD5",
      Value = 1,
      CheckSum = 11
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age55to59",
      Descriptions = "Age group 55-59",
      Value = 4,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityCompletionData",
      Key = "Other",
      Descriptions = "Other",
      Value = 6,
      CheckSum = 16
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "HighCategoryLowScoreCompletion",
      Descriptions = "High Category Low Score Completion",
      Value = 17,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age55to59",
      Descriptions = "Age group 55-59",
      Value = 5,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityCompletionData",
      Key = "Black",
      Descriptions = "Black",
      Value = 1,
      CheckSum = 16
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age40to44",
      Descriptions = "Age group 40-44",
      Value = 10,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationCompletionData",
      Key = "IMD4",
      Descriptions = "IMD4",
      Value = 2,
      CheckSum = 11
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexCompletionData",
      Key = "Male",
      Descriptions = "Male",
      Value = 1,
      CheckSum = 2
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityWeightData",
      Key = "Mixed",
      Descriptions = "Mixed",
      Value = 6,
      CheckSum = 33
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age75Plus",
      Descriptions = "Age group 75+",
      Value = 5,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MediumCategoryHighScoreWeight",
      Descriptions = "Medium Category High Score Weight",
      Value = 25,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MinimumPossibleScoreWeight",
      Descriptions = "Minimum Possible Score Weight",
      Value = 4,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationWeightData",
      Key = "IMD5",
      Descriptions = "IMD5",
      Value = 1,
      CheckSum = 17
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexWeightData",
      Key = "Male",
      Descriptions = "Male",
      Value = 1,
      CheckSum = 7
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityWeightData",
      Key = "White",
      Descriptions = "White",
      Value = 1,
      CheckSum = 33
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationCompletionData",
      Key = "IMD1",
      Descriptions = "IMD1",
      Value = 4,
      CheckSum = 11
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MaximumPossibleScoreWeight",
      Descriptions = "Maximum Possible Score Weight",
      Value = 35,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityWeightData",
      Key = "Black",
      Descriptions = "Black",
      Value = 5,
      CheckSum = 33
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "HighCategoryHighScoreWeight",
      Descriptions = "High Category High Score Weight",
      Value = 35,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityCompletionData",
      Key = "White",
      Descriptions = "White",
      Value = 1,
      CheckSum = 16
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age45to49",
      Descriptions = "Age group 45-49",
      Value = 5,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationWeightData",
      Key = "IMD1",
      Descriptions = "IMD1",
      Value = 6,
      CheckSum = 17
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "HighCategoryLowScoreWeight",
      Descriptions = "High Category Low Score Weight",
      Value = 26,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "MediumCategoryLowScoreCompletion",
      Descriptions = "Medium Category Low Score Completion",
      Value = 11,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "DeprivationWeightData",
      Key = "IMD2",
      Descriptions = "IMD2",
      Value = 4,
      CheckSum = 17
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age00to39",
      Descriptions = "Age group <40",
      Value = 13,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "LowCategoryLowScoreCompletion",
      Descriptions = "Low Category Low Score Completion",
      Value = 4,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age60to64",
      Descriptions = "Age group 60-64",
      Value = 1,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "HighCategoryHighScoreCompletion",
      Descriptions = "High Category High Score Completion",
      Value = 24,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "LowCategoryHighScoreWeight",
      Descriptions = "Low Category High Score Weight",
      Value = 14,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age75Plus",
      Descriptions = "Age group 75+",
      Value = 3,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityCompletionData",
      Key = "Asian",
      Descriptions = "Asian",
      Value = 4,
      CheckSum = 16
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupCompletionData",
      Key = "Age50to54",
      Descriptions = "Age group 50-54",
      Value = 8,
      CheckSum = 56
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "LowCategoryHighScoreCompletion",
      Descriptions = "Low Category High Score Completion",
      Value = 10,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "CompletionScores",
      Key = "LowCategoryLowScoreWeight",
      Descriptions = "Low Category Low Score Weight",
      Value = 4,
      CheckSum = -1
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "AgeGroupWeightData",
      Key = "Age00to39",
      Descriptions = "Age group <40",
      Value = 9,
      CheckSum = 42
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexWeightData",
      Key = "Female",
      Descriptions = "Female",
      Value = 6,
      CheckSum = 7
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "EthnicityWeightData",
      Key = "Other",
      Descriptions = "Other",
      Value = 8,
      CheckSum = 33
    });
    try
    {
      ctx.SaveChanges();
    }
    catch (Exception)
    { }
  }
}