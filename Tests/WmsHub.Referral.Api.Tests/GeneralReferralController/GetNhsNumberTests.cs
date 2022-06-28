using FluentAssertions;
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
using static WmsHub.Referral.Api.Models.GeneralReferral
  .GetNhsNumberConflictResponse;

namespace WmsHub.Referral.Api.Tests
{
  public class GetNhsNumberTests : GeneralReferralControllerTests
  {
    private const string EXPECTED_UBRN = "123456789012";
    private const string VALID_NHS_NUMBER = "9999999999";
    private const string EXPECTED_PROVIDER = "Expected Provider";
    private readonly DateTimeOffset EXPECTED_DATE_OF_REFERRAL =
      DateTimeOffset.Now;

    private Mock<IReferralService> _referralService = new();
    private GeneralReferralController _controller;
    private InUseResponse _inUseResponse = new();

    public GetNhsNumberTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _referralService
        .Setup(r => r.IsNhsNumberInUseAsync(It.IsAny<string>()))
        .ReturnsAsync(_inUseResponse);

      _controller = new(_referralService.Object, serviceFixture.Mapper);

      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext
        {
          User = GetGeneralReferralServiceClaimsPrincipal()
        }
      };
    }

    [Fact]
    public async Task NullClaimsPrincipal_NotAuthorised_401()
    {
      // arrange
      _controller.ControllerContext = new ControllerContext()
      {
        HttpContext = new DefaultHttpContext
        {
          User = null
        }
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var unauthorizedObjectResult = Assert
        .IsType<UnauthorizedObjectResult>(actionResult);
      unauthorizedObjectResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvalidClaimsPrincipal_NotAuthorised_401()
    {
      // arrange
      _controller.ControllerContext = new ControllerContext()
      {
        HttpContext = new DefaultHttpContext
        {
          User = GetInvalidClaimsPrincipal()
        }
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var unauthorizedObjectResult = Assert
        .IsType<UnauthorizedObjectResult>(actionResult);
      unauthorizedObjectResult.StatusCode.Should().Be(401);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("1234567890")]
    public async Task InvalidNhsNumber_400(string nhsNumber)
    {
      // act
      var actionResult = await _controller.GetNhsNumber(nhsNumber);

      // assert
      var objectResult = Assert.IsType<ObjectResult>(actionResult);
      objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Referral_NotFound_204()
    {
      // arrange
      _inUseResponse.Referral = null;

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var noContentResult = Assert.IsType<NoContentResult>(actionResult);
      noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task Referral_Found_Cancelled_ProviderNotSelected_204()
    {
      // arrange
      
      _inUseResponse.Referral = new Business.Models.Referral
      {
        DateOfReferral = EXPECTED_DATE_OF_REFERRAL,
        ProviderId = null,
        Provider = new Business.Models.Provider()
        {
          Name = EXPECTED_PROVIDER
        },
        ReferralSource = ReferralSource.GeneralReferral.ToString(),
        Status = ReferralStatus.CancelledByEreferrals.ToString(),
        Ubrn = EXPECTED_UBRN        
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var noContentResult = Assert.IsType<NoContentResult>(actionResult);
      noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }


    [Fact]
    public async Task Referral_Found_Cancelled_ProviderSelected_409()
    {
      // arrange
      _inUseResponse.Referral = new Business.Models.Referral
      {
        DateOfReferral = EXPECTED_DATE_OF_REFERRAL,
        ProviderId = Guid.NewGuid(),
        Provider = new Business.Models.Provider()
        {
          Name = EXPECTED_PROVIDER
        },
        ReferralSource = ReferralSource.GeneralReferral.ToString(),
        Status = ReferralStatus.CancelledByEreferrals.ToString(),
        Ubrn = EXPECTED_UBRN        
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult);
      conflictResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
      conflictResult.Value.Should().BeEquivalentTo(
        ExpectedNhsNumberConflictReponse(ErrorType.PreviousReferralCancelled));
    }

    [Fact]
    public async Task Referral_Found_Complete_409()
    {
      // arrange
      _inUseResponse.Referral = new Business.Models.Referral
      {
        DateOfReferral = EXPECTED_DATE_OF_REFERRAL,
        ProviderId = Guid.NewGuid(),
        Provider = new Business.Models.Provider()
        {
          Name = EXPECTED_PROVIDER
        },
        ReferralSource = ReferralSource.GeneralReferral.ToString(),
        Status = ReferralStatus.Complete.ToString(),
        Ubrn = EXPECTED_UBRN
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult);
      conflictResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
      conflictResult.Value.Should().BeEquivalentTo(
        ExpectedNhsNumberConflictReponse(ErrorType.PreviousReferralCompleted));
    }

    [Fact]
    public async Task Referral_Found_NotGeneral_409()
    {
      // arrange
      _inUseResponse.Referral = new Business.Models.Referral
      {
        DateOfReferral = EXPECTED_DATE_OF_REFERRAL,
        ProviderId = Guid.NewGuid(),
        Provider = new Business.Models.Provider()
        {
          Name = EXPECTED_PROVIDER
        },
        ReferralSource = ReferralSource.GpReferral.ToString(),
        Status = ReferralStatus.ProviderCompleted.ToString(),
        Ubrn = EXPECTED_UBRN
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult);
      conflictResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
      conflictResult.Value.Should().BeEquivalentTo(
        ExpectedNhsNumberConflictReponse(ErrorType.OtherReferralSource));
    }

    [Fact]
    public async Task Referral_Found_General_ProviderSelected_409()
    {
      // arrange
      _inUseResponse.Referral = new Business.Models.Referral
      {
        DateOfReferral = EXPECTED_DATE_OF_REFERRAL,
        ProviderId = Guid.NewGuid(),
        Provider = new Business.Models.Provider()
        {
          Name = EXPECTED_PROVIDER
        },
        ReferralSource = ReferralSource.GeneralReferral.ToString(),
        Status = ReferralStatus.ProviderAwaitingStart.ToString(),
        Ubrn = EXPECTED_UBRN
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult);
      conflictResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
      conflictResult.Value.Should().BeEquivalentTo(
        ExpectedNhsNumberConflictReponse(ErrorType.ProviderPreviouslySelected));
    }

    [Fact]
    public async Task Referral_Found_General_ProviderNotSelected_200()
    {
      // arrange
      _inUseResponse.Referral = new Business.Models.Referral
      {
        DateOfReferral = EXPECTED_DATE_OF_REFERRAL,
        Id = Guid.NewGuid(),
        ProviderId = null,
        ReferralSource = ReferralSource.GeneralReferral.ToString(),
        Status = ReferralStatus.New.ToString(),
        Ubrn = EXPECTED_UBRN
      };

      // act
      var actionResult = await _controller.GetNhsNumber(VALID_NHS_NUMBER);

      // assert
      var okResult = Assert.IsType<OkObjectResult>(actionResult);
      okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
      var okResultValue = Assert.IsType<GetNhsNumberOkResponse>(okResult.Value);
      okResultValue.Id.Should().Be(_inUseResponse.Referral.Id);
    }

    private GetNhsNumberConflictResponse ExpectedNhsNumberConflictReponse(
      ErrorType errorType)
    {
      return new()
      {
        Error = errorType,
        DateOfReferral = _inUseResponse.Referral.DateOfReferral,
        ProviderName = _inUseResponse.Referral.Provider.Name,
        ReferralSource = _inUseResponse.Referral.ReferralSource,
        Ubrn = _inUseResponse.Referral.Ubrn
      };
    }
  }
}
