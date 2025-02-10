using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Controllers;
using WmsHub.BusinessIntelligence.Api.Models;
using WmsHub.BusinessIntelligence.Api.Test;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.BusinessIntelligence.Api.Tests;

[Collection("Service collection")]
public class RmcControllerTests : ServiceTestsBase, IDisposable
{
  private readonly DatabaseContext _context;
  private readonly IBusinessIntelligenceService _service;
  private readonly RmcController _controller;
  private readonly Mock<IOptions<
    Business.Models.BusinessIntelligence.BusinessIntelligenceOptions>> 
      _mockOptions = new();
  private readonly Mock<
    Business.Models.BusinessIntelligence.BusinessIntelligenceOptions>
      _mockOptionsValues = new();

  public RmcControllerTests(
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

    _controller = new RmcController(
      _service,
      _serviceFixture.Mapper);

    AddUsersStoreFromJson();
    AddUserLogsFromJson();
  }

  [Fact]
  public async Task Get_InvalidDates_BadRequest()
  {
    // ARRANGE
    DateTimeOffset? fromDate = DateTimeOffset.Now.AddDays(-20);
    DateTimeOffset? toDate = DateTimeOffset.Now.AddDays(-40);
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.Providers.RemoveRange(_context.Providers);
    _context.ProviderSubmissions.RemoveRange(_context.ProviderSubmissions);

    var expectedMessage = 
      $"'from' date {fromDate.Value} cannot be later than 'to' date " +
      $"{toDate.Value}.";

    // Act.
    var result = await _controller.Get(fromDate, toDate);

    // Assert.
    using (new AssertionScope())
    {
      result.Should().NotBeNull();
      ObjectResult outputResultbad = 
        Assert.IsType<ObjectResult>(result.Result);
      ProblemDetails problemDetails =
        ((ObjectResult)result.Result).Value as ProblemDetails;
      problemDetails.Should().NotBeNull();
      problemDetails.Status.Should().Be(400);
      problemDetails.Detail.Should().Be(expectedMessage);

    }
  }

  [Fact]
  public async Task Get_DatesNull_ResultsFrom10kRowsLessThan10Seconds()
  {
    // Arrange.
    var ts = DateTime.UtcNow;
    
    // Act.
    var actionResult = await _controller.Get(null, null);
    
    // Assert.
    using (new AssertionScope())
    {
      actionResult.Should().NotBeNull();

      OkObjectResult result = Assert
        .IsType<OkObjectResult>(actionResult.Result);

      result.Should().NotBeNull();

      IEnumerable<BiRmcUserInformation> userActions =
        ConvertResponse(result);

      userActions.Any().Should().BeTrue();

      var timeTaken = DateTime.UtcNow - ts;

      timeTaken.Seconds.Should().BeLessOrEqualTo(10);
    }
  }

  [Fact]
  public async Task Get_BothDatesNull_ReturnsLast31Days()
  {
    // Arrange.
    DateTime expectedFrom = DateTime.UtcNow.AddDays(-31);
    DateTime expectedTo = DateTime.UtcNow;

    // Act.
    var actionResult = await _controller.Get(null, null);

    // Assert.
    using (new AssertionScope())
    {
      actionResult.Should().NotBeNull();

      OkObjectResult result = Assert
        .IsType<OkObjectResult>(actionResult.Result);

      result.Should().NotBeNull();

      IEnumerable<BiRmcUserInformation> userActions =
        ConvertResponse(result);

      userActions.Any().Should().BeTrue();

      foreach (BiRmcUserInformation userAction in userActions)
      {
        userAction.ActionDateTime.Date.Should().BeOnOrAfter(expectedFrom);
        userAction.ActionDateTime.Date.Should().BeOnOrBefore(expectedTo);
      }
    }
  }

  [Fact]
  public async Task Get_ToDateNullFromDateMinus10Days_ReturnsLast10Days()
  {
    // Arrange.
    DateTime expectedFrom = DateTime.UtcNow.AddDays(-10);
    DateTime expectedTo = DateTime.UtcNow;

    // Act.
    var actionResult = await _controller.Get(expectedFrom, null);

    // Assert.
    using (new AssertionScope())
    {
      actionResult.Should().NotBeNull();

      OkObjectResult result = Assert
        .IsType<OkObjectResult>(actionResult.Result);

      result.Should().NotBeNull();

      IEnumerable<BiRmcUserInformation> userActions =
        ConvertResponse(result);

      userActions.Any().Should().BeTrue();

      foreach (BiRmcUserInformation userAction in userActions)
      {
        userAction.ActionDateTime.Date.Should().BeOnOrAfter(expectedFrom);
        userAction.ActionDateTime.Date.Should().BeOnOrBefore(expectedTo);
      }
    }
  }

  private IEnumerable<BiRmcUserInformation> ConvertResponse(
    OkObjectResult result)
  {
    return result.Value as IEnumerable<BiRmcUserInformation>;
  }

  private void AddUsersStoreFromJson()
  {
    if (_context.UsersStore.Any()) 
      return;

    var entities = WmsHub.Tests.Helper.FakeGenerator.FakeUserStore();
    _context.UsersStore.AddRange(entities);
    _context.SaveChanges();
  }

  private void AddUserLogsFromJson()
  {
    if (_context.UserActionLogs.Any()) 
      return;
    var entities = WmsHub.Tests.Helper.FakeGenerator.FakeUserActionLogs();

    _context.UserActionLogs.AddRange(entities);
    _context.SaveChanges();

    var count = _context.UserActionLogs.Count();
    count.Should().BeGreaterThan(10000);
  }

  public void Dispose()
  {
    _context?.Dispose();
  }
}