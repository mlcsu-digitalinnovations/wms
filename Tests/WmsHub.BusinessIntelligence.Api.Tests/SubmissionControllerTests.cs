using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Controllers;
using WmsHub.BusinessIntelligence.Api.Models;
using WmsHub.BusinessIntelligence.Api.Test;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.Extensions.Options;

namespace WmsHub.BusinessIntelligence.Api.Tests;

[Collection("Service collection")]
public class SubmissionControllerTests : ServiceTestsBase, IDisposable
{
  protected readonly DatabaseContext _context;
  protected readonly IBusinessIntelligenceService _service;
  protected readonly SubmissionController _controller;
  private readonly Mock<IOptions<
 Business.Models.BusinessIntelligence.BusinessIntelligenceOptions>>
   _mockOptions = new();
  private readonly Mock<
    Business.Models.BusinessIntelligence.BusinessIntelligenceOptions>
      _mockOptionsValues = new();

  public SubmissionControllerTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(_serviceFixture.Options);
    _mockOptionsValues.Setup(t => t.ProviderSubmissionEndedStatusesValue)
      .Returns($"{ReferralStatus.ProviderRejected}," +
      $"{ReferralStatus.ProviderDeclinedByServiceUser}," +
      $"{ReferralStatus.ProviderTerminated}");
    _mockOptions.Setup(t => t.Value).Returns(_mockOptionsValues.Object);
    _service = new BusinessIntelligenceService(
      _context,
      _serviceFixture.Mapper,
      _mockOptions.Object,
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
    // Arrange.
    DateTimeOffset? fromDate = DateTimeOffset.Now.AddDays(-20);
    DateTimeOffset? toDate = DateTimeOffset.Now.AddDays(-40);
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.Providers.RemoveRange(_context.Providers);
    _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

    string expectedMessage = $"'from' date {fromDate.Value} cannot be " +
                             $"later than 'to' date {toDate.Value}.";

    // Act.
    ActionResult<IEnumerable<AnonymisedReferral>> result = 
      await _controller.Get(fromDate, toDate);

    // Assert.
    Assert.NotNull(result);
    ObjectResult outputResultbad = Assert.IsType<ObjectResult>(result.Result);
    ProblemDetails problemDetails = 
      ((ObjectResult)result.Result).Value as ProblemDetails;
    Assert.NotNull(problemDetails);
    Assert.True(problemDetails.Status == 400);
    Assert.Equal(expectedMessage, problemDetails.Detail);
  }

  [Fact]
  public async Task Get_ReturnsListOfAnonymisedReferrals_WithBothFilterDate()
  {
    // Arrange.
    Clear(_context);
    Setup(_context);
    DateTime? fromDate = DateTime.Now.AddDays(-30);
    DateTime? toDate = DateTime.Now.AddDays(-5);

    await AddTestReferralWithSubmission(
      _context, 
      _providers[0].Id, 
      "Doctor", 
      -30);
    await AddTestReferralWithSubmission(
      _context, 
      _providers[1].Id, 
      "Porter", 
      -30);

    // Act.
    ActionResult<IEnumerable<AnonymisedReferral>> result = 
      await _controller.Get(fromDate, toDate);

    // Assert.
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      result.Result.Should().BeOfType<OkObjectResult>();
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      okObjectResult.Should().NotBeNull();
      okObjectResult.Value.Should().
        BeOfType<List<Models.AnonymisedReferral>>();
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      anonReferrals.Count.Should()
        .Be(anonReferrals[0].ProviderSubmissions.Count());
      foreach (AnonymisedReferral anonReferral in anonReferrals)
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
    
  }

