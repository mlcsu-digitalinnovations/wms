using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Models.ReferralStatusReason;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Tests.Helper;
using WmsHub.Ui.Controllers;
using WmsHub.Ui.Models;
using WmsHub.Ui.Models.Profiles;
using Xunit;
using static WmsHub.Common.Helpers.Constants;
using Provider = WmsHub.Ui.Models.Provider;

namespace WmsHub.Ui.Tests.Controllers;

public class RmcControllerTests : ABaseTests
{
  private RmcController _classToTest;
  private readonly Mock<IReferralService> _mockReferralService = new();
  private readonly Mock<IProviderService> _mockProviderService = new();
  private readonly Mock<IEthnicityService> _mockEthnicityService = new();
  private readonly Mock<INotificationService> _mockNotificationService = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IOptions<WebUiSettings>> _mockOptions = new();
  private readonly Mock<IOptions<NotificationOptions>> _mockTempOptions =
    new();
  private readonly Mock<ILogger<RmcController>> _mockLogger;

  public RmcControllerTests()
  {
    _mockLogger = new Mock<ILogger<RmcController>>();
    _mockOptions.Setup(x => x.Value)
      .Returns(new WebUiSettings { Environment = "Staging" });
  }

  public class MapperConfigTests
  {
    //[Fact] TODO: #1619 BUG reported
    public void ProviderProfile_Valid()
    {
      // Arrange.
      Mock<Provider> providerModel = new();
      providerModel.Object.Id = Guid.NewGuid();
      Mock<Business.Models.Provider> provider = new();
      provider.Object.Id = providerModel.Object.Id;
      provider.Object.Summary = "Test Summary 1";
      provider.Object.Summary2 = "Test Summary 2";
      provider.Object.Summary3 = "Test Summary 3";
      Mock<IReferral> referral = new();
      List<Business.Models.Provider> providers =
        new() { provider.Object };
      referral.Setup(t => t.Providers).Returns(providers);

      // Act.
      MapperConfiguration config = 
        new(cfg => cfg.AddProfile<ProviderProfile>());

      IMapper mapper = config.CreateMapper();
      List<Provider> result = mapper.Map<List<Provider>>(referral);
    }
  }

