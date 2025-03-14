﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Helpers;
using WmsHub.Common.Helpers;

namespace WmsHub.Tests.Helper;
public class TestWebApplicationFactory<TStartup>
  : WebApplicationFactory<TStartup> where TStartup : class
{
  public DbContextOptions<DatabaseContext> Options { get; private set; } = null!;

  private static Dictionary<string, string> _environmentVariables;
  private static Dictionary<string, string> _inMemoryVariables;

  public IHost Host { get; private set; }

  public TestWebApplicationFactory()
  {
    EnvironmentVariableConfigurator.SetDefaults();
    EnvironmentVariableConfigurator.ConfigureEnvironmentVariablesForAlwaysEncrypted();
  }

  public static TestWebApplicationFactory<TStartup> Create(WebApplicationFactoryBuilder builder)
  {
    _environmentVariables = builder.EnvironmentVariables ??= [];
    _inMemoryVariables = builder.InMemoryValues;
    Environment.SetEnvironmentVariable(
      Constants.POLICIES_IN_TEST, 
      string.Join(",", builder.PoliciesToInclude));
    return new TestWebApplicationFactory<TStartup>();
  }

  protected override IHostBuilder CreateHostBuilder()
  {
    Environment.SetEnvironmentVariable("X-UNIT_TEST", "true");
    IHostBuilder hostBuilder = base.CreateHostBuilder()
      .ConfigureAppConfiguration((ctx, config) =>
      {
        config.AddInMemoryCollection(_inMemoryVariables);
      });
 
    // Add your custom configuration
    hostBuilder.ConfigureServices((hostContext, services) =>
    {
      // Configure additional services for testing
      // ...
      ServiceDescriptor descriptor = services
        .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));

      if (descriptor != null)
      {
        services.Remove(descriptor!);
      }

      services.AddDbContext<DatabaseContext>(
        options => options.UseInMemoryDatabase("wmsHub_TestApi"));

      ServiceProvider sp = services.BuildServiceProvider();

      using IServiceScope scope = sp.CreateScope();
      IServiceProvider scopedServices = scope.ServiceProvider;
      DatabaseContext db = scopedServices.GetRequiredService<DatabaseContext>();

      db.Database.EnsureCreated();

      PopulatePatientTriageService(db);
      PopulateEthnicities(db);
    });

    return hostBuilder;
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
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexCompletionData",
      Key = "NotKnown",
      Descriptions = "Not Known",
      Value = 1,
      CheckSum = 2
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexCompletionData",
      Key = "NotSpecified",
      Descriptions = "Not Specified",
      Value = 1,
      CheckSum = 2
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexWeightData",
      Key = "NotKnown",
      Descriptions = "Not Known",
      Value = 1,
      CheckSum = 7
    });
    ctx.PatientTriages.Add(new PatientTriage
    {
      Id = Guid.NewGuid(),
      IsActive = true,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = Guid.Empty,
      TriageSection = "SexWeightData",
      Key = "NotSpecified",
      Descriptions = "Not Specified",
      Value = 1,
      CheckSum = 7
    });
    try
    {
      ctx.SaveChanges();
    }
    catch (Exception)
    { }
  }

  public void PopulateEthnicities(DatabaseContext context)
  {
    if (context.Ethnicities.Any())
    {
      return;
    }

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("3C69F5AE-073F-F180-3CAC-2197EB73E369"),
      displayName: "Indian",
      groupName: "Asian or Asian British",
      census2001: "Indian or British Indian",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 1,
      nhsDataDictionary2001Code: "H",
      nhsDataDictionary2001Description: "Asian or Asian British - Indian"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("76D69A87-D9A7-EAC6-2E2D-A6017D02E04F"),
      displayName: "Pakistani",
      groupName: "Asian or Asian British",
      census2001: "Pakistani or British Pakistani",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 2,
      nhsDataDictionary2001Code: "J",
      nhsDataDictionary2001Description: "Asian or Asian British - Pakistani"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("5BF8BFAB-DAB1-D472-51CA-9CF0CB056D3F"),
      displayName: "Bangladeshi",
      groupName: "Asian or Asian British",
      census2001: "Pakistani or British Pakistani",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 3,
      nhsDataDictionary2001Code: "K",
      nhsDataDictionary2001Description:
        "Asian or Asian British - Bangladeshi"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("EFC61F30-F872-FA71-9709-1A416A51982F"),
      displayName: "Chinese",
      groupName: "Asian or Asian British",
      census2001: "Chinese",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 4,
      nhsDataDictionary2001Code: "R",
      nhsDataDictionary2001Description: "Other Ethnic Groups - Chinese"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("CB5CA465-C397-A34F-F32B-729A38932E0E"),
      displayName: "Any other Asian background",
      groupName: "Asian or Asian British",
      census2001: "Other Asian background",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 5,
      nhsDataDictionary2001Code: "L",
      nhsDataDictionary2001Description:
        "Asian or Asian British - Any other Asian background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("F6C29207-A3FC-163B-94BC-2CE840AF9396"),
      displayName: "African",
      groupName: "Black, African, Caribbean or Black British",
      census2001: "African",
      triageName: "Black",
      minimumBmi: 27.50M,
      groupOrder: 4,
      displayOrder: 1,
      nhsDataDictionary2001Code: "N",
      nhsDataDictionary2001Description: "Black or Black British - African"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("36FE1D6A-3B04-5A31-FBD9-8D378C2CB86A"),
      displayName: "Caribbean",
      groupName: "Black, African, Caribbean or Black British",
      census2001: "Caribbean",
      triageName: "Black",
      minimumBmi: 27.50M,
      groupOrder: 4,
      displayOrder: 2,
      nhsDataDictionary2001Code: "M",
      nhsDataDictionary2001Description: "Black or Black British - Caribbean"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("E0694F9A-2D9E-BEF6-2F46-6EB9FB7891AD"),
      displayName: "Any other Black, African or Caribbean background",
      groupName: "Black, African, Caribbean or Black British",
      census2001: "Other Black background",
      triageName: "Black",
      minimumBmi: 27.50M,
      groupOrder: 4,
      displayOrder: 3,
      nhsDataDictionary2001Code: "P",
      nhsDataDictionary2001Description:
        "Black or Black British - Any other Black background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("3185A21D-2FD4-4313-4A59-43DB28A2E89A"),
      displayName: "White and Black Caribbean",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "White and Black Caribbean",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 1,
      nhsDataDictionary2001Code: "D",
      nhsDataDictionary2001Description: "Mixed - White and Black Caribbean"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("EDFE5D64-E5D8-9D27-F9C5-DC953D351CF7"),
      displayName: "White and Black African",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "White and Black African",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 2,
      nhsDataDictionary2001Code: "E",
      nhsDataDictionary2001Description: "Mixed - White and Black African"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("279DC2CB-6F4B-96BC-AE72-B96BF7A2579A"),
      displayName: "White and Asian",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "White and Asian",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 3,
      nhsDataDictionary2001Code: "F",
      nhsDataDictionary2001Description: "Mixed - White and Asian"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("4E84EFCD-3DBA-B459-C302-29BCBD9E8E64"),
      displayName: "Any other Mixed or Multiple ethnic background",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "Any other Mixed background",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 4,
      nhsDataDictionary2001Code: "G",
      nhsDataDictionary2001Description: "Mixed - Any other mixed background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("5DC90D60-F03C-3CE6-72F6-A34D4E6F163B"),
      displayName: "English, Welsh, Scottish, Northern Irish or British",
      groupName: "White",
      census2001: "British",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 1,
      nhsDataDictionary2001Code: "A",
      nhsDataDictionary2001Description: "White - British"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("5D2B37FD-24C4-7572-4AEA-D437C6E17318"),
      displayName: "Irish",
      groupName: "White",
      census2001: "Irish",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 2,
      nhsDataDictionary2001Code: "B",
      nhsDataDictionary2001Description: "White - Irish"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("A1B8C48B-FA12-E001-9F8E-3C9BA9D3D065"),
      displayName: "Gypsy or Irish Traveller",
      groupName: "White",
      census2001: "Other White background",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 3,
      nhsDataDictionary2001Code: null,
      nhsDataDictionary2001Description: null));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("75E8313C-BFDF-5ABF-B6DA-D6CA64138CF4"),
      displayName: "Any other White background",
      groupName: "White",
      census2001: "Any other White background",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 4,
      nhsDataDictionary2001Code: "C",
      nhsDataDictionary2001Description: "White - Any other White background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("934A2FA6-F541-60F1-D08D-46F5E647A28D"),
      displayName: "Arab",
      groupName: "Other ethnic group",
      census2001: "Other - ethnic category",
      triageName: "Other",
      minimumBmi: 27.50M,
      groupOrder: 5,
      displayOrder: 1,
      nhsDataDictionary2001Code: null,
      nhsDataDictionary2001Description: null));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("D15B2787-7926-1EF6-704E-1012F9298AE1"),
      displayName: "Any other ethnic group",
      groupName: "Other ethnic group",
      census2001: "Other - ethnic category",
      triageName: "Other",
      minimumBmi: 27.50M,
      groupOrder: 5,
      displayOrder: 2,
      nhsDataDictionary2001Code: "S",
      nhsDataDictionary2001Description:
        "Other Ethnic Groups - Any other ethnic group"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("95b0feb5-5ece-98ed-1269-c71e327e98c5"),
      displayName: "I do not wish to Disclose my Ethnicity",
      groupName: "I do not wish to Disclose my Ethnicity",
      census2001: null,
      triageName: "Other",
      minimumBmi: 30.00M,
      groupOrder: 6,
      displayOrder: 1,
      nhsDataDictionary2001Code: "Z",
      nhsDataDictionary2001Description: "Not stated"));

    context.SaveChanges();
  }
}
