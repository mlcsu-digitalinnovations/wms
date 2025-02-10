using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Entities.Interfaces;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Tests.Helper;
using WmsHub.TextMessage.Api.Controllers;
using WmsHub.TextMessage.Api.Tests.TestSetup;
using Xunit;

namespace WmsHub.TextMessage.Api.Tests;

public class SmsControllerTest : TestSetup.TestSetup
{
  private readonly Mock<DatabaseContext> _mockContext = new();
  private readonly Mock<ILinkIdService> _mockLinkIdService = new();
  private readonly Mock<ISmsMessage> _mockRequest = new();
  private readonly Mock<TextService> _mockService;

  private readonly Mock<IMapper> _mockMapper = new();
  private readonly Mock<TextNotificationHelper> _mockHelper;
  private readonly TextMessageRequest _smsRequest = new();
  private SmsController _classToTest;

  private readonly Mock<TextOptions> _mockTextOptions = new();

  public SmsControllerTest()
  {
    _mockTextOptions.Object.NotifyLink = "https://test_link";
    _mockTextOptions.Object.GeneralReferralNotifyLink = "https://test_link_general";

    _mockRequest.Setup(x => x.ClientReference).Returns("test_reference");
    _mockRequest.Setup(x => x.TemplateId).Returns("Test Template");
    _mockHelper = new(Options.Create(_mockTextOptions.Object));
    _mockService = new(
      Options.Create(_mockTextOptions.Object),
      _mockHelper.Object,
      _mockContext.Object,
      new DateTimeProvider(),
      _mockLinkIdService.Object);
  }

  public class PostTests : SmsControllerTest
  {

    [Fact]
    public async Task AddMessageReturnsOk()
    {
      //Arrange
      _mockService.Setup(x => x.AddNewMessageAsync(
        It.IsAny<TextMessageRequest>()))
          .Returns(Task.FromResult(true));

      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);


      _smsRequest.ReferralId = Guid.NewGuid();
      _smsRequest.MobileNumber = "0770000000";

      //Act
      var result = await _classToTest.Post(_smsRequest);

      //Assert
      Assert.NotNull(result);
      var outputResult = Assert.IsType<OkObjectResult>(result);

      Assert.True(outputResult.StatusCode == 200);

    }

    public class BadRequestsResponses : PostTests
    {
      public BadRequestsResponses()
      {
        _mockService.Setup(x => x.AddNewMessageAsync(
          It.IsAny<TextMessageRequest>()))
          .Returns(Task.FromResult(false));

        _classToTest = new SmsController(
          _mockService.Object,
          Options.Create(_mockTextOptions.Object),
          _mockMapper.Object);
      }

      [Fact]
      public async Task AddMessageReturnsBadRequest()
      {
        //Arrange
        _smsRequest.ReferralId = Guid.NewGuid();
        _smsRequest.MobileNumber = "0770000000";

        //Act
        var response = await _classToTest.Post(_smsRequest);

        //Assert
        Assert.NotNull(response);
        var outputResult = Assert.IsType<BadRequestObjectResult>(response);

        Assert.True(outputResult.StatusCode ==
          (int)HttpStatusCode.BadRequest);
      }

      [Fact]
      public async Task AddMessageMissingNumberReturnsBadRequest()
      {
        //Arrange
        string expected = "Model is not valid:The MobileNumber field is " +
          "required.";
        int expectedStatus = (int)HttpStatusCode.BadRequest;
        _smsRequest.ReferralId = Guid.NewGuid();

        //Act
        var response = await _classToTest.Post(_smsRequest);

        //Assert
        Assert.NotNull(response);
        var outputResult = Assert.IsType<BadRequestObjectResult>(response);
        var returnError = JObject.FromObject(outputResult.Value);
        Assert.True(outputResult.StatusCode == expectedStatus);
        Assert.Equal(expected, returnError["message"].ToString());
      }

