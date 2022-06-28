using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
  public class SubmissionControllerTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly IBusinessIntelligenceService _service;
    private readonly SubmissionController _controller;

    public SubmissionControllerTests(
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

      _controller = new SubmissionController(
        _service,
        _serviceFixture.Mapper);
    }

    [Fact]
    public async Task Get_Returns_BadRequest_DatesIncorrect()
    {
      // ARRANGE
      DateTimeOffset? fromDate = DateTimeOffset.Now.AddDays(-20);
      DateTimeOffset? toDate = DateTimeOffset.Now.AddDays(-40);
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      var expectedMessage = $"'from' date {fromDate.Value} cannot be " +
        $"later than 'to' date {toDate.Value}.";

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      Assert.NotNull(result);
      ObjectResult outputResultbad = Assert.IsType<ObjectResult>(result.Result);
      ProblemDetails problemDetails = 
        ((ObjectResult)result.Result).Value as ProblemDetails;
      Assert.NotNull(problemDetails);
      Assert.True(problemDetails.Status == 400);
      Assert.Equal(expectedMessage, problemDetails.Detail);
    }

    [Fact]
    public async Task Get_ReturnsListOfAnonymisedReferrals_WithBothFilterDates()
    {
      // ARRANGE
      DateTime? fromDate = DateTime.Now.AddDays(-30);
      DateTime? toDate = DateTime.Now.AddDays(-5);
      int expectedReferralCount = 2;
      int expectedProviderSubCount = 2;

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      await AddTestReferrals();

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      Assert.NotNull(result);
      Assert.NotNull(okObjectResult);
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      Assert.Equal(expectedReferralCount, anonReferrals.Count);
      Assert.Equal(expectedProviderSubCount,
        anonReferrals[0].ProviderSubmissions.Count());
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
    public async Task Get_ReturnsListOfAnonymisedReferrals_WithFromFilterDates()
    {
      // ARRANGE
      DateTime? fromDate = DateTime.Now.AddDays(-35);
      DateTime? toDate = null;
      int expectedReferralCount = 2;
      int expectedProviderSubCount = 2;

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      await AddTestReferrals();

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      Assert.NotNull(result);
      Assert.NotNull(okObjectResult);
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      Assert.Equal(expectedReferralCount, anonReferrals.Count);
      foreach (Models.AnonymisedReferral referral in anonReferrals)
      {
        Assert.Equal(expectedProviderSubCount,
          referral.ProviderSubmissions.Count());
      }
      Assert.True(okObjectResult.StatusCode == 200);
    }


    [Fact]
    public async Task Get_ReturnsListOfAnonymisedReferrals_WithToFilterDates()
    {
      // ARRANGE
      DateTime? fromDate = null;
      DateTime? toDate = DateTime.Now.AddDays(-6);
      int expectedReferralCount = 2;
      int expectedProviderSubCount = 2;

      _context.Referrals.RemoveRange(_context.Referrals);
      _context.Providers.RemoveRange(_context.Providers);
      _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

      await AddTestReferrals();

      // ACT
      var result = await _controller.Get(fromDate, toDate);

      // ASSERT
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      Assert.NotNull(result);
      Assert.NotNull(okObjectResult);
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      Assert.Equal(expectedReferralCount, anonReferrals.Count);
      foreach (Models.AnonymisedReferral referral in anonReferrals)
      {
        Assert.Equal(expectedProviderSubCount,
          referral.ProviderSubmissions.Count());
      }
      Assert.True(okObjectResult.StatusCode == 200);
    }

    private async Task AddTestReferrals()
    {
      //add entities
      Entities.Provider provider1 = RandomEntityCreator.CreateRandomProvider(
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: "Provider One");

      Entities.Provider provider2 = RandomEntityCreator.CreateRandomProvider(
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: "Provider Two");

      _context.Providers.Add(provider1);
      _context.Providers.Add(provider2);
      _context.SaveChanges();

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
        DateOfReferral = DateTime.Now.AddDays(-30),
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
        ProviderId = provider1.Id,
        ReferringGpPracticeName = "Referrer One",
        HasAPhysicalDisability = false,
        HasALearningDisability = true,
        DateOfProviderContactedServiceUser = DateTimeOffset.Now.AddDays(15),
        ReferralSource = ReferralSource.GpReferral.ToString(),
        StaffRole = "Test"
      };

      Entities.Referral referral2 = new Entities.Referral
      {
        CalculatedBmiAtRegistration = 33m,
        ConsentForFutureContactForEvaluation = false,
        DateOfBirth = DateTime.Now.AddYears(-47),
        DateOfReferral = DateTime.Now.AddDays(-30),
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
        TriagedCompletionLevel = null,
        TriagedWeightedLevel = null,
        Ubrn = "cc5eba9f-c6d9-47j3-1tt3-3rt26f0d763e",
        VulnerableDescription = "Vulnerable",
        WeightKg = 160m,
        Address1 = "Address1",
        Address2 = "Address2",
        FamilyName = "User 122",
        ProviderId = provider2.Id,
        ReferringGpPracticeName = "Referrer Two",
        HasAPhysicalDisability = true,
        HasALearningDisability = false,
        DateOfProviderContactedServiceUser = DateTimeOffset.Now.AddDays(9),
        ReferralSource = ReferralSource.SelfReferral.ToString(),
        StaffRole = staff2.DisplayName
      };

      _context.Referrals.Add(referral1);
      _context.Referrals.Add(referral2);
      _context.SaveChanges();

      Entities.ProviderSubmission providerSub1 =
        new Entities.ProviderSubmission()
        {
          Coaching = 5,
          Date = DateTime.Now.AddDays(-28),
          Measure = 1,
          Weight = 87,
          ProviderId = provider1.Id,
          ReferralId = referral1.Id,
          IsActive = true
        };

      Entities.ProviderSubmission providerSub2 =
        new Entities.ProviderSubmission()
        {
          Coaching = 6,
          Date = DateTime.Now.AddDays(-7),
          Measure = 7,
          Weight = 97,
          ProviderId = provider1.Id,
          ReferralId = referral1.Id,
          IsActive = true
        };

      List<Entities.ProviderSubmission> listProviderSubs1 =
        new List<Entities.ProviderSubmission>() {
          providerSub1,
          providerSub2
        };

      Entities.ProviderSubmission providerSub3 =
      new Entities.ProviderSubmission()
      {
        Coaching = 5,
        Date = DateTime.Now.AddDays(-28),
        Measure = 1,
        Weight = 87,
        ProviderId = provider2.Id,
        ReferralId = referral2.Id,
        IsActive = true
      };

      Entities.ProviderSubmission providerSub4 =
        new Entities.ProviderSubmission()
        {
          Coaching = 6,
          Date = DateTime.Now.AddDays(-14),
          Measure = 7,
          Weight = 97,
          ProviderId = provider2.Id,
          ReferralId = referral2.Id,
          IsActive = true
        };

      List<Entities.ProviderSubmission> listProviderSubs2 =
        new List<Entities.ProviderSubmission>() {
          providerSub3,
          providerSub4
          };

      _context.ProviderSubmissions.Add(providerSub1);
      _context.ProviderSubmissions.Add(providerSub2);
      _context.ProviderSubmissions.Add(providerSub3);
      _context.ProviderSubmissions.Add(providerSub4);
      _context.SaveChanges();
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