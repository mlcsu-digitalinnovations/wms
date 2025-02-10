using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using WmsHub.Business;
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
using WmsHub.Business.Models.BusinessIntelligence;

namespace WmsHub.BusinessIntelligence.Api.Tests;

[Collection("Service collection")]
public class BusinessIntelligenceReferralControllerTests :
  ServiceTestsBase,
  IDisposable
{
  protected readonly DatabaseContext _context;
  protected readonly IBusinessIntelligenceService _service;
  protected readonly ReferralController _controller;
  private readonly Mock<IOptions<BusinessIntelligenceOptions>> _mockOptions =
  new();
  private readonly Mock<BusinessIntelligenceOptions> _mockOptionsValues =
    new();

  public BusinessIntelligenceReferralControllerTests(
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

    _controller = new ReferralController(
      _service,
      _serviceFixture.Mapper);
  }

  [Fact]
  public async Task Get_Returns_NoContentResult()
  {
    // Arrange.
    DateTime fromDate = new(8192, 1, 1);
    DateTime toDate = new(8192, 1, 2);

    // Act.
    ActionResult<IEnumerable<AnonymisedReferral>> result = await _controller.Get(fromDate, toDate);

    // Assert.
    result.Should().NotBeNull();
    result.Result.Should().BeOfType<NoContentResult>();
  }

  [Fact]
  public async Task Get_ReturnsAViewModel_WithAListOfAnonymisedReferrals()
  {
    // Arrange.
    Clear(_context);
    Setup(_context);
    int offsetOne = -90;
    int offsetTwo = -50;
    DateTimeOffset? fromDate = new DateTimeOffset(new DateTime(2020, 1, 1));
    DateTimeOffset? toDate = DateTimeOffset.Now;
    int expectedCount = 2;

    await AddTestReferral(_context, _providers[0].Id, "Test", offsetOne);
    await AddTestReferral(
      _context,
      _providers[1].Id,
      "Ambulance Worker",
      offsetTwo);

    // Act.
    var result = await _controller.Get(fromDate, toDate);

    // Assert.
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
    // Arrange.
    Clear(_context);
    Setup(_context);
    int offsetOne = -90;
    int offsetTwo = -50;
    DateTimeOffset? fromDate = DateTime.Now.AddDays(offsetOne + 10);
    DateTimeOffset? toDate = null;
    int expectedCount = 1;
    int expected = 200;

    await AddTestReferral(_context, _providers[0].Id, "Test", offsetOne);
    await AddTestReferral(
      _context,
      _providers[1].Id,
      "Ambulance Worker",
      offsetTwo);

    // Act.
    var result = await _controller.Get(fromDate, toDate);

    // Assert.
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      result.Result.Should().BeOfType<OkObjectResult>();
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      okObjectResult.Should().NotBeNull();
      List<AnonymisedReferral> anons =
        (List<AnonymisedReferral>)okObjectResult.Value;
      anons.Count.Should().Be(expectedCount);
      okObjectResult.StatusCode.Should().Be(expected);
    }
  }

  [Fact]
  public async Task Get_ReturnsAViewModel_usingToDate()
  {
    // Arrange.
    Clear(_context);
    Setup(_context);
    int offsetDays = -90;
    DateTimeOffset? fromDate = null;
    DateTimeOffset? toDate = DateTime.Now.AddDays(offsetDays + 10);
    int expectedCount = 1;
    int expected = 200;
    await AddTestReferral(_context, _providers[0].Id, "Test", offsetDays);
    await AddTestReferral(
      _context,
      _providers[1].Id,
      "Ambulance Worker",
      offsetDays + 20);

    // Act.
    var result = await _controller.Get(fromDate, toDate);

    // Assert.
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      result.Result.Should().BeOfType<OkObjectResult>();
      OkObjectResult okObjectResult = result.Result as OkObjectResult;
      okObjectResult.Should().NotBeNull();
      List<AnonymisedReferral> anons =
        (List<AnonymisedReferral>)okObjectResult.Value;
      anons.Count.Should().Be(expectedCount);
      okObjectResult.StatusCode.Should().Be(expected);
    }
  }

  [Fact]
  public async Task Get_ReturnsAViewModel_usingBothToDates()
  {
    // Arrange.
    Clear(_context);
    Setup(_context);
    int offsetOne = -90;
    int offsetTwo = -50;
    DateTimeOffset? fromDate = DateTime.Now.AddDays(offsetOne - 10);
    DateTimeOffset? toDate = DateTime.Now.AddDays(offsetTwo + 10);
    int expectedCount = 2;
    int expected = 200;
    await AddTestReferral(_context, _providers[0].Id, "Test", offsetOne);
    await AddTestReferral(
      _context,
      _providers[1].Id,
      "Ambulance Worker",
      offsetTwo);

    // Act.
    var result = await _controller.Get(fromDate, toDate);

    // Assert.
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      result.Result.Should().BeOfType<OkObjectResult>();
      OkObjectResult okResult = result.Result as OkObjectResult;
      okResult.Should().NotBeNull();
      okResult.Value.Should().BeOfType<List<AnonymisedReferral>>();
      List<AnonymisedReferral> anons =
        (List<AnonymisedReferral>)okResult.Value;
      anons.Count.Should().Be(expectedCount);
      okResult.StatusCode.Should().Be(expected);
    }
  }

  [Fact]
  public async Task Get_ReturnsModel_With_Ethnicity()
  {
    // Arrange.
    Clear(_context);
    Setup(_context);
    int offsetOne = -90;
    int offsetTwo = -50;
    DateTimeOffset? fromDate = new DateTimeOffset(new DateTime(2020, 1, 1));
    DateTimeOffset? toDate = DateTimeOffset.Now;
    int expectedCount = 2;

    EthnicityGrouping eg1 =
      Generators.GenerateEthnicityGrouping(new Random());
    string ubrn1 = Generators.GenerateUbrnGp(new Random());
    EthnicityGrouping eg2 =
      Generators.GenerateEthnicityGrouping(new Random());
    string ubrn2 = Generators.GenerateUbrnGp(new Random());
    await AddTestReferral(
      _context,
      _providers[0].Id,
      "Test",
      offsetOne,
      eg1,
      ubrn1);
    await AddTestReferral(
      _context,
      _providers[1].Id,
      "Ambulance Worker",
      offsetTwo,
      eg2,
      ubrn2);

    // Act.
    var result = await _controller.Get(fromDate, toDate);

    // Assert.
    OkObjectResult okObjectResult = result.Result as OkObjectResult;
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      okObjectResult.Should().NotBeNull();
      List<AnonymisedReferral> anonReferrals =
        (List<AnonymisedReferral>)okObjectResult.Value;
      anonReferrals.Count.Should().Be(expectedCount);
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

  [Fact]
  public async Task Get_NoContent204()
  {
    // Arrange.
    DateTimeOffset? fromDate = null;
    DateTimeOffset? toDate = null;
    int expected = 204;
    _context.Referrals.RemoveRange(_context.Referrals);
    await _context.SaveChangesAsync();

    // Act.
    var result = await _controller.Get(fromDate, toDate);

    // Assert.
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      NoContentResult returned = result.Result as NoContentResult;
      returned.Should().NotBeNull();
      returned.StatusCode.Should().Be(expected);
    }
  }

  public void Dispose()
  {
    _context?.Dispose();
  }

  public class GetAnonymisedReferralsByLastModifiedDateAsync
    : BusinessIntelligenceReferralControllerTests
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
      ReferralController controller =
        new ReferralController(mockService.Object, null);
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