  [Fact]
  public async Task Get_ReturnsListOfAnonymisedReferrals_WithFromFilterDate()
  {
    // Arrange.
    Clear(_context);
    Setup(_context);
    DateTime? fromDate = DateTime.Now.AddDays(-35);
    DateTime? toDate = null;
    int expectedReferralCount = 2;
    int expectedProviderSubCount = 2;

    await AddTestReferralWithSubmission(
      _context, 
      _providers[0].Id, 
      "Doctor", 
      -30);
    await AddTestReferralWithSubmission(
      _context, 
      _providers[1].Id, 
      "Porter", 
      -30);

    // Act.
    ActionResult<IEnumerable<AnonymisedReferral>> result = 
      await _controller.Get(fromDate, toDate);

    // Assert.
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
    // Arrange.
    Clear(_context);
    Setup(_context);
    DateTime? fromDate = null;
    DateTime? toDate = DateTime.Now.AddDays(-6);
    int expectedReferralCount = 2;
    int expectedProviderSubCount = 2;

    await AddTestReferralWithSubmission(
      _context, 
      _providers[0].Id,
      "Doctor", 
      -30);
    await AddTestReferralWithSubmission(
      _context, 
      _providers[1].Id, 
      "Porter", 
      -30);

    // Act.
    ActionResult<IEnumerable<AnonymisedReferral>> result = 
      await _controller.Get(fromDate, toDate);

    // Assert.
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
  public async Task Get_ReturnsModel_With_Ethnicity()
  {
    // Arrange.
    Clear(_context);
    Setup(_context);
    DateTime? fromDate = null;
    DateTime? toDate = DateTime.Now.AddDays(-6);
    int expectedReferralCount = 2;

    EthnicityGrouping eg1 =
      Generators.GenerateEthnicityGrouping(new Random());
    string ubrn1 = Generators.GenerateUbrnGp(new Random());
    EthnicityGrouping eg2 =
      Generators.GenerateEthnicityGrouping(new Random());
    string ubrn2 = Generators.GenerateUbrnGp(new Random());
    await AddTestReferralWithSubmission(
      _context, 
      _providers[0].Id, 
      "Doctor", 
      -30, 
      eg1);
    await AddTestReferralWithSubmission(
      _context, 
      _providers[1].Id, 
      "Ambulance Worker", 
      -30, 
      eg2);

    // Act.
    ActionResult<IEnumerable<AnonymisedReferral>> result = 
      await _controller.Get(fromDate, toDate);

    // Assert.
    OkObjectResult okObjectResult = result.Result as OkObjectResult;
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      okObjectResult.Should().NotBeNull();
      List<Models.AnonymisedReferral> anonReferrals =
        (List<Models.AnonymisedReferral>)okObjectResult.Value;
      anonReferrals.Count.Should().Be(expectedReferralCount);
      okObjectResult.StatusCode.Should().Be(200);
      foreach (AnonymisedReferral r in anonReferrals)
      {
        r.Ethnicity.Should().NotBeNullOrWhiteSpace();
        r.ServiceUserEthnicity.Should().NotBeNullOrWhiteSpace();
        r.ServiceUserEthnicityGroup.Should().NotBeNullOrWhiteSpace();
        if (r.Ubrn == ubrn1)
        {
          r.Ethnicity.Should().Be(eg1.Ethnicity);
          r.ServiceUserEthnicity.Should().Be(eg1.ServiceUserEthnicity);
          r.ServiceUserEthnicityGroup.Should()
            .Be(eg1.ServiceUserEthnicityGroup);
        }
        else if (r.Ubrn == ubrn2)
        {
          r.Ethnicity.Should().Be(eg2.Ethnicity);
          r.ServiceUserEthnicity.Should().Be(eg2.ServiceUserEthnicity);
          r.ServiceUserEthnicityGroup.Should()
            .Be(eg2.ServiceUserEthnicityGroup);
        }
      }
    }

  }

  protected async Task AddTestReferralWithSubmission(
    DatabaseContext context,
    Guid providerId,
    string staffRole,
    int offset,
    EthnicityGrouping ethnictyGrouping = null)
  {
   Guid id =  await AddTestReferral(context, providerId, staffRole, offset, 
     ethnictyGrouping);

   ProviderSubmission[] submissions = await _context.ProviderSubmissions.
     Where(t => t.ProviderId == providerId).ToArrayAsync();

   foreach (ProviderSubmission submission in submissions)
   {
     submission.ReferralId = id;
   }

   await context.SaveChangesAsync();
  }

  protected override void Setup(DatabaseContext context)
  {
    AddStaffRoles(context);

    Guid p1 = AddProvider(context, "Provider One");
    Guid p2 = AddProvider(context, "Provider Two");
    
    AddProviderSubmission(context, p1, 5, DateTime.Now.AddDays(-28), 1, 87);
    AddProviderSubmission(context, p1, 6, DateTime.Now.AddDays(-14), 7, 97);
    AddProviderSubmission(context, p2, 5, DateTime.Now.AddDays(-28), 1, 87);
    AddProviderSubmission(context, p2, 6, DateTime.Now.AddDays(-14), 7, 97);
  }

  [Fact]
  public async Task Get_NoContent204()
  {
    // Arrange.
    DateTimeOffset? fromDate = null;
    DateTimeOffset? toDate = null;

    _context.Referrals.RemoveRange(_context.Referrals);
    await _context.SaveChangesAsync();

    // Act.
    ActionResult<IEnumerable<AnonymisedReferral>> result = 
      await _controller.Get(fromDate, toDate);

    // Assert.
    Assert.NotNull(result);
    NoContentResult returned = result.Result as NoContentResult;
    Assert.NotNull(returned);
    Assert.True(returned.StatusCode == 204);
  }

  public void Dispose()
  {
    _context?.Dispose();
  }

  public class GetAnonymisedReferralsByLastModifiedDateAsync
    : SubmissionControllerTests
  {
    public GetAnonymisedReferralsByLastModifiedDateAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    { }

    [Fact]
    public async Task Null_BadRequest()
    {
      // Arrange.
      string expectedDetail = "lastDownloadDate is required.";

      // Act.
      IActionResult result = await _controller.GetChanges(null);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task FutureDate_BadRequest()
    {
      // Arrange.
      string expectedDetail = "lastDownloadDate cannot be in future.";

      // Act.
      IActionResult result = await _controller.GetChanges(
        DateTimeOffset.Now.AddDays(1));

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task InternalServerError()
    {
      // Arrange.
      Mock<IBusinessIntelligenceService> mockService = new();
      SubmissionController controller = new(mockService.Object, null);
      mockService.Setup(x =>
        x.GetAnonymisedReferralsChangedFromDate(
          It.IsAny<DateTimeOffset>())).ThrowsAsync(new Exception());

      // Act.
      IActionResult result = await controller.GetChanges(
        DateTimeOffset.Now.AddDays(-10));

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }
  }
}