      [Fact]
      public async Task AddMessageMissingReferralIdReturnsBadRequest()
      {
        //Arrange
        string expected = "Model is not valid:The ReferralId field must " +
          "not be empty";
        int expectedStatus = (int)HttpStatusCode.BadRequest;
        _smsRequest.MobileNumber = "0770000000";

        //Act
        var response = await _classToTest.Post(_smsRequest);

        //Assert
        Assert.NotNull(response);
        var outputResult = Assert.IsType<BadRequestObjectResult>(response);
        var returnError = JObject.FromObject(outputResult.Value);
        Assert.True(outputResult.StatusCode == expectedStatus);
        Assert.Equal(expected, returnError["message"].ToString());
      }
    }
  }

  public class GetTests : SmsControllerTest
  {

    [Fact]
    public async Task Valid()
    {
      //Arrange
      int expectedCount = 1;
      _mockService.Object.User = GetClaimsPrincipal();
      _mockService.Setup(t => t.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ReturnsAsync(1);

      List<SmsMessage> messages = TestGenerator.GenerateSmsMessages();
      _mockService.Setup(t => t.GetMessagesToSendAsync(It.IsAny<int>()))
       .ReturnsAsync(messages);

      _mockTextOptions.Setup(t =>
          t.ValidateNumbersAgainstWhiteList(It.IsAny<List<string>>()))
       .Verifiable();

      Mock<SmsNotificationResponse> _mockSmsResponse =
        new Mock<SmsNotificationResponse>();

      _mockSmsResponse.Object.id = Guid.NewGuid().ToString();
      _mockSmsResponse.Object.reference = Guid.NewGuid().ToString();

      _mockService.Setup(t => t.SendSmsMessageAsync(It.IsAny<ISmsMessage>()))
       .ReturnsAsync(_mockSmsResponse.Object);

      _mockService
       .Setup(t => t.UpdateMessageRequestAsync(
         It.IsAny<ISmsMessage>(), "SENT"))
       .Verifiable();

      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);

      //Act
      var result = await _classToTest.Get();
      //Assert
      result.Should().NotBeNull();
      result.Should().BeOfType<OkObjectResult>();
      OkObjectResult response = result as OkObjectResult;
      response.Value.Should().Be(expectedCount);
      //Cleanup
      _classToTest = null;
    }

    [Fact]
    public async Task Valid_GeneralReferral()
    {
      //Arrange
      int expectedCount = 1;
      _mockService.Object.User = GetClaimsPrincipal();
      _mockService.Setup(t => t.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ReturnsAsync(1);

      List<SmsMessage> messages = TestGenerator.GenerateSmsMessages();
      foreach (var message in messages)
      {
        message.ReferralSource = ReferralSource.GeneralReferral.ToString();
      }
      _mockService.Setup(t => t.GetMessagesToSendAsync(It.IsAny<int>()))
        .ReturnsAsync(messages);

      _mockTextOptions.Setup(t =>
          t.ValidateNumbersAgainstWhiteList(It.IsAny<List<string>>()))
        .Verifiable();

      Mock<SmsNotificationResponse> _mockSmsResponse =
        new Mock<SmsNotificationResponse>();

      _mockSmsResponse.Object.id = Guid.NewGuid().ToString();
      _mockSmsResponse.Object.reference = Guid.NewGuid().ToString();

      _mockService.Setup(t => t.SendSmsMessageAsync(It.IsAny<ISmsMessage>()))
        .ReturnsAsync(_mockSmsResponse.Object);

      _mockService
        .Setup(t => t.UpdateMessageRequestAsync(
          It.IsAny<ISmsMessage>(), "SENT"))
        .Verifiable();

      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);
      //Act
      var result = await _classToTest.Get();
      //Assert
      result.Should().NotBeNull();
      result.Should().BeOfType<OkObjectResult>();
      OkObjectResult response = result as OkObjectResult;
      response.Value.Should().Be(expectedCount);
      //Cleanup
      _classToTest = null;
    }

    [Fact]
    public async Task ShouldBeValid()
    {
      //Arrange
      string expected = "1 messages sent out of 1 requested";
      _mockService.Object.User = GetClaimsPrincipal();
      _mockService.Setup(t => t.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ReturnsAsync(1);
      List<SmsMessage> messages = TestGenerator.GenerateSmsMessages();
      _mockService.Setup(t => t.GetMessageByReferralIdToSendAsync(It
       .IsAny<Guid>())).ReturnsAsync(messages.FirstOrDefault());
      _mockTextOptions.Setup(t =>
       t.ValidateNumbersAgainstWhiteList(It.IsAny<List<string>>()))
       .Verifiable();

      Mock<SmsNotificationResponse> _mockSmsResponse =
       new Mock<SmsNotificationResponse>();

      _mockSmsResponse.Object.id = Guid.NewGuid().ToString();
      _mockSmsResponse.Object.reference = Guid.NewGuid().ToString();
      _mockService.Setup(t => t.SendSmsMessageAsync(It
       .IsAny<ISmsMessage>())).ReturnsAsync(_mockSmsResponse.Object);
      _mockService
       .Setup(t => t.UpdateMessageRequestAsync(
         It.IsAny<ISmsMessage>(), "SENT"))
       .Verifiable();

      _classToTest = new SmsController(
        _mockService.Object,
       Options.Create(_mockTextOptions.Object),
       _mockMapper.Object);

      //Act
      var result = await _classToTest.Get(It.IsAny<Guid>());

      //Assert
      result.Should().NotBeNull();
      result.Should().BeOfType<OkObjectResult>();
      OkObjectResult response = result as OkObjectResult;
      response.Value.Should().Be(expected);

      //Cleanup
      _classToTest = null;
    }

    [Fact]
    public async Task Get_NumberWhiteListException_TrueNoNumbers()
    {
      //Arrange
      TextOptions options = TestConfiguration.CreateTextOptions().Value;
      options.IsNumberWhiteListEnabled = true;
      options.NumberWhiteList = new List<string>();

      var classToTest = new SmsController(
        _mockService.Object,
        Options.Create(options),
        _mockMapper.Object);

      //Act
      var result = await classToTest.Get();

      //Assert
      result.Should().NotBeNull();

      var outputResult = Assert.IsType<ObjectResult>(result);
      outputResult.StatusCode.Should()
        .Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Get_NumberWhiteListException_FalseWithNumbers()
    {
      //Arrange
      TextOptions options = TestConfiguration.CreateTextOptions().Value;
      options.IsNumberWhiteListEnabled = false;
      options.NumberWhiteList = new List<string> { "+447000000000" };

      var classToTest = new SmsController(
        _mockService.Object,
        Options.Create(options),
        _mockMapper.Object);

      int expected = StatusCodes.Status500InternalServerError;

      //Act
      var result = await classToTest.Get();

      //Assert
      result.Should().NotBeNull();
      var outputResult = Assert.IsType<ObjectResult>(result);
      Assert.Equal(expected, outputResult.StatusCode);
    }

    [Fact]
    public async Task Get_NumberWhiteListException_TrueNumbersNotInList()
    {
      // Arrange.
      TextOptions options = TestConfiguration.CreateTextOptions().Value;
      options.IsNumberWhiteListEnabled = true;
      options.NumberWhiteList = new List<string> { "+447000000000" };

      Mock<TextService> mockService = new(
        Options.Create(options),
        _mockHelper.Object,
        _mockContext.Object,
        new DateTimeProvider(),
        _mockLinkIdService.Object);

      mockService
        .Setup(x => x.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ReturnsAsync(1);

      mockService
        .Setup(x => x.GetMessagesToSendAsync(It.IsAny<int>()))
        .ReturnsAsync(new List<SmsMessage> { new() { MobileNumber = "+447000000001" } });

      SmsController classToTest = new(
        mockService.Object,
        Options.Create(options),
        _mockMapper.Object);

      int expected = StatusCodes.Status500InternalServerError;

      // Act.
      IActionResult result = await classToTest.Get();

      // Assert.
      result.Should().NotBeNull().And.BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task NoRequestsFound()
    {
      //Arrange
      IEnumerable<ISmsMessage> requests = new List<ISmsMessage>();
      _mockService
        .Setup(x => x.GetMessagesToSendAsync(It.IsAny<int>()))
        .ReturnsAsync(requests);

      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);

      //Act
      var response = await _classToTest.Get();

      //Assert
      response.Should().NotBeNull();
      response.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task OneRequestsFoundButValidationException()
    {
      //Arrange
      var expected = "There was a problem sending the message." +
        " Check logs for more details";
      Mock<ISmsMessage> _mockMessage = new Mock<ISmsMessage>();
      IEnumerable<ISmsMessage> requests = new List<ISmsMessage>()
    {_mockMessage.Object };

      _mockService
        .Setup(x => x.GetMessagesToSendAsync(It.IsAny<int>()))
        .Returns(Task.FromResult(requests));
      _mockService
        .Setup(x => x.SendSmsMessageAsync(It.IsAny<ISmsMessage>()))
        .Throws(new ValidationException("test"));

      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);

      //Act
      var response = await _classToTest.Get();

      //Assert
      Assert.NotNull(response);
      var outputResult = Assert.IsType<ObjectResult>(response);
      ProblemDetails result = (ProblemDetails)outputResult.Value;
      Assert.Equal(expected, result.Detail);
      Assert.True(result.Status == 500);
    }

    [Fact]
    public async Task NoReferralsFound_Throw_404_StatusCode()
    {

      //Arrange            
      Mock<ISmsMessage> _mockMessage = new Mock<ISmsMessage>();
      IEnumerable<ISmsMessage> requests = new List<ISmsMessage>()
      {_mockMessage.Object };
      _mockService.Setup(x => x.GetMessageByReferralIdToSendAsync(It
        .IsAny<Guid>()))
        .ThrowsAsync(new BadHttpRequestException("Not Found"));
      _mockService.Setup(x => x.SendSmsMessageAsync(It
        .IsAny<ISmsMessage>()))
        .Throws(new ValidationException("test"));
      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);

      //Act
      var response = await _classToTest.Get(It.IsAny<Guid>());

      //Assert
      Assert.NotNull(response);
      var outputResult = Assert.IsType<ObjectResult>(response);
      Assert.True(outputResult.StatusCode == 404);
    }

    [Fact]
    public async Task InvalidNumbers_Throw_500_StatusCode()
    {
      //Arrange            
      Mock<ISmsMessage> _mockMessage = new Mock<ISmsMessage>();
      IEnumerable<ISmsMessage> requests = new List<ISmsMessage>()
      {_mockMessage.Object };
      _mockService.Object.User = GetClaimsPrincipal();
      _mockService
        .Setup(t => t.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ReturnsAsync(1);
      _mockService
        .Setup(t => t.GetMessageByReferralIdToSendAsync(It.IsAny<Guid>()))
        .ThrowsAsync(new ReferralInvalidStatusException());
      _mockService
        .Setup(x => x.SendSmsMessageAsync(It.IsAny<ISmsMessage>()))
        .Throws(new ValidationException("test"));
      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);

      //Act
      var response = await _classToTest.Get(It.IsAny<Guid>());

      //Assert
      Assert.NotNull(response);
      var outputResult = Assert.IsType<ObjectResult>(response);
      Assert.True(outputResult.StatusCode == 500);
    }

    [Fact]
    public async Task InvalidMessages_Throw_409_StatusCode()
    {

      //Arrange            
      Mock<ISmsMessage> _mockMessage = new Mock<ISmsMessage>();
      IEnumerable<ISmsMessage> requests = new List<ISmsMessage>()
    {_mockMessage.Object };
      _mockService.Object.User = GetClaimsPrincipal();
      _mockService
        .Setup(t => t.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ReturnsAsync(1);
      List<SmsMessage> messages = TestGenerator.GenerateSmsMessages();
      messages.FirstOrDefault().MobileNumber = "+12345";
      _mockService.Setup(t => t.GetMessageByReferralIdToSendAsync(It
        .IsAny<Guid>()))
        .ThrowsAsync(new InvalidOperationException());

      Mock<SmsNotificationResponse> _mockSmsResponse =
        new Mock<SmsNotificationResponse>();
      _mockSmsResponse.Object.id = Guid.NewGuid().ToString();
      _mockSmsResponse.Object.reference = Guid.NewGuid().ToString();
      _mockService.Setup(t => t.SendSmsMessageAsync(It
        .IsAny<ISmsMessage>())).ReturnsAsync(_mockSmsResponse.Object);
      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);

      //Act
      var response = await _classToTest.Get(It.IsAny<Guid>());

      //Assert
      Assert.NotNull(response);
      var outputResult = Assert.IsType<ObjectResult>(response);
      Assert.True(outputResult.StatusCode == 409);
    }
  }

  public class GetCheckSendTests : SmsControllerTest
  {
    public GetCheckSendTests() : base()
    {
      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);
    }

    [Fact]
    public async Task SuccessfulCheck_Returns200()
    {
      // Arrange.
      IEnumerable<ISmsMessage> checkedMessages = TestGenerator.GenerateSmsMessages();

      _mockService.Setup(s => s.GetMessagesToSendAsync(It.IsAny<int?>()))
        .ReturnsAsync(checkedMessages)
        .Verifiable();

      // Act.
      IActionResult response = await _classToTest.GetCheckSend();

      // Assert.
      _mockService.Verify();
      response.Should().BeOfType<OkObjectResult>()
        .Subject.Value.Should().Be(checkedMessages.Count());
    }

    [Fact]
    public async Task Exception_Returns500()
    {
      // Arrange.
      string exceptionMessage = "There was a problem checking the text messages. Check logs for "
        + "more details";
      _mockService.Setup(s => s.GetMessagesToSendAsync(It.IsAny<int?>()))
        .ThrowsAsync(new Exception());

      // Act.
      IActionResult response = await _classToTest.GetCheckSend();

      // Assert.
      response.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

      response.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }
  }

  public class GetPrepareTests : SmsControllerTest
  {
    public GetPrepareTests() : base()
    {
      _classToTest = new SmsController(
        _mockService.Object,
        Options.Create(_mockTextOptions.Object),
        _mockMapper.Object);
    }

    [Fact]
    public async Task SuccessfulPreparation_Returns200()
    {
      // Arrange.
      int numberCreated = 5;

      _mockService.Setup(s => s.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ReturnsAsync(numberCreated)
        .Verifiable();

      // Act.
      IActionResult response = await _classToTest.GetPrepare();

      // Assert.
      _mockService.Verify();
      response.Should().BeOfType<OkObjectResult>()
        .Subject.Value.Should().Be(numberCreated);
    }

    [Fact]
    public async Task ProcessAlreadyRunning_Returns409()
    {
      // Arrange.
      string exceptionMessage = "Exception message";
      _mockService.Setup(s => s.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ThrowsAsync(new ProcessAlreadyRunningException(exceptionMessage));

      // Act.
      IActionResult response = await _classToTest.GetPrepare();

      // Assert.
      response.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status409Conflict);

      response.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task Exception_Returns500()
    {
      // Arrange.
      string exceptionMessage = "There was a problem preparing the text messages. " +
        "Check logs for more details";
      _mockService.Setup(s => s.PrepareMessagesToSend(It.IsAny<Guid>()))
        .ThrowsAsync(new Exception());

      // Act.
      IActionResult response = await _classToTest.GetPrepare();

      // Assert.
      response.Should().BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

      response.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(exceptionMessage);
    }
  }
}

