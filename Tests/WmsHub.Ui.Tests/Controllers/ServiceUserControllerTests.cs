using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using WmsHub.Ui.Models;
using Xunit;

namespace WmsHub.Ui.Controllers.Tests;

public class ServiceUserControllerTests : ATheoryData
{
  private readonly Mock<IReferralService> _mockReferralService = new();
  private Mock<IOptions<WebUiSettings>> _mockSettings = new();

  public ServiceUserControllerTests()
  {
    _mockSettings.Setup(x => x.Value)
        .Returns(new WebUiSettings { Environment = "Staging" });
  }

  public class GetReferralWithValidation : ServiceUserControllerTests
  {
    private class FakeServiceUserController : ServiceUserController
    {
      public FakeServiceUserController(
        Mock<IReferralService> mockReferralService,
        Mock<IOptions<WebUiSettings>> mockSettings)
        : base(
          null,
          null,
          mockReferralService.Object,
          null,
          null,
          null,
          mockSettings.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = new ClaimsPrincipal(),
            Session = new Mock<ISession>().Object
          }
        };
      }

      public async Task<IReferral> GetReferralWithValidation(Guid id)
      {
        return await base.GetReferralWithValidation(id);
      }
    }

    private FakeServiceUserController _serviceUserController;

    public GetReferralWithValidation()
    {
      _serviceUserController = new(_mockReferralService, _mockSettings);
    }

    [Fact]
    public async Task IsProviderSelected_True_Exception()
    {
      // Arrange.
      Referral referral = new()
      {
        ProviderId = Guid.Empty,
        Id = Guid.Empty
      };

      _mockReferralService
        .Setup(x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
        .ReturnsAsync(referral);

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _serviceUserController.GetReferralWithValidation(referral.Id));

      // Assert.
      using (new AssertionScope())
      {
        ex.Should().BeOfType<ReferralProviderSelectedException>();
        ex.Message.Should().Contain(referral.Id.ToString());
        ex.Message.Should().Contain(referral.ProviderId.ToString());

        _mockReferralService.Verify(
          x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()),
          Times.Once());
      }
    }

    public static TheoryData<string> IsExceptionDueToEmailNotProvidedData
    {
      get
      {
        TheoryData<string> data = [];
        data.Add(null);
        data.Add(Constants.DO_NOT_CONTACT_EMAIL);
        return data;
      }
    }

    [Theory]
    [MemberData(nameof(IsExceptionDueToEmailNotProvidedData))]
    public async Task IsExceptionDueToEmailNotProvided_True_Exception(string outcome)
    {
      // Arrange.
      Referral referral = new()
      {
        ProviderId = null,
        Id = Guid.Empty,
        TextMessages = outcome == null ? null : [new() { Outcome = outcome }]
      };

      _mockReferralService
        .Setup(x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
        .ReturnsAsync(referral);

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _serviceUserController.GetReferralWithValidation(referral.Id));

      // Assert.
      using (new AssertionScope())
      {
        ex.Should().BeOfType<TextMessageExpiredByEmailException>();

        _mockReferralService.Verify(
          x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()),
          Times.Once());
      }
    }

    public static ReferralStatus[] ValidReferralStatuses
    {
      get
      {
        ReferralStatus[] data = new[]
        {
          ReferralStatus.TextMessage1,
          ReferralStatus.TextMessage2,
          ReferralStatus.ChatBotCall1,
          ReferralStatus.ChatBotTransfer,
          ReferralStatus.RmcCall,
          ReferralStatus.RmcDelayed,
          ReferralStatus.TextMessage3
        };
        return data;
      }
    }

    public static TheoryData<ReferralStatus> InvalidStatusesData =>
      ReferralStatusesTheoryData(excludedStatuses: ValidReferralStatuses);

    [Theory]
    [MemberData(nameof(InvalidStatusesData))]
    public async Task InvalidStatuses_Exception(ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = new()
      {
        ProviderId = null,
        Id = Guid.Empty,
        Status = referralStatus.ToString(),
        TextMessages = new() { new() }
      };

      _mockReferralService
        .Setup(x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
        .ReturnsAsync(referral);

      // Act.
      Exception ex = await Record.ExceptionAsync(
        () => _serviceUserController.GetReferralWithValidation(referral.Id));

      // Assert.
      using (new AssertionScope())
      {
        ex.Should().BeOfType<ReferralInvalidStatusException>();
        ex.Message.Should().Contain(referral.Status);

        _mockReferralService.Verify(
          x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()),
          Times.Once());
      }
    }

    public static TheoryData<ReferralStatus> ValidStatusesData
    {
      get
      {
        TheoryData<ReferralStatus> data = new();
        foreach (ReferralStatus referralStatus in ValidReferralStatuses)
        {
          data.Add(referralStatus);
        }
        return data;
      }
    }

    [Theory]
    [MemberData(nameof(ValidStatusesData))]
    public async Task ValidStatuses_ReferralReturned(
      ReferralStatus referralStatus)
    {
      // Arrange.
      Referral referral = new()
      {
        ProviderId = null,
        Id = Guid.Empty,
        Status = referralStatus.ToString(),
        TextMessages = new() { new() }
      };

      _mockReferralService
        .Setup(x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
        .ReturnsAsync(referral);

      // Act.
      IReferral result = await _serviceUserController
        .GetReferralWithValidation(referral.Id);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeEquivalentTo(referral);

        _mockReferralService.Verify(
          x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()),
          Times.Once());
      }
    }
  }
}