  public class ConfirmDelayTests : RmcControllerTests
  {
    public IMapper Mapper { get; set; }
    public ConfirmDelayTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.Ui"
        })
      );
      Mapper = config.CreateMapper();

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    [Fact]
    public async Task DelayReferralException_ModelStateError()
    {
      // Arrange.
      Business.Entities.Referral referralEntity =
        RandomEntityCreator.CreateRandomReferral(
          id: Guid.NewGuid(),
          status: ReferralStatus.RmcCall);

      Referral referral =
        Mapper.Map<Business.Entities.Referral, Referral>(referralEntity);

      List<Guid> idParameters = new();

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(
        It.IsAny<Guid>()))
        .ReturnsAsync(referral);

      _mockReferralService.Setup(x => x.GetProviderNameAsync(
        It.IsAny<Guid>()))
        .ReturnsAsync(string.Empty);

      _mockReferralService.Setup(x => x.GetReferralAuditForServiceUserAsync(
        It.IsAny<Guid>()))
        .ReturnsAsync(new List<ReferralAudit>());

      _mockReferralService.Setup(t => t.DelayReferralUntilAsync(
        It.IsAny<Guid>(),
        It.IsAny<string>(),
        It.IsAny<DateTimeOffset>()))
        .ThrowsAsync(new DelayReferralException());

      ReferralListItemModel model = new()
      {
        DelayUntil = DateTimeOffset.Now.AddDays(1),
        DelayReason = "Test Delay",
        Id = referralEntity.Id
      };

      string expectedViewName = "ReferralView";
      string partialExpectedModelStateError =
        "The delay has been cancelled and the referral refreshed to show " +
        "the changes.";
      string stateKey = "There was a problem when delaying the referral.";

      // Act.
      IActionResult result = await _classToTest.ConfirmDelay(model);

      // Assert.
      using (new AssertionScope())
      {
        _mockReferralService.Verify(x =>
          x.GetReferralWithTriagedProvidersById(model.Id),
          Times.Once);

        _mockReferralService.Verify(x => x.GetProviderNameAsync(
          It.IsAny<Guid>()),
          Times.Never);

        _mockReferralService.Verify(x =>
          x.GetReferralAuditForServiceUserAsync(model.Id),
          Times.Once);

        _mockReferralService.Verify(t => t.DelayReferralUntilAsync(
          model.Id,
          model.DelayReason,
          model.DelayUntil.Value),
          Times.Once);

        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        viewResult.ViewName.Should().Be(expectedViewName);

        ModelStateDictionary state = viewResult.ViewData.ModelState;
        state.ErrorCount.Should().Be(1);

        state[stateKey].Errors[0].ErrorMessage.Should()
          .Contain(partialExpectedModelStateError);
      }
    }

    [Fact]
    public async Task DelayReferralValid_ReferralDelayed()
    {
      // Arrange.
      Business.Entities.Referral referralEntity =
        RandomEntityCreator.CreateRandomReferral(
          id: Guid.NewGuid(),
          status: ReferralStatus.RmcCall);

      Referral referral =
        Mapper.Map<Business.Entities.Referral, Referral>(referralEntity);

      List<Guid> idParameters = new();

      _mockReferralService.Setup(t => t.DelayReferralUntilAsync(
        It.IsAny<Guid>(),
        It.IsAny<string>(),
        It.IsAny<DateTimeOffset>()))
        .ReturnsAsync(referral);

      ReferralListItemModel model = new()
      {
        DelayUntil = DateTimeOffset.Now.AddDays(1),
        DelayReason = "Test Delay",
        Id = referralEntity.Id
      };

      string expectedActionName = "referralList";
      string expectedControllerName = "rmc";

      // Act.
      IActionResult result = await _classToTest.ConfirmDelay(model);

      // Assert.
      using (new AssertionScope())
      {
        _mockReferralService.Verify(t => t.DelayReferralUntilAsync(
          model.Id,
          model.DelayReason,
          model.DelayUntil.Value),
          Times.Once);

        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();

        RedirectToActionResult redirectToActionResult = Assert
          .IsType<RedirectToActionResult>(result);
        redirectToActionResult.ActionName.Should().Be(expectedActionName);
        redirectToActionResult.ControllerName.Should()
          .Be(expectedControllerName);
      }
    }
  }

  public class ConfirmEthnicityTests : RmcControllerTests
  {
    public IMapper Mapper { get; set; }

    public ConfirmEthnicityTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.Ui"
        })
      );
      Mapper = config.CreateMapper();

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    [Fact]
    public async Task BmiTooLowSetsStatusReason()
    {
      // Arrange.
      string displayName = "displayName";
      string groupName = "groupName";
      string triageName = "triageName";
      decimal minimumBmi = 27.5m;

      Business.Models.Ethnicity ethnicity = new()
      {
        DisplayName = displayName,
        GroupName = groupName,
        MinimumBmi = minimumBmi,
        TriageName = triageName
      };

      _mockEthnicityService.Setup(x => x.GetByMultiple(It.IsAny<string>()))
        .ReturnsAsync(ethnicity)
        .Verifiable();
      _mockEthnicityService.Setup(x => x.GetEthnicityGroupNamesAsync())
        .ReturnsAsync([groupName])
        .Verifiable();
      _mockEthnicityService.Setup(x => x.GetEthnicityGroupMembersAsync(groupName))
        .ReturnsAsync([ethnicity])
        .Verifiable();

      decimal bmi = 20.0m;
      IReferral referral = new Referral()
      {
        IsBmiTooLow = true,
        CalculatedBmiAtRegistration = bmi,
        Ethnicity = triageName,
        Id = Guid.NewGuid(),
        SelectedEthnicGroupMinimumBmi = minimumBmi,
        ServiceUserEthnicity = displayName,
        ServiceUserEthnicityGroup = groupName
      };

      ReferralListItemModel model = new()
      {
        Id = referral.Id,
        SelectedServiceUserEthnicity = displayName
      };

      _mockReferralService.Setup(x => x.UpdateEthnicity(
        model.Id,
        ethnicity))
        .ReturnsAsync(referral)
        .Verifiable();

      _mockReferralService.Setup(x => x.GetRmcRejectedReferralStatusReasonsAsync())
        .ReturnsAsync(It.IsAny<ReferralStatusReason[]>());

      _mockReferralService.Setup(x => x.GetReferralAuditForServiceUserAsync(model.Id))
        .ReturnsAsync([]);

      string expectedStatusReason = $"The service user's BMI of {bmi} is " +
              $"below the minimum of {minimumBmi} " +
              $"for the selected ethnic group of {ethnicity.TriageName}.";

      // Act.
      IActionResult result = await _classToTest.ConfirmEthnicity(model);

      // Assert.
      _mockReferralService.Verify();
      _mockEthnicityService.Verify();
      ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
      viewResult.ViewName.Should().Be("ReferralView");
      ReferralListItemModel viewResultModel =
        viewResult.Model.Should().BeOfType<ReferralListItemModel>().Subject;
      viewResultModel.SelectedEthnicity.Should().Be(triageName);
      SelectListItem groupList = viewResultModel.ServiceUserEthnicityGroupList.Should()
        .ContainSingle()
        .Subject;
      groupList.Value.Should().Be(groupName);
      groupList.Text.Should().Be(groupName);
      SelectListItem ethnicityList = viewResultModel.ServiceUserEthnicityList.Should()
        .ContainSingle()
        .Subject;
      ethnicityList.Value.Should().Be(displayName);
      ethnicityList.Text.Should().Be(displayName);
      viewResultModel.StatusReason.Should().Be(expectedStatusReason);
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Trace),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == "Ethnicity Updated"),
          null,
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

    }

    [Fact]
    public async Task InvalidSelectServiceUserEthnicityLogsError()
    {
      // Arrange.
      string selectedServiceUserEthnicity = "invalidDisplayName";
      string expectedMessage = "An ethnicity with a DisplayName of " +
        $"{selectedServiceUserEthnicity} could not be found.";

      _mockEthnicityService.Setup(x => x.GetByMultiple(It.IsAny<string>()))
        .Returns(Task.FromResult<Business.Models.Ethnicity>(null));

      ReferralListItemModel model = new()
      {
        SelectedServiceUserEthnicity = selectedServiceUserEthnicity
      };

      // Act.
      IActionResult result = await _classToTest.ConfirmEthnicity(model);

      // Assert.
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Error),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
          It.IsAny<Exception>(),
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
      result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("ReferralView");
    }

    [Fact]
    public async Task ModelStateIsNotValidLogsError()
    {
      // Arrange.
      string errorMessage = "Id is not valid.";
      _classToTest.ModelState.AddModelError("Id", errorMessage);

      ReferralListItemModel model = new();
      
      // Act.
      IActionResult result = await _classToTest.ConfirmEthnicity(model);

      // Assert.
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Error),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == errorMessage),
          It.IsAny<Exception>(),
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
      result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("ReferralView");
    }

    [Fact]
    public async Task ValidCallsUpdateEthnicityAndReturnsModel()
    {
      // Arrange.
      string displayName = "displayName";
      string groupName = "groupName";
      string triageName = "triageName";

      Business.Models.Ethnicity ethnicity = new()
      {
        DisplayName = displayName,
        GroupName = groupName,
        TriageName = triageName
      };

      _mockEthnicityService.Setup(x => x.GetByMultiple(It.IsAny<string>()))
        .ReturnsAsync(ethnicity)
        .Verifiable();
      _mockEthnicityService.Setup(x => x.GetEthnicityGroupNamesAsync())
        .ReturnsAsync([groupName])
        .Verifiable();
      _mockEthnicityService.Setup(x => x.GetEthnicityGroupMembersAsync(groupName))
        .ReturnsAsync([ethnicity])
        .Verifiable();

      IReferral referral = new Referral()
      {
        CalculatedBmiAtRegistration = 35.0m,
        Ethnicity = triageName,
        Id = Guid.NewGuid(),
        ServiceUserEthnicity = displayName,
        ServiceUserEthnicityGroup = groupName
      };

      ReferralListItemModel model = new()
      {
        Id = referral.Id,
        SelectedServiceUserEthnicity = displayName
      };

      _mockReferralService.Setup(x => x.UpdateEthnicity(
        model.Id,
        ethnicity))
        .ReturnsAsync(referral)
        .Verifiable();

      _mockReferralService.Setup(x => x.GetRmcRejectedReferralStatusReasonsAsync())
        .ReturnsAsync(It.IsAny<ReferralStatusReason[]>());

      _mockReferralService.Setup(x => x.GetReferralAuditForServiceUserAsync(model.Id))
        .ReturnsAsync([]);

      // Act.
      IActionResult result = await _classToTest.ConfirmEthnicity(model);

      // Assert.
      _mockReferralService.Verify();
      _mockEthnicityService.Verify();
      ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
      viewResult.ViewName.Should().Be("ReferralView");
      ReferralListItemModel viewResultModel = 
        viewResult.Model.Should().BeOfType<ReferralListItemModel>().Subject;
      viewResultModel.SelectedEthnicity.Should().Be(triageName);
      SelectListItem groupList = viewResultModel.ServiceUserEthnicityGroupList.Should()
        .ContainSingle()
        .Subject;
      groupList.Value.Should().Be(groupName);
      groupList.Text.Should().Be(groupName);
      SelectListItem ethnicityList = viewResultModel.ServiceUserEthnicityList.Should()
        .ContainSingle()
        .Subject;
      ethnicityList.Value.Should().Be(displayName);
      ethnicityList.Text.Should().Be(displayName);
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Trace),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == "Ethnicity Updated"),
          null,
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
    }
  }

  public class EmailElectiveCareLinkToServiceUserTests : RmcControllerTests
  {
    private const string HostName = "HostName";
    private const string ElectiveCareServiceUserHubLink = "ServiceUserHubLink";

    private readonly string _electiveCareServiceUserHubLinkTemplateId;
    private readonly string _replyToId;

    public IMapper Mapper { get; set; }

    public EmailElectiveCareLinkToServiceUserTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps([
          "WmsHub.Business",
          "WmsHub.Ui"
        ])
      );
      Mapper = config.CreateMapper();

      Mock<HttpContext> mockHttpContext = new();
      Mock<HttpRequest> mockHttpRequest = new();
      mockHttpContext.Setup(x => x.Request).Returns(mockHttpRequest.Object);
      mockHttpContext.Setup(x => x.User).Returns(GetClaimsPrincipal());
      mockHttpRequest.Setup(x => x.Host).Returns(new HostString(HostName));

      _electiveCareServiceUserHubLinkTemplateId = Guid.NewGuid().ToString();
      _replyToId = Guid.NewGuid().ToString();

      IOptions<WebUiSettings> options = Options.Create(
        new WebUiSettings()
        {
          ElectiveCareServiceUserHubLink = ElectiveCareServiceUserHubLink,
          ElectiveCareServiceUserHubLinkTemplateId = _electiveCareServiceUserHubLinkTemplateId,
          ReplyToId = _replyToId
        });

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: _mapper.Object,
        notificationService: _mockNotificationService.Object,
        options: options,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = mockHttpContext.Object
        }
      };
    }

    [Fact]
    public async Task EmptyReferralIdGuidThrowsException()
    {
      // Arrange.

      // Act.
      Func<Task<IActionResult>> result =
        () => _classToTest.EmailElectiveCareLinkToServiceUser(Guid.Empty);

      // Assert.
      await result.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExceptionReturnsBadRequest()
    {
      // Arrange.
      _mockReferralService.Setup(x => x.GetReferralEntity(It.IsAny<Guid>()))
        .ThrowsAsync(new Exception());

      // Act.
      IActionResult result = await _classToTest.EmailElectiveCareLinkToServiceUser(Guid.NewGuid());

      // Assert.
      result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MissingReferralReturnsBadRequest()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();

      _mockReferralService.Setup(x => x.GetReferralEntity(referralId))
        .ThrowsAsync(new ReferralNotFoundException(referralId));

      // Act.
      IActionResult result = await _classToTest.EmailElectiveCareLinkToServiceUser(referralId);

      // Assert.
      result.Should().BeOfType<BadRequestObjectResult>()
        .Subject.Value.Should().Be($"Unable to find a referral with an id of {referralId}.");
    }

    [Fact]
    public async Task ServiceUserLinkIdProcessAlreadyRunningReturnsConflict()
    {
      // Arrange.
      Business.Models.Provider provider = RandomModelCreator.CreateRandomProvider();

      Business.Entities.Referral referralEntity = new()
      {
        Id = Guid.NewGuid()
      };

      Referral referralModel = new()
      {
        Id = referralEntity.Id
      };

      _mapper.Setup(x => x.Map<Referral>(It.IsAny<Business.Entities.Referral>()))
        .Returns(referralModel);

      _mockReferralService.Setup(x => x.GetReferralEntity(referralEntity.Id))
        .ReturnsAsync(referralEntity);

      _mockReferralService.Setup(x => x.GetServiceUserLinkIdAsync(referralModel))
        .ThrowsAsync(new ProcessAlreadyRunningException());

      // Act.
      IActionResult response =
        await _classToTest.EmailElectiveCareLinkToServiceUser(referralEntity.Id);

      // Assert.
      response.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task SuccessReturnsOk()
    {
      // Arrange.
      string email = "dummy@test.com";
      string givenName = "GivenName";
      string messageId = Guid.NewGuid().ToString();
      Guid referralId = Guid.NewGuid();
      string serviceUserLinkId = "abc123def456";

      string expectedLink = $"{ElectiveCareServiceUserHubLink}?u={serviceUserLinkId}";

      Business.Entities.Referral referralEntity = new()
      {
        Email = email,
        GivenName = givenName,
        Id = referralId
      };

      Referral referralModel = new()
      {
        Email = email,
        GivenName = givenName,
        Id = referralId
      };

      _mapper.Setup(x => x.Map<Referral>(It.IsAny<Business.Entities.Referral>()))
        .Returns(referralModel);

      _mockReferralService.Setup(x => x.GetReferralEntity(referralId))
        .ReturnsAsync(referralEntity);

      _mockReferralService.Setup(x => x.GetServiceUserLinkIdAsync(referralModel))
        .ReturnsAsync(serviceUserLinkId);

      EmailElectiveCareLinkResponse emailElectiveCareLinkResponse = new()
      {
        Id = messageId,
        Status = Actions.DELIVERED
      };

      StringContent emailElectiveCareLinkResponseContent =
        new(JsonSerializer.Serialize(emailElectiveCareLinkResponse));

      HttpResponseMessage httpResponseMessage = new()
      {
        StatusCode = System.Net.HttpStatusCode.OK,
        Content = emailElectiveCareLinkResponseContent
      };

      _mockNotificationService.Setup(x => x.SendMessageAsync(
          It.Is<MessageQueue>(x => 
            x.Personalisation.ContainsValue(expectedLink) &&
            x.Personalisation.ContainsValue(givenName) &&
            x.EmailTo == email)))
        .ReturnsAsync(httpResponseMessage)
        .Verifiable();
      _mockNotificationService.Setup(x => x.GetMessageVerification(messageId))
        .ReturnsAsync(httpResponseMessage);

      // Act.
      IActionResult response = await _classToTest.EmailElectiveCareLinkToServiceUser(referralId);

      // Assert.
      _mockNotificationService.Verify();
      response.Should().BeOfType<OkObjectResult>().Subject.Value.Should().Be(expectedLink);
    }
  }

  public class EmailProviderListToServiceUserTests : RmcControllerTests
  {
    private const string HostName = "HostName";
    private const string ProviderLinkEndpoint = "ProviderDetails";
    private const string ServiceUserHubLink = "ServiceUserHubLink";

    private readonly string _providerByEmailTemplateId;
    private readonly string _replyToId;

    public IMapper Mapper { get; set; }

    public EmailProviderListToServiceUserTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps([
          "WmsHub.Business",
          "WmsHub.Ui"
        ])
      );
      Mapper = config.CreateMapper();

      Mock<HttpContext> mockHttpContext = new();
      Mock<HttpRequest> mockHttpRequest = new();
      mockHttpContext.Setup(x => x.Request).Returns(mockHttpRequest.Object);
      mockHttpContext.Setup(x => x.User).Returns(GetClaimsPrincipal());
      mockHttpRequest.Setup(x => x.Host).Returns(new HostString(HostName));

      _providerByEmailTemplateId = Guid.NewGuid().ToString();
      _replyToId = Guid.NewGuid().ToString();

      IOptions<WebUiSettings> options = Options.Create(
        new WebUiSettings()
        {
          ProviderByEmailTemplateId = _providerByEmailTemplateId,
          ProviderLinkEndpoint = ProviderLinkEndpoint,
          ReplyToId = _replyToId,
          ServiceUserHubLink = ServiceUserHubLink
        });

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: options,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = mockHttpContext.Object
        }
      };
    }

    [Fact]
    public async Task EmptyProvidersReturnsBadRequest()
    {
      // Arrange.
      Referral referral = new()
      {
        Providers = []
      };

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
        .ReturnsAsync(referral);

      // Act.
      IActionResult result = await _classToTest.EmailProviderListToServiceUser(
        "000000000001",
        Guid.NewGuid());

      // Assert.
      result.Should().BeOfType<BadRequestObjectResult>()
        .Subject.Value.Should().Be("Referral has no providers.");
    }

    [Fact]
    public async Task ExceptionReturnsBadRequest()
    {
      // Arrange.
      string ubrn = "000000000001";
      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
        .ThrowsAsync(new Exception());

      // Act.
      IActionResult result = await _classToTest.EmailProviderListToServiceUser(
        ubrn,
        Guid.NewGuid());

      // Assert.
      result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MissingReferralReturnsBadRequest()
    {
      // Arrange.
      Referral referral = null;

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
        .ReturnsAsync(referral);

      // Act.
      IActionResult result = await _classToTest.EmailProviderListToServiceUser(
        "000000000001",
        Guid.NewGuid());

      // Assert.
      result.Should().BeOfType<BadRequestObjectResult>()
        .Subject.Value.Should().Be("Referral not found.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("000000000001")]
    public async Task NullParametersThrowsException(string ubrn)
    {
      // Arrange.

      // Act.
      Func<Task<IActionResult>> result = 
        () => _classToTest.EmailProviderListToServiceUser(ubrn, null);

      // Assert.
      await result.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ServiceUserLinkIdProcessAlreadyRunningReturnsConflict()
    {
      // Arrange.
      string ubrn = "000000000001";

      Business.Models.Provider provider = RandomModelCreator.CreateRandomProvider();

      Referral referral = new()
      {
        Id = Guid.NewGuid(),
        Providers = [provider]
      };

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(referral.Id))
        .ReturnsAsync(referral);

      _mockReferralService.Setup(x => x.GetServiceUserLinkIdAsync(referral))
        .ThrowsAsync(new ProcessAlreadyRunningException());

      // Act.
      IActionResult response = await _classToTest.EmailProviderListToServiceUser(ubrn, referral.Id);

      // Assert.
      response.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task SuccessReturnsOk()
    {
      // Arrange.
      string expectedFilePath = $"https://{HostName}/{ProviderLinkEndpoint}/ProviderProvider_1.pdf";
      string messageId = Guid.NewGuid().ToString();
      string providerName = "Provider & Provider";
      string serviceUserLinkId = "abc123def456";
      string ubrn = "000000000001";

      string expectedProviderLine = $"Provider 1.  {providerName}.  {expectedFilePath}\r\n";

      Business.Models.Provider provider = RandomModelCreator.CreateRandomProvider(
        level1: true,
        name: providerName);

      Referral referral = new()
      {
        Email = "dummy@test.com",
        GivenName = "GivenName",
        Id = Guid.NewGuid(),
        Providers = [provider],
        OfferedCompletionLevel = "1"
      };

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(referral.Id))
        .ReturnsAsync(referral);

      _mockReferralService.Setup(x => x.GetServiceUserLinkIdAsync(referral))
        .ReturnsAsync(serviceUserLinkId);

      EmailProvidersListResponse emailProvidersListResponse = new()
      {
        Id = messageId,
        Status = Actions.DELIVERED
      };

      StringContent emailProvidersListResponseContent = 
        new(JsonSerializer.Serialize(emailProvidersListResponse));

      HttpResponseMessage httpResponseMessage = new()
      {
        StatusCode = System.Net.HttpStatusCode.OK,
        Content = emailProvidersListResponseContent
      };

      _mockNotificationService.Setup(x => x.SendMessageAsync(
          It.Is<MessageQueue>(x => x.Personalisation.ContainsValue(expectedProviderLine))))
        .ReturnsAsync(httpResponseMessage)
        .Verifiable();
      _mockNotificationService.Setup(x => x.GetMessageVerification(messageId))
        .ReturnsAsync(httpResponseMessage);

      // Act.
      IActionResult response = await _classToTest.EmailProviderListToServiceUser(ubrn, referral.Id);

      // Assert.
      _mockNotificationService.Verify();
      response.Should().BeOfType<OkObjectResult>();
    }
  }

  /// <summary>
  /// Tests to be confirmed in a different refactor RMCController story.
  /// </summary>
  public class ForwardProvidersTests : RmcControllerTests
  {
    public IMapper Mapper { get; set; }
    private readonly Mock<NotificationOptions> _options = new();

    public ForwardProvidersTests()
    {
      //_options.Setup(t => t.GetTemplateIdFor(It.IsAny<string>()))
      //  .Returns(Guid.NewGuid());
      //_options.Setup(t => t.GetEmailIdFor(It.IsAny<string>()))
      //  .Returns(Guid.NewGuid());
      _mockTempOptions.Setup(t => t.Value).Returns(_options.Object);
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps(new[] {
                "WmsHub.Business",
                "WmsHub.Ui"
        })
      );
      Mapper = config.CreateMapper();
      Business.Entities.Provider provider =
        RandomEntityCreator.CreateRandomProvider();

      Business.Models.Provider providerModel =
        Mapper.Map<Business.Models.Provider>(provider);

      Business.Entities.Referral referralEntity =
        RandomEntityCreator.CreateRandomReferral(
          id: Guid.NewGuid(),
          providerId: Guid.Empty,
          status: ReferralStatus.RmcCall);

      Referral referral =
        Mapper.Map<Business.Entities.Referral, Referral>(referralEntity);

      referral.Providers = new List<Business.Models.Provider>
      {
          providerModel
      };

      _mockReferralService.Setup(
        t => t.GetReferralWithTriagedProvidersById(It.IsAny<Guid>()))
       .ReturnsAsync(referral);
      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    //[Fact]
    public async Task ForwardProviders_Success()
    {
      // Arrange.
      //_mockNotificationService.Setup(
      // t=>t.SendEmailMessageAsync(It.IsAny<EmailMessage>())).Returns()

      // Act.
      IActionResult result = await _classToTest.EmailProviderListToServiceUser(
        Guid.NewGuid().ToString(),
        Guid.NewGuid()
      );
    }
  }

  public class ProviderDetailsEmailHistoryTests : RmcControllerTests
  {
    public IMapper Mapper { get; set; }

    public ProviderDetailsEmailHistoryTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.Ui"
        })
      );
      Mapper = config.CreateMapper();

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    [Fact]
    public async Task EmailHistoryPresentReturnsOk()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();

      ProviderDetailsEmailHistoryItem[] emailHistory =
        [
          new ProviderDetailsEmailHistoryItem()
          {
            Created = DateTimeOffset.UtcNow,
            Delivered = DateTimeOffset.UtcNow,
            Email = "email@test.com",
            Sending = DateTimeOffset.UtcNow,
            Status = "Delivered"
          }
        ];

      string serializedEmailHistory = JsonSerializer.Serialize(emailHistory);

      HttpContent content = new StringContent(serializedEmailHistory);

      HttpResponseMessage httpResponseMessage = new(System.Net.HttpStatusCode.OK)
      {
        Content = content
      };

      _mockNotificationService.Setup(x => x.GetEmailHistory(referralId.ToString()))
        .ReturnsAsync(httpResponseMessage);

      // Act.
      IActionResult result = await _classToTest.ProviderDetailsEmailHistory(referralId);

      // Assert.
      result.Should().BeOfType<OkObjectResult>().Subject.Value.Should().Be(serializedEmailHistory);
    }

    [Fact]
    public async Task EmptyReferralIdReturnsBadRequest()
    {
      // Arrange.

      // Act.
      IActionResult result = await _classToTest.ProviderDetailsEmailHistory(Guid.Empty);

      // Assert.
      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ExceptionLogsErrorAndReturnsInternalServerError()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();

      string expectedErrorMessage = "Error message.";

      _mockNotificationService.Setup(x => x.GetEmailHistory(referralId.ToString()))
        .ThrowsAsync(new Exception(expectedErrorMessage));

      // Act.
      IActionResult result = await _classToTest.ProviderDetailsEmailHistory(referralId);

      // Assert.
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Error),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedErrorMessage),
          It.IsAny<Exception>(),
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task NotificationProxyExceptionReturnsInternalServerError()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();

      _mockNotificationService.Setup(x => x.GetEmailHistory(referralId.ToString()))
        .ThrowsAsync(new NotificationProxyException());

      // Act.
      IActionResult result = await _classToTest.ProviderDetailsEmailHistory(referralId);

      // Assert.
      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task NotificationProxyNotFoundResponseReturnsNoContent()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();

      ProviderDetailsEmailHistoryItem[] emailHistory = [];

      string serializedEmailHistory = JsonSerializer.Serialize(emailHistory);

      HttpContent content = new StringContent(serializedEmailHistory);

      HttpResponseMessage httpResponseMessage = new(System.Net.HttpStatusCode.NotFound);

      _mockNotificationService.Setup(x => x.GetEmailHistory(referralId.ToString()))
        .ReturnsAsync(httpResponseMessage);

      // Act.
      IActionResult result = await _classToTest.ProviderDetailsEmailHistory(referralId);

      // Assert.
      result.Should().BeOfType<NoContentResult>();
    }
  }

  public class ServiceUserEthnicityGroupMembersTests : RmcControllerTests
  {
    public IMapper Mapper { get; set; }

    public ServiceUserEthnicityGroupMembersTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.Ui"
        })
      );
      Mapper = config.CreateMapper();

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    [Fact]
    public async Task InvalidEthnicityGroupNameReturns400()
    {
      // Arrange.
      string ethnicityGroupName = "invalidGroupName";

      _mockEthnicityService.Setup(x => x.GetEthnicityGroupMembersAsync(ethnicityGroupName))
        .ReturnsAsync([]);

      // Act.
      IActionResult result = await _classToTest.ServiceUserEthnicityGroupMembers(ethnicityGroupName);

      // Assert.
      result.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ValidEthnicityGroupNameReturnsArray()
    {
      // Arrange.
      string ethnicityGroupName = "testGroup";
      string[] expectedOutput = ["ethnicity1", "ethnicity2"];

      Business.Models.Ethnicity[] ethnicities =
        [
          new() { DisplayName = expectedOutput[0] },
          new() { DisplayName = expectedOutput[1] }
        ];

      _mockEthnicityService.Setup(x => x.GetEthnicityGroupMembersAsync(ethnicityGroupName))
        .ReturnsAsync(ethnicities);

      // Act.
      IActionResult result = await _classToTest.ServiceUserEthnicityGroupMembers(ethnicityGroupName);

      // Assert.
      result.Should().BeOfType<OkObjectResult>()
        .Subject.Value.Should().BeEquivalentTo(expectedOutput);
    }
  }

  public class ReferralViewTests : RmcControllerTests
  {
    public ReferralViewTests() : base()
    {
      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: _mapper.Object,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    public static TheoryData<DateTimeOffset?, DateTimeOffset> MaxDateToDelayTheoryData()
    {
      TheoryData<DateTimeOffset?, DateTimeOffset> theoryData = [];
      theoryData.Add(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-5));
      theoryData.Add(null, DateTimeOffset.UtcNow);
      return theoryData;
    }

    [Theory]
    [MemberData(nameof(MaxDateToDelayTheoryData))]
    public async Task ValidReferralReturnsModelWithMaxDateToDelay(
      DateTimeOffset? dateOfFirstContact,
      DateTimeOffset dateOfReferral)
    {
      // Arrange.
      _mockOptions.Object.Value.MaxDaysAfterFirstContactToDelay = 35;
      DateTimeOffset expectedMaxDateToDelay = DateTimeOffset.UtcNow.AddDays(35).Date;
      int expectedMaxDaysToDelay = 34;

      Business.Entities.Referral referralEntity = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid());

      Referral referralModel = new()
      {
        Id = referralEntity.Id
      };

      ReferralListItemModel referralListItemModel = new()
      {
        DateOfReferral = dateOfReferral,
        Id = referralEntity.Id
      };

      _mapper.Setup(x => x.Map<IReferral, ReferralListItemModel>(referralModel))
        .Returns(referralListItemModel);

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(referralEntity.Id))
        .ReturnsAsync(referralModel);
      _mockReferralService.Setup(x => x.GetDateOfFirstContact(referralEntity.Id))
        .ReturnsAsync(dateOfFirstContact);
      _mockReferralService.Setup(x => x.GetReferralAuditForServiceUserAsync(referralEntity.Id))
        .ReturnsAsync([]);

      // Act.
      IActionResult result = await _classToTest.ReferralView(referralEntity.Id);

      // Assert.
      ViewResult view = result.Should().BeOfType<ViewResult>().Subject;
      view.ViewData["Title"].Should().Be("Referral View");
      ReferralListItemModel model = view.Model.Should().BeOfType<ReferralListItemModel>().Subject;
      model.MaxDaysToDelay.Should().Be(expectedMaxDaysToDelay);
      model.MaxDateToDelay.Should().HaveValue().And.Subject.Should().Be(expectedMaxDateToDelay);
    }
  }
  public class UpdateDateOfBirthTests : RmcControllerTests
  {
    public IMapper Mapper { get; set; }

    public UpdateDateOfBirthTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.Ui"
        })
      );
      Mapper = config.CreateMapper();

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    [Fact]
    public async Task ModelStateIsNotValidLogsError()
    {
      // Arrange.
      string errorMessage = "Id is not valid.";
      string expectedErrorMessage = "Model is NOT valid: " + errorMessage;
      _classToTest.ModelState.AddModelError("Id", errorMessage);


      ReferralListItemModel model = new();

      // Act.
      IActionResult result = await _classToTest.UpdateDateOfBirth(model);

      // Assert.
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Error),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedErrorMessage),
          It.IsAny<Exception>(),
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
      result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("ReferralView");
    }

    [Fact]
    public async Task ValidCallUpdatesDateOfBirthAndReturnsModel()
    {
      // Arrange.
      string displayName = "displayName";
      string groupName = "groupName";
      string triageName = "triageName";

      Business.Models.Ethnicity ethnicity = new()
      {
        DisplayName = displayName,
        GroupName = groupName,
        TriageName = triageName
      };

      _mockEthnicityService.Setup(x => x.GetEthnicityGroupNamesAsync())
        .ReturnsAsync([groupName])
        .Verifiable();
      _mockEthnicityService.Setup(x => x.GetEthnicityGroupMembersAsync(groupName))
        .ReturnsAsync([ethnicity])
        .Verifiable();

      DateTimeOffset dateOfBirth = new(1980, 01, 01, 0, 0, 0, new TimeSpan(0));

      Guid providerId = Guid.NewGuid();
      string providerName = "ProviderName";
      IReferral referral = new Referral()
      {
        DateOfBirth = dateOfBirth,
        Ethnicity = triageName,
        Id = Guid.NewGuid(),
        ProviderId = providerId,
        ServiceUserEthnicity = displayName,
        ServiceUserEthnicityGroup = groupName,
      };

      ReferralListItemModel model = new()
      {
        Id = referral.Id,
        DateOfBirth = dateOfBirth,
        SelectedServiceUserEthnicityGroup = groupName
      };

      _mockReferralService.Setup(x => x.UpdateDateOfBirth(model.Id, dateOfBirth))
        .ReturnsAsync(referral)
        .Verifiable();

      _mockReferralService.Setup(x => x.GetRmcRejectedReferralStatusReasonsAsync())
        .ReturnsAsync(It.IsAny<ReferralStatusReason[]>())
        .Verifiable();

      _mockReferralService.Setup(x => x.GetReferralAuditForServiceUserAsync(model.Id))
        .ReturnsAsync([])
        .Verifiable();

      _mockReferralService.Setup(x => x.GetProviderNameAsync(providerId))
        .ReturnsAsync(providerName)
        .Verifiable();

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(model.Id))
        .ReturnsAsync(referral)
        .Verifiable();

      // Act.
      IActionResult result = await _classToTest.UpdateDateOfBirth(model);

      // Assert.
      _mockReferralService.Verify();
      _mockEthnicityService.Verify();
      ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
      viewResult.ViewName.Should().Be("ReferralView");
      ReferralListItemModel viewResultModel =
        viewResult.Model.Should().BeOfType<ReferralListItemModel>().Subject;
      viewResultModel.DateOfBirth.Should().Be(dateOfBirth);
      SelectListItem groupList = viewResultModel.ServiceUserEthnicityGroupList.Should()
        .ContainSingle()
        .Subject;
      groupList.Value.Should().Be(groupName);
      groupList.Text.Should().Be(groupName);
      SelectListItem ethnicityList = viewResultModel.ServiceUserEthnicityList.Should()
        .ContainSingle()
        .Subject;
      ethnicityList.Value.Should().Be(displayName);
      ethnicityList.Text.Should().Be(displayName);
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Trace),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == "Date Of Birth Updated"),
          null,
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
    }
  }

  public class UpdateMobileNumberTests : RmcControllerTests
  {
    public IMapper Mapper { get; set; }

    public UpdateMobileNumberTests()
    {
      MapperConfiguration config = new(cfg =>
        cfg.AddMaps(new[] {
          "WmsHub.Business",
          "WmsHub.Ui"
        })
      );
      Mapper = config.CreateMapper();

      _classToTest = new RmcController(
        logger: _mockLogger.Object,
        ethnicityService: _mockEthnicityService.Object,
        mapper: Mapper,
        notificationService: _mockNotificationService.Object,
        options: _mockOptions.Object,
        referralService: _mockReferralService.Object,
        providerService: _mockProviderService.Object)
      {
        ControllerContext = new ControllerContext
        {
          HttpContext = new DefaultHttpContext
          {
            User = GetClaimsPrincipal()
          }
        }
      };
    }

    [Fact]
    public async Task ModelStateIsNotValidLogsError()
    {
      // Arrange.
      string errorMessage = "Id is not valid.";
      _classToTest.ModelState.AddModelError("Id", errorMessage);


      ReferralListItemModel model = new();

      // Act.
      IActionResult result = await _classToTest.UpdateMobileNumber(model);

      // Assert.
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Error),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == errorMessage),
          It.IsAny<Exception>(),
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
      result.Should().BeOfType<ViewResult>().Subject.ViewName.Should().Be("ReferralView");
    }

    [Fact]
    public async Task ValidCallUpdatesMobileNumberAndReturnsModel()
    {
      // Arrange.
      string displayName = "displayName";
      string groupName = "groupName";
      string triageName = "triageName";

      Business.Models.Ethnicity ethnicity = new()
      {
        DisplayName = displayName,
        GroupName = groupName,
        TriageName = triageName
      };

      _mockEthnicityService.Setup(x => x.GetEthnicityGroupNamesAsync())
        .ReturnsAsync([groupName])
        .Verifiable();
      _mockEthnicityService.Setup(x => x.GetEthnicityGroupMembersAsync(groupName))
        .ReturnsAsync([ethnicity])
        .Verifiable();

      string mobileNumber = "+447123456789";

      Guid providerId = Guid.NewGuid();
      string providerName = "ProviderName";
      IReferral referral = new Referral()
      {
        Ethnicity = triageName,
        Id = Guid.NewGuid(),
        Mobile = mobileNumber,
        ProviderId = providerId,
        ServiceUserEthnicity = displayName,
        ServiceUserEthnicityGroup = groupName,
      };

      ReferralListItemModel model = new()
      {
        Id = referral.Id,
        Mobile = mobileNumber,
        SelectedServiceUserEthnicityGroup = groupName
      };

      _mockReferralService.Setup(x => x.UpdateMobile(model.Id, mobileNumber))
        .ReturnsAsync(referral)
        .Verifiable();

      _mockReferralService.Setup(x => x.GetRmcRejectedReferralStatusReasonsAsync())
        .ReturnsAsync(It.IsAny<ReferralStatusReason[]>())
        .Verifiable();

      _mockReferralService.Setup(x => x.GetReferralAuditForServiceUserAsync(model.Id))
        .ReturnsAsync([])
        .Verifiable();

      _mockReferralService.Setup(x => x.GetProviderNameAsync(providerId))
        .ReturnsAsync(providerName)
        .Verifiable();

      _mockReferralService.Setup(x => x.GetReferralWithTriagedProvidersById(model.Id))
        .ReturnsAsync(referral)
        .Verifiable();

      // Act.
      IActionResult result = await _classToTest.UpdateMobileNumber(model);

      // Assert.
      _mockReferralService.Verify();
      _mockEthnicityService.Verify();
      ViewResult viewResult = result.Should().BeOfType<ViewResult>().Subject;
      viewResult.ViewName.Should().Be("ReferralView");
      ReferralListItemModel viewResultModel =
        viewResult.Model.Should().BeOfType<ReferralListItemModel>().Subject;
      viewResultModel.Mobile.Should().Be(mobileNumber);
      SelectListItem groupList = viewResultModel.ServiceUserEthnicityGroupList.Should()
        .ContainSingle()
        .Subject;
      groupList.Value.Should().Be(groupName);
      groupList.Text.Should().Be(groupName);
      SelectListItem ethnicityList = viewResultModel.ServiceUserEthnicityList.Should()
        .ContainSingle()
        .Subject;
      ethnicityList.Value.Should().Be(displayName);
      ethnicityList.Text.Should().Be(displayName);
      _mockLogger.Verify(
        x => x.Log(
          It.Is<LogLevel>(l => l == LogLevel.Trace),
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString() == "Mobile Number Updated"),
          null,
          It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
    }
  }
}