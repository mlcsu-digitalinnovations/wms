using AutoMapper;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Business.Services;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class PatientTriageServiceTests : ServiceTestsBase
  {
    private readonly MapperConfiguration _config;
    private readonly IMapper _map;
    private PatientTriageService _classToTest;
    private readonly DatabaseContext _context;
    private Entities.PatientTriage _triageItem;
    private const int LOW_SCORE_COMPLETION_LIMIT = 13;
    private const int LOW_SCORE_WEIGHT_LIMIT = 14;
    private const int MEDIUM_SCORE_COMPLETION_LIMIT = 16;
    private const int MEDIUM_SCORE_WEIGHT_LIMIT = 25;


    public PatientTriageServiceTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _config = new MapperConfiguration(cfg => cfg
        .CreateMap<Entities.PatientTriage, PatientTriage>());
      _map = _config.CreateMapper();
      _context = new DatabaseContext(_serviceFixture.Options);
      _classToTest = new PatientTriageService(_context, _map);
      _triageItem = _context.PatientTriages.FirstOrDefault();

      if (_triageItem == null)
      {

        List<Entities.PatientTriage> items = new List<Entities.PatientTriage>
        {
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age00to39.ToString(),
            Value = 13,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age00to39.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age40to44.ToString(),
            Value = 10,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age40to44.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age45to49.ToString(),
            Value = 10,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age45to49.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age50to54.ToString(),
            Value = 8,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age50to54.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age55to59.ToString(),
            Value = 5,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age55to59.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age60to64.ToString(),
            Value = 4,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age60to64.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age65to69.ToString(),
            Value = 1,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age65to69.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age70to74.ToString(),
            Value = 2,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age70to74.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupCompletionData.ToString(),
            Descriptions = Enums.AgeGroup.Age75Plus.ToString(),
            Value = 3,
            CheckSum = 56,
            Key = Enums.AgeGroup.Age75Plus.ToString()
          },

          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age00to39.ToString(),
            Value = 9,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age00to39.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age40to44.ToString(),
            Value = 10,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age40to44.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age45to49.ToString(),
            Value = 5,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age45to49.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age50to54.ToString(),
            Value = 6,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age50to54.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age55to59.ToString(),
            Value = 4,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age55to59.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age60to64.ToString(),
            Value = 1,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age60to64.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age65to69.ToString(),
            Value = 1,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age65to69.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age70to74.ToString(),
            Value = 1,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age70to74.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.AgeGroupWeightData.ToString(),
            Descriptions = Enums.AgeGroup.Age75Plus.ToString(),
            Value = 5,
            CheckSum = 42,
            Key = Enums.AgeGroup.Age75Plus.ToString()
          },

          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.SexCompletionData.ToString(),
            Descriptions = "Male",
            Value = 1,
            CheckSum = 2,
            Key = Enums.Sex.Male.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.SexCompletionData.ToString(),
            Descriptions = "Female",
            Value = 1,
            CheckSum = 2,
            Key = Enums.Sex.Female.ToString()
          },

          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.SexWeightData.ToString(),
            Descriptions = "Male",
            Value = 1,
            CheckSum = 7,
            Key = Enums.Sex.Male.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.SexWeightData.ToString(),
            Descriptions = "Female",
            Value = 6,
            CheckSum = 7,
            Key = Enums.Sex.Female.ToString()
          },

          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityCompletionData.ToString(),
            Descriptions = "White",
            Value = 1,
            CheckSum = 16,
            Key = Enums.Ethnicity.White.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityCompletionData.ToString(),
            Descriptions = "Asian",
            Value = 4,
            CheckSum = 16,
            Key = Enums.Ethnicity.Asian.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityCompletionData.ToString(),
            Descriptions = "Black",
            Value = 1,
            CheckSum = 16,
            Key = Enums.Ethnicity.Black.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityCompletionData.ToString(),
            Descriptions = "Mixed",
            Value = 4,
            CheckSum = 16,
            Key = Enums.Ethnicity.Mixed.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityCompletionData.ToString(),
            Descriptions = "Other",
            Value = 6,
            CheckSum = 16,
            Key = Enums.Ethnicity.Other.ToString()
          },

          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityWeightData.ToString(),
            Descriptions = "White",
            Value = 1,
            CheckSum = 33,
            Key = Enums.Ethnicity.White.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityWeightData.ToString(),
            Descriptions = "Asian",
            Value = 13,
            CheckSum = 33,
            Key = Enums.Ethnicity.Asian.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityWeightData.ToString(),
            Descriptions = "Black",
            Value = 5,
            CheckSum = 33,
            Key = Enums.Ethnicity.Black.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityWeightData.ToString(),
            Descriptions = "Mixed",
            Value = 6,
            CheckSum = 33,
            Key = Enums.Ethnicity.Mixed.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.EthnicityWeightData.ToString(),
            Descriptions = "Other",
            Value = 8,
            CheckSum = 33,
            Key = Enums.Ethnicity.Other.ToString()
          },

          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationCompletionData.ToString(),
            Descriptions = "IMD1",
            Value = 4,
            CheckSum = 11,
            Key = Enums.Deprivation.IMD1.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationCompletionData.ToString(),
            Descriptions = "IMD2",
            Value = 2,
            CheckSum = 11,
            Key = Enums.Deprivation.IMD2.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationCompletionData.ToString(),
            Descriptions = "IMD3",
            Value = 2,
            CheckSum = 11,
            Key = Enums.Deprivation.IMD3.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationCompletionData.ToString(),
            Descriptions = "IMD4",
            Value = 2,
            CheckSum = 11,
            Key = Enums.Deprivation.IMD4.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationCompletionData.ToString(),
            Descriptions = "IMD5",
            Value = 1,
            CheckSum = 11,
            Key = Enums.Deprivation.IMD5.ToString()
          },


          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationWeightData.ToString(),
            Descriptions = "IMD1",
            Value = 6,
            CheckSum = 17,
            Key = Enums.Deprivation.IMD1.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationWeightData.ToString(),
            Descriptions = "IMD2",
            Value = 4,
            CheckSum = 17,
            Key = Enums.Deprivation.IMD2.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationWeightData.ToString(),
            Descriptions = "IMD3",
            Value = 4,
            CheckSum = 17,
            Key = Enums.Deprivation.IMD3.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationWeightData.ToString(),
            Descriptions = "IMD4",
            Value = 2,
            CheckSum = 17,
            Key = Enums.Deprivation.IMD4.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(),
            IsActive = true,
            ModifiedAt = DateTimeOffset.Now,
            ModifiedByUserId = Guid.NewGuid(),
            TriageSection =
              Enums.TriageSection.DeprivationWeightData.ToString(),
            Descriptions = "IMD5",
            Value = 1,
            CheckSum = 17,
            Key = Enums.Deprivation.IMD5.ToString()
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Minimum Possible Score Completion", Value = 4,
            CheckSum = -1, Key = "MinimumPossibleScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Minimum Possible Score Weight", Value = 4,
            CheckSum = -1, Key = "MinimumPossibleScoreWeight"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Low Category Low Score Completion", Value = 4,
            CheckSum = -1, Key = "LowCategoryLowScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Low Category Low Score Weight", Value = 4,
            CheckSum = -1, Key = "LowCategoryLowScoreWeight"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Low Category High Score Completion", Value = 13,
            CheckSum = -1, Key = "LowCategoryHighScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Medium Category Low Score Completion", Value = 14,
            CheckSum = -1, Key = "MediumCategoryLowScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Low Category High Score Weight", Value = 14,
            CheckSum = -1, Key = "LowCategoryHighScoreWeight"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Medium Category Low Score Weight", Value = 15,
            CheckSum = -1, Key = "MediumCategoryLowScoreWeight"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Medium Category High Score Completion", Value = 16,
            CheckSum = -1, Key = "MediumCategoryHighScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "High Category Low Score Completion", Value = 17,
            CheckSum = -1, Key = "HighCategoryLowScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Maximum Possible Score Completion", Value = 24,
            CheckSum = -1, Key = "MaximumPossibleScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "High Category High Score Completion", Value = 24,
            CheckSum = -1, Key = "HighCategoryHighScoreCompletion"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Medium Category High Score Weight", Value = 25,
            CheckSum = -1, Key = "MediumCategoryHighScoreWeight"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "High Category Low Score Weight", Value = 26,
            CheckSum = -1, Key = "HighCategoryLowScoreWeight"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "Maximum Possible Score Weight", Value = 35,
            CheckSum = -1, Key = "MaximumPossibleScoreWeight"
          },
          new Entities.PatientTriage
          {
            Id = Guid.NewGuid(), IsActive = true,
            ModifiedAt = DateTimeOffset.Now, ModifiedByUserId = Guid.NewGuid(),
            TriageSection = "CompletionScores",
            Descriptions = "High Category High Score Weight", Value = 35,
            CheckSum = -1, Key = "HighCategoryHighScoreWeight"
          },


        };

        _context.PatientTriages.AddRange(items);
        _context.SaveChanges();
      }

    }

    [Fact]
    public void DataIntegrity_AgeGroupCompletion()
    {
      // assert       
      ChecksumPassed(_classToTest.AgeGroupCompletionData)
        .Should().BeTrue();
    }

    [Fact]
    public void DataIntegrity_AgeGroupWeight()
    {
      // assert
      ChecksumPassed(_classToTest.AgeGroupWeightData)
        .Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(AgeGroupData))]
    public void GetScores_AgeGroups(
      Enums.AgeGroup ageGroup,
      Enums.Sex sex,
      Enums.Ethnicity ethnicity,
      Enums.Deprivation deprivation,
      Enums.TriageLevel expectedCompletionTriageCategory,
      Enums.TriageLevel expectedWeightTriageCategory)
    {
      // arrange 
      CourseCompletionParameters parameters = new CourseCompletionParameters(
        ageGroup, sex, ethnicity, deprivation);

      // act
      CourseCompletionResult result = _classToTest.GetScores(parameters);

      // assert
      result.Should().BeOfType<CourseCompletionResult>();
      result.TriagedCompletionLevel
        .Should().Be(expectedCompletionTriageCategory);
      result.TriagedWeightedLevel
        .Should().Be(expectedWeightTriageCategory);

    }

    [Theory]
    [MemberData(nameof(AgeValueData))]
    public void GetScores_AgeValues(
      int age,
      Enums.Sex sex,
      Enums.Ethnicity ethnicity,
      Enums.Deprivation deprivation,
      Enums.TriageLevel expectedCompletionTriageCategory,
      Enums.TriageLevel expectedWeightTriageCategory)
    {
      // arrange
      CourseCompletionParameters parameters = new CourseCompletionParameters(
        age,
        sex,
        ethnicity,
        deprivation);

      // act
      CourseCompletionResult result = _classToTest.GetScores(parameters);

      // assert
      result.Should().BeOfType<CourseCompletionResult>();
      result.TriagedCompletionLevel
        .Should().Be(expectedCompletionTriageCategory);
      result.TriagedWeightedLevel
        .Should().Be(expectedWeightTriageCategory);
    }

    [Fact]
    public void GetScores_InvalidParameter()
    {
      // act
      Assert.Throws<ArgumentNullException>(() => _classToTest.GetScores(null));
    }

    /// <summary>
    /// Creates and returns an enumerable of object arrays containing all 
    /// combinations of age groups, sex, ethnicity, deprivations and their
    /// calculated completion and weighted scores usable by 
    /// GetScores_AgeGroups_Test
    /// </summary>
    public static IEnumerable<object[]> AgeGroupData()
    {
      List<ValueCompWeight> ages = new List<ValueCompWeight>()
      {
        new ValueCompWeight((int)Enums.AgeGroup.Age00to39, 13, 9),
        new ValueCompWeight((int)Enums.AgeGroup.Age40to44, 10, 10),
        new ValueCompWeight((int)Enums.AgeGroup.Age45to49, 10, 5),
        new ValueCompWeight((int)Enums.AgeGroup.Age50to54, 8, 6),
        new ValueCompWeight((int)Enums.AgeGroup.Age55to59, 5, 4),
        new ValueCompWeight((int)Enums.AgeGroup.Age60to64, 4, 1),
        new ValueCompWeight((int)Enums.AgeGroup.Age65to69, 1, 1),
        new ValueCompWeight((int)Enums.AgeGroup.Age70to74, 2, 1),
        new ValueCompWeight((int)Enums.AgeGroup.Age75Plus, 3, 5)
      };

      return AgeData(ages, true);
    }

    /// <summary>
    /// Creates and returns an enumerable of object arrays containing all 
    /// combinations of ages (0 to 125), sex, ethnicity, deprivations and their
    /// calculated completion and weighted scores usable by 
    /// GetScores_AgeValues_Test
    /// </summary>
    public static IEnumerable<object[]> AgeValueData()
    {
      List<ValueCompWeight> ages = new List<ValueCompWeight>();
      for (int i = 0; i <= 125; i++)
      {
        if (i < 40) ages.Add(new ValueCompWeight(i, 13, 9));
        else if (i < 45) ages.Add(new ValueCompWeight(i, 10, 10));
        else if (i < 50) ages.Add(new ValueCompWeight(i, 10, 5));
        else if (i < 55) ages.Add(new ValueCompWeight(i, 8, 6));
        else if (i < 60) ages.Add(new ValueCompWeight(i, 5, 4));
        else if (i < 65) ages.Add(new ValueCompWeight(i, 4, 1));
        else if (i < 70) ages.Add(new ValueCompWeight(i, 1, 1));
        else if (i < 75) ages.Add(new ValueCompWeight(i, 2, 1));
        else ages.Add(new ValueCompWeight(i, 3, 5));
      }

      return AgeData(ages, false);

    }

    /// <summary>
    /// Creates and returns an enumerable of object arrays containing all 
    /// combinations of ages/age groups, sex, ethnicity, deprivations and their
    /// calculated completion and weighted scores
    /// </summary>
    /// <param name="ages">A list where the value property of each object is 
    /// either an age integer or the age group enum as an integer</param>
    /// <param name="areAgeValuesGroupEnums">True if the ages list contains
    /// the enumerate for age groups. False if it is an integer</param>
    private static IEnumerable<object[]> AgeData(
      List<ValueCompWeight> ages, bool areAgeValuesGroupEnums)
    {
      List<ValueCompWeight> sexes = new List<ValueCompWeight>()
      {
        new ValueCompWeight(0,1,1),
        new ValueCompWeight(1,1,6)
      };

      List<ValueCompWeight> ethnicities = new List<ValueCompWeight>()
      {
        new ValueCompWeight(0,1,1),
        new ValueCompWeight(1,4,13),
        new ValueCompWeight(2,1,5),
        new ValueCompWeight(3,4,6),
        new ValueCompWeight(4,6,8)
      };


      List<ValueCompWeight> deprivations = new List<ValueCompWeight>()
      {
        new ValueCompWeight(0,4,6),
        new ValueCompWeight(1,2,4),
        new ValueCompWeight(2,2,4),
        new ValueCompWeight(3,2,2),
        new ValueCompWeight(4,1,1)
      };

      List<object[]> ageData = new List<object[]>();

      ages.ForEach(age =>
      {
        sexes.ForEach(sex =>
        {
          ethnicities.ForEach(ethnicity =>
          {
            deprivations.ForEach(deprivation =>
            {
              ageData.Add(new object[]
              {
                areAgeValuesGroupEnums ? (Enums.AgeGroup)age.value : age.value,
                (Enums.Sex)sex.value,
                (Enums.Ethnicity)ethnicity.value,
                (Enums.Deprivation)deprivation.value,
                CalculateComp(age, sex, ethnicity, deprivation),
                CalculateWeight(age, sex, ethnicity, deprivation)
              });
            });
          });
        });
      });

      return ageData;
    }

    /// <summary>
    /// Calculates and returns the completion score from the comp properties
    /// of the provided parameters where:
    /// Low <= 13
    /// Medium > 14 <= 16
    /// High > 17
    /// </summary>
    private static Enums.TriageLevel CalculateComp(
      ValueCompWeight age,
      ValueCompWeight sex,
      ValueCompWeight ethnicity,
      ValueCompWeight deprivation)
    {
      Enums.TriageLevel score;

      int total = age.comp + sex.comp + ethnicity.comp + deprivation.comp;

      if (total <= LOW_SCORE_COMPLETION_LIMIT)
      {
        score = Enums.TriageLevel.Low;
      }
      else if (total <= MEDIUM_SCORE_COMPLETION_LIMIT)
      {
        score = Enums.TriageLevel.Medium;
      }
      else
      {
        score = Enums.TriageLevel.High;
      }

      return score;
    }

    /// <summary>
    /// Calculates and returns the weight score from the weight properties of 
    /// the provided parameters where:
    /// Low <= 11
    /// Medium > 11 <= 18
    /// High > 18
    /// </summary>
    private static Enums.TriageLevel CalculateWeight(
      ValueCompWeight age,
      ValueCompWeight sex,
      ValueCompWeight ethnicity,
      ValueCompWeight deprivation)
    {
      Enums.TriageLevel score;

      int total =
        age.weight + sex.weight + ethnicity.weight + deprivation.weight;

      if (total <= LOW_SCORE_WEIGHT_LIMIT)
      {
        score = Enums.TriageLevel.Low;
      }
      else if (total <= MEDIUM_SCORE_WEIGHT_LIMIT)
      {
        score = Enums.TriageLevel.Medium;
      }
      else
      {
        score = Enums.TriageLevel.High;
      }

      return score;
    }

    /// <summary>
    /// Checks the total of all values of the table and compares to the 
    /// checksum value
    /// </summary>
    /// <param name="tableData"></param>
    /// <returns>True if Totals match Checksum</returns>
    private static bool ChecksumPassed(Dictionary<string, PatientTriage> dict)
    {
      int totalTableValues = dict.Sum(t => t.Value.Value);
      KeyValuePair<string, PatientTriage> kvp = dict.First();
      return totalTableValues == kvp.Value.CheckSum;

    }

    private struct ValueCompWeight
    {
      public readonly int value;
      public readonly int comp;
      public readonly int weight;

      public ValueCompWeight(int value, int comp, int weight)
      {
        this.value = value;
        this.comp = comp;
        this.weight = weight;
      }
    }
  }
}
