using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Controllers;
using WmsHub.BusinessIntelligence.Api.Test;
using Xunit;
using Xunit.Abstractions;
using Entities = WmsHub.Business.Entities;

namespace WmsHub.BusinessIntelligence.Api.Tests
{
  [Collection("Service collection")]
  public class BusinessIntelligenceReferralControllerTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly IBusinessIntelligenceService _service;
    private readonly ReferralController _controller;

    public BusinessIntelligenceReferralControllerTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context = new DatabaseContext(_serviceFixture.Options);

      _service = new BusinessIntelligenceService(
        _context,
        _serviceFixture.Mapper,
        _log)
      {
        User = GetClaimsPrincipal()
      };

      _controller = new ReferralController(
        _service,
        _serviceFixture.Mapper);
    }


    [Fact]
    public async Task Get_Returns_NOTFOUND()
    {
      // ARRANGE
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      // ACT
      var result = await _controller.Get(null, null);

      // ASSERT
      result.Result.ToString()
        .Equals("Microsoft.AspNetCore.Mvc.NotFoundResult");
    }

    [Fact]
    public async Task Get_ReturnsAViewModel_WithAListOfAnonymisedReferrals()
    {
      // ARRANGE
      int offsetOne = -90;
      int offsetTwo = -50;
      DateTimeOffset? fromDate = new DateTimeOffset(new DateTime(2020, 1, 1));
      DateTimeOffset? toDate = DateTimeOffset.Now;
      int expectedCount = 2;

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);
      _context.StaffRoles.RemoveRange(_context.StaffRoles);

      await AddTestReferrals(offsetOne, offsetTwo);

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      Assert.NotNull(result);
      Assert.NotNull(okObjectResult);
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      Assert.Equal(expectedCount, anonReferrals.Count);
      Assert.True(okObjectResult.StatusCode == 200);
      foreach (var anonReferral in anonReferrals)
      {
        if (anonReferral.ReferralSource ==
            ReferralSource.SelfReferral.ToString())
        {
          anonReferral.StaffRole.Should().NotBeNullOrWhiteSpace();
        }
        else
        {
          anonReferral.StaffRole.Should().BeNull();
        }
      }
    }

    [Fact]
    public async Task Get_ReturnsAViewModel_usingFromDate()
    {
      // ARRANGE
      int offsetOne = -90;
      int offsetTwo = -50;
      DateTimeOffset? fromDate = DateTime.Now.AddDays(offsetOne + 10);
      DateTimeOffset? toDate = null;
      int expectedCount = 1;

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      await AddTestReferrals(offsetOne, offsetTwo);

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      Assert.NotNull(result);
      Assert.NotNull(okObjectResult);
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      Assert.Equal(expectedCount, anonReferrals.Count);
      Assert.True(okObjectResult.StatusCode == 200);
    }


    [Fact]
    public async Task Get_ReturnsAViewModel_usingToDate()
    {
      // ARRANGE
      int offsetDays = -90;
      DateTimeOffset? fromDate = null;
      DateTimeOffset? toDate = DateTime.Now.AddDays(offsetDays + 10); ;
      int expectedCount = 1;

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      await AddTestReferrals(offsetDays, offsetDays + 20);

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      Assert.NotNull(result);
      Assert.NotNull(okObjectResult);
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      Assert.Equal(expectedCount, anonReferrals.Count);
      Assert.True(okObjectResult.StatusCode == 200);
    }

    [Fact]
    public async Task Get_ReturnsAViewModel_usingBothToDates()
    {
      // ARRANGE
      int offsetOne = -90;
      int offsetTwo = -50;
      DateTimeOffset? fromDate = DateTime.Now.AddDays(offsetOne - 10);
      DateTimeOffset? toDate = DateTime.Now.AddDays(offsetTwo + 10);
      int expectedCount = 2;

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      await AddTestReferrals(offsetOne, offsetTwo);

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      Assert.NotNull(result);
      Assert.NotNull(okObjectResult);
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      Assert.Equal(expectedCount, anonReferrals.Count);
      Assert.True(okObjectResult.StatusCode == 200);
    }

    private async Task AddTestReferrals(int offsetOne, int offsetTwo)
    {
      //add entities
      Guid providerId1 = Guid.NewGuid();

      Entities.ProviderSubmission providerSub1 = new()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = providerId1
      };
      Entities.ProviderSubmission providerSub2 = new()
      {
        Coaching = 6,
        Date = DateTime.Now.AddDays(-14),
        Measure = 7,
        Weight = 97,
        ProviderId = providerId1
      };

      List<Entities.ProviderSubmission> listProviderSubs1 = new()
      {
        providerSub1,
        providerSub2
      };

      Entities.Provider provider1 = RandomEntityCreator.CreateRandomProvider(
        id: providerId1,
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: "Provider One");
      provider1.ProviderSubmissions = listProviderSubs1;

      Guid providerId2 = Guid.NewGuid();

      Entities.ProviderSubmission providerSub3 = new()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = providerId2
      };

      Entities.ProviderSubmission providerSub4 = new()
      {
        Coaching = 6,
        Date = DateTime.Now.AddDays(-14),
        Measure = 7,
        Weight = 97,
        ProviderId = providerId2
      };

      List<Entities.ProviderSubmission> listProviderSubs2 = new()
      {
        providerSub3,
        providerSub4
      };

      Entities.Provider provider2 = RandomEntityCreator.CreateRandomProvider(
        id: providerId2,
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: "Provider Two");
      provider2.ProviderSubmissions = listProviderSubs2;

      _context.Providers.Add(provider1);
      _context.Providers.Add(provider2);

      Entities.StaffRole staff1 = new Entities.StaffRole()
      {
        DisplayName = "Doctor",
        IsActive = true,
        DisplayOrder = 1
      };
      Entities.StaffRole staff2 = new Entities.StaffRole()
      {
        DisplayName = "Ambulance Worker",
        IsActive = true,
        DisplayOrder = 2
      };
      Entities.StaffRole staff3 = new Entities.StaffRole()
      {
        DisplayName = "Nurse",
        IsActive = true,
        DisplayOrder = 3
      };
      Entities.StaffRole staff4 = new Entities.StaffRole()
      {
        DisplayName = "Porter",
        IsActive = true,
        DisplayOrder = 4
      };

      _context.StaffRoles.Add(staff1);
      _context.StaffRoles.Add(staff2);
      _context.StaffRoles.Add(staff3);
      _context.StaffRoles.Add(staff4);

      Entities.Referral referral1 = new Entities.Referral
      {
        CalculatedBmiAtRegistration = 30m,
        ConsentForFutureContactForEvaluation = false,
        DateOfBirth = DateTime.Now.AddYears(-50),
        DateOfReferral = DateTime.Now.AddDays(offsetOne),
        Ethnicity = "White",
        HasDiabetesType1 = true,
        HasDiabetesType2 = false,
        HasHypertension = true,
        HasRegisteredSeriousMentalIllness = false,
        HeightCm = 150m,
        IsActive = true,
        IsVulnerable = false,
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
        ReferringGpPracticeNumber = "M11111",
        Sex = "Male",
        Status = "ReferralStatus",
        StatusReason = null,
        TriagedCompletionLevel = null,
        TriagedWeightedLevel = null,
        Ubrn = "bb5eba8e-b6d9-47b5-9bb7-2bc36f0d394b",
        VulnerableDescription = "Not Vulnerable",
        WeightKg = 170m,
        Address1 = "Another Address1",
        Address2 = "Another Address2",
        FamilyName = "User 98",
        ProviderId = providerId1,
        ReferringGpPracticeName = "Referrer One",
        Deprivation = "IMD2",
        HasAPhysicalDisability = false,
        HasALearningDisability = true,
        DateOfProviderContactedServiceUser = DateTimeOffset.Now.AddDays(10),
        DateCompletedProgramme = DateTimeOffset.MinValue,
        DateOfProviderSelection = DateTimeOffset.Now,
        DateToDelayUntil = DateTimeOffset.MinValue,
        ReferralSource = ReferralSource.GpReferral.ToString(),
        StaffRole = "Test"
      };

      Entities.Referral referral2 = new Entities.Referral
      {
        CalculatedBmiAtRegistration = 33m,
        ConsentForFutureContactForEvaluation = false,
        DateOfBirth = DateTime.Now.AddYears(-47),
        DateOfReferral = DateTime.Now.AddDays(offsetTwo),
        Ethnicity = "White",
        HasDiabetesType1 = true,
        HasDiabetesType2 = false,
        HasHypertension = true,
        HasRegisteredSeriousMentalIllness = false,
        HeightCm = 150m,
        IsActive = true,
        IsVulnerable = false,
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId =
        new Guid("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
        ReferringGpPracticeNumber = "J62344",
        Sex = "Female",
        Status = "ReferralStatus",
        StatusReason = null,
        TriagedCompletionLevel = "2",
        TriagedWeightedLevel = null,
        Ubrn = "cc5eba9f-c6d9-47j3-1tt3-3rt26f0d763e",
        VulnerableDescription = "Vulnerable",
        WeightKg = 160m,
        Address1 = "Address1",
        Address2 = "Address2",
        FamilyName = "User 122",
        ProviderId = providerId2,
        ReferringGpPracticeName = "Referrer Two",
        Deprivation = "IMD4",
        DateCompletedProgramme = DateTimeOffset.MinValue,
        DateOfProviderSelection = DateTimeOffset.Now,
        DateToDelayUntil = DateTimeOffset.MinValue,
        HasAPhysicalDisability = true,
        HasALearningDisability = false,
        DateOfProviderContactedServiceUser = DateTimeOffset.Now.AddDays(16),
        ReferralSource = ReferralSource.SelfReferral.ToString(),
        StaffRole = staff2.DisplayName
      };

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);

      await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_NoContent204()
    {
      // ARRANGE
      DateTimeOffset? fromDate = null;
      DateTimeOffset? toDate = null;

      _context.Referrals.RemoveRange(_context.Referrals);
      await _context.SaveChangesAsync();

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      Assert.NotNull(result);
      NoContentResult returned = result.Result as NoContentResult;
      Assert.NotNull(returned);
      Assert.True(returned.StatusCode == 204);
    }
  }
}