using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Referral.Api.Controllers;
using WmsHub.Referral.Api.Models.GeneralReferral;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Referral.Api.Tests;

public class GetNhsNumberTests : GeneralReferralControllerTests
{
  private readonly Mock<IReferralService> _referralService = new();
  private GeneralReferralController _controller;

  public GetNhsNumberTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _controller = new(_referralService.Object, serviceFixture.Mapper)
    {
      ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext
        {
          User = GetGeneralReferralServiceClaimsPrincipal()
        }
      }
    };
  }

  [Fact]
  public async Task CanCreateReferral_204()
  {
    // Arrange.
    CanCreateReferralResponse response = new(
      CanCreateReferralResult.CanCreate,
      string.Empty);

    _referralService
      .Setup(x => x
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(It.IsAny<string>()))
      .ReturnsAsync(response);

    GetNhsNumberRequest request = new();

    // Act.
    IActionResult actionResult = await _controller.GetNhsNumber(request);

    // Assert.
   actionResult.Should().BeOfType<NoContentResult>()
      .Which.StatusCode.Should().Be(StatusCodes.Status204NoContent);
  }

  [Theory]
  [InlineData(CanCreateReferralResult.IneligibleReferralSource)]
  [InlineData(CanCreateReferralResult.ProgrammeStarted)]
  [InlineData(CanCreateReferralResult.ProviderSelected)]
  public async Task CannotCreateReferral_409(
    CanCreateReferralResult canCreateReferralResult)
  {
    // Arrange.
    CanCreateReferralResponse canCreateReferralResponse = new(
      canCreateReferralResult,
      $"{canCreateReferralResult}",
      new Business.Models.Referral()
      {
        DateOfReferral = DateTimeOffset.Now,
        Provider = new Business.Models.Provider() { Name = "TestProvider" },
        ReferralSource = ReferralSource.GpReferral.ToString(),
        Ubrn = "000000000001",
      });

    _referralService
      .Setup(x => x
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(It.IsAny<string>()))
      .ReturnsAsync(canCreateReferralResponse);

    GetNhsNumberRequest request = new();

    // Act.
    IActionResult actionResult = await _controller.GetNhsNumber(request);

    // Assert.
    ConflictObjectResult conflictObjectResult = Assert
      .IsType<ConflictObjectResult>(actionResult);

    GetNhsNumberConflictResponse response = Assert
      .IsType<GetNhsNumberConflictResponse>(conflictObjectResult.Value);

    using (new AssertionScope())
    {
      conflictObjectResult.StatusCode.Should()
        .Be(StatusCodes.Status409Conflict);

      response.DateOfReferral.Should()
        .Be(canCreateReferralResponse.Referral.DateOfReferral);
      response.Error.Should()
        .Be(canCreateReferralResponse.CanCreateResult);
      response.ErrorDescription.Should()
        .Be(canCreateReferralResponse.Reason);
      response.ProviderName.Should()
        .Be(canCreateReferralResponse.Referral.Provider.Name);
      response.ReferralSource.Should()
        .Be(canCreateReferralResponse.Referral.ReferralSource);
      response.Ubrn.Should().Be(canCreateReferralResponse.Referral.Ubrn);
    }
  }

  [Fact]
  public async Task UpdateExistingReferral_200()
  {
    // Arrange.
    CanCreateReferralResponse canCreateReferralResponse = new(
      CanCreateReferralResult.UpdateExisting,
      $"{CanCreateReferralResult.UpdateExisting}",
      new Business.Models.Referral()
      {
        ConsentForGpAndNhsNumberLookup = false,
        HeightCm = null,
        WeightKg = null,
      });

    _referralService
      .Setup(x => x
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(It.IsAny<string>()))
      .ReturnsAsync(canCreateReferralResponse);

    GetNhsNumberRequest request = new();

    // Act.
    IActionResult actionResult = await _controller.GetNhsNumber(request);

    // Assert.
    OkObjectResult okObjectResult = Assert
      .IsType<OkObjectResult>(actionResult);

    GetNhsNumberOkResponse response = Assert
      .IsType<GetNhsNumberOkResponse>(okObjectResult.Value);

    using (new AssertionScope())
    {
      okObjectResult.StatusCode.Should().Be(StatusCodes.Status200OK);

      response.Should().BeEquivalentTo(
        canCreateReferralResponse.Referral,
        options => options.ExcludingMissingMembers());
    }
  }
}
