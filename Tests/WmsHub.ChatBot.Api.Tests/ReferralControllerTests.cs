
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Business.Services;
using WmsHub.ChatBot.Api.Clients;
using WmsHub.ChatBot.Api.Controllers;
using WmsHub.ChatBot.Api.Models;
using WmsHub.Common.SignalR;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ChatBot.Api.Tests
{
  public class ReferralControllerTests
  {
    private readonly Mock<DatabaseContext> _mockContext =
      new Mock<DatabaseContext>();
    private readonly Mock<ClaimsPrincipal> _mockUser =
      new Mock<ClaimsPrincipal>();
    private readonly Mock<ArcusOptions> _mockSettings =
      new Mock<ArcusOptions>();
    private readonly Mock<IOptions<ArcusOptions>> _mockOptions =
      new Mock<IOptions<ArcusOptions>>();
    private readonly Mock<IArcusClientHelper> _mockHelper =
      new Mock<IArcusClientHelper>();
    private readonly Mock<UpdateReferralWithCallRequest> _mockUpdateRequest =
      new Mock<UpdateReferralWithCallRequest>(
        Guid.NewGuid(),
        "test",
        "+447395700001",
        (DateTimeOffset)DateTime.UtcNow);
    private readonly Mock<GetReferralCallListRequest> _mockCallListRequest =
      new Mock<GetReferralCallListRequest>();

    private Mock<UpdateReferralWithCallResponse> _mockUpdateResponse;

    private readonly Mock<GetReferralCallListResponse> _mockCallListResponse;

    private readonly Mock<ChatBotService> _mockChatBotService;

    private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

    private readonly SerilogLoggerMock _loggerMock = new SerilogLoggerMock();

    private ReferralCallController _classToTest;

    private readonly Mock<IHubClients> _mockClients =
      new Mock<IHubClients>();

    public ReferralControllerTests()
    {
      Log.Logger = _loggerMock;

      var response = new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.Created,
        Content = new StringContent("[{'id':1,'value':'1'}]")
      };

      _mockSettings.Object.ReturnLimit = 600;
      _mockOptions.Setup(x => x.Value).Returns(_mockSettings.Object);

      _mockHelper.Setup(x => x.BatchPost(It.IsAny<IArcusCall>()))
        .Returns(Task.FromResult(response));

      _mockUpdateResponse =
         new Mock<UpdateReferralWithCallResponse>(_mockUpdateRequest.Object);

      _mockCallListResponse = new Mock<GetReferralCallListResponse>(
        _mockCallListRequest.Object, _mockSettings.Object);

      _mockCallListResponse.Setup(x => x.Arcus.NumberOfCallsToMake).Returns(2);

      _mockCallListResponse.Setup(x => x.Arcus.Callees)
        .Returns(new List<ICallee>());

      _mockMapper.Setup(x => x.Map<UpdateReferralWithCallRequest>
        (It.IsAny<ReferralCall>())).Returns(_mockUpdateRequest.Object);

      _mockMapper.Setup(x => x.Map<GetReferralCallListRequest>
        (It.IsAny<ReferralCallStart>())).Returns(_mockCallListRequest.Object);

      Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
      _mockClients.Setup(clients => clients.All)
        .Returns(mockClientProxy.Object);

      var hubContext = new Mock<IHubContext<SignalRHub>>();
      hubContext.Setup(x => x.Clients).Returns(() => _mockClients.Object);

      _mockChatBotService = new Mock<ChatBotService>(
        _mockContext.Object,
        _mockMapper.Object,
        _mockOptions.Object,
        null);

      _mockChatBotService.Setup(x => x
        .UpdateReferralWithCall(It.IsAny<UpdateReferralWithCallRequest>()))
        .Returns(Task.FromResult(_mockUpdateResponse.Object));

      _mockChatBotService.Setup(x => x
        .PrepareCallsAsync())
        .Returns(Task.FromResult(new PrepareCallsForTodayResponse()
        { CallsPrepared = 1 }));

      _mockChatBotService.Setup(x => x
        .GetReferralCallList(It.IsAny<GetReferralCallListRequest>()))
        .Returns(Task.FromResult(_mockCallListResponse.Object));

      _mockMapper.Setup(x => x
        .Map<UpdateReferralWithCallRequest>(It.IsAny<ReferralCall>()))
        .Returns(_mockUpdateRequest.Object);

      _classToTest = new ReferralCallController(
        _mockHelper.Object,
        _mockChatBotService.Object,
        _mockOptions.Object,
        _mockMapper.Object);

    }

    public class HttpPostCalls : ReferralControllerTests
    {
      private readonly dynamic _call = new JObject();

      [Fact]
      public async Task JsonToReferalCallRequiredIdMissingInvalid()
      {
        //Arrange
        var expected = "Model is not valid:The Id field is required.";
        _call.Outcome = "Test";
        _call.Number = "+447395700001";
        _call.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_call);

        //Act
        ReferralCall referralCall = JsonConvert
          .DeserializeObject<ReferralCall>(json);
        new Mock<UpdateReferralWithCallResponse>(_mockUpdateRequest.Object);

        _mockUpdateResponse.Setup(x => x.ResponseStatus)
          .Returns(Business.Enums.StatusType.StatusIsUnknown);
        _mockUpdateResponse.Setup(x => x.GetErrorMessage()).Returns(expected);

        _mockChatBotService.Setup(x => x.UpdateReferralWithCall(
          It.IsAny<UpdateReferralWithCallRequest>()))
          .Returns(Task.FromResult(_mockUpdateResponse.Object));

        _classToTest = new ReferralCallController(
        _mockHelper.Object,
        _mockChatBotService.Object,
        _mockOptions.Object,
        _mockMapper.Object);
        //Act
        IActionResult result = await _classToTest.Post(referralCall);

        //Assert
        result.Should().NotBeNull();
        ObjectResult outputResult = 
          Assert.IsType<BadRequestObjectResult>(result);

        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
      }

      [Fact]
      public async Task JsonToReferalCallRequiredPhoneInvalid()
      {
        //Arrange
        var expected = "Model is not valid:The field Number must match the " +
          "regular expression '^\\+[0-9]+$'.";
        _call.Id = Guid.NewGuid();
        _call.Outcome = "Test";
        _call.Number = "07395700001";
        _call.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_call);

        ReferralCall referralCall =
          JsonConvert.DeserializeObject<ReferralCall>(json);

        _mockUpdateResponse =
          new Mock<UpdateReferralWithCallResponse>(_mockUpdateRequest.Object);

        _mockUpdateResponse.Setup(x => x.ResponseStatus)
          .Returns(Business.Enums.StatusType.TelephoneNumberMismatch);
        _mockUpdateResponse.Setup(x => x.GetErrorMessage()).Returns(expected);

        _mockChatBotService.Setup(x => x.UpdateReferralWithCall(
          It.IsAny<UpdateReferralWithCallRequest>()))
          .Returns(Task.FromResult(_mockUpdateResponse.Object));

        _classToTest = new ReferralCallController(
          _mockHelper.Object,
          _mockChatBotService.Object,
          _mockOptions.Object,
          _mockMapper.Object);

        //Act
        var result = await _classToTest.Post(referralCall);

        //Assert
        result.Should().NotBeNull();
        ObjectResult outputResult = Assert
          .IsType<BadRequestObjectResult>(result);
        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
      }

      [Fact]
      public async Task JsonToReferalCallRequiredOutcomeMissingInvalid()
      {
        //Arrange
        var expected = "Model is not valid:The Outcome field is required.";
        _call.Id = Guid.NewGuid();
        _call.Number = "+447395700001";
        _call.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_call);

        //Act
        ReferralCall referralCall = JsonConvert
          .DeserializeObject<ReferralCall>(json);
        _mockUpdateResponse =
         new Mock<UpdateReferralWithCallResponse>(_mockUpdateRequest.Object);

        _mockUpdateResponse.Setup(x => x.ResponseStatus)
          .Returns(Business.Enums.StatusType.OutcomeIsUnknown);
        _mockUpdateResponse.Setup(x => x.GetErrorMessage()).Returns(expected);

        _mockChatBotService.Setup(x => x.UpdateReferralWithCall(
          It.IsAny<UpdateReferralWithCallRequest>()))
          .Returns(Task.FromResult(_mockUpdateResponse.Object));

        _classToTest = new ReferralCallController(
        _mockHelper.Object,
        _mockChatBotService.Object,
        _mockOptions.Object,
        _mockMapper.Object);
        //Act
        var result = await _classToTest.Post(referralCall);

        //Assert
        result.Should().NotBeNull();
        Assert.NotNull(result);
        ObjectResult outputResult = Assert
          .IsType<BadRequestObjectResult>(result);

        outputResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
      }

      [Fact]
      public async Task JsonToReferalCallValidReturnOk()
      {
        //Arrange
        _call.Id = Guid.NewGuid();
        _call.Outcome =
          Business.Enums.ChatBotCallOutcome.CallerReached.ToString();
        _call.Number = "+447395700001";
        _call.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_call);

        //Act
        ReferralCall referralCall = JsonConvert
          .DeserializeObject<ReferralCall>(json);
        var result = await _classToTest.Post(referralCall);

        //Assert
        result.Should().NotBeNull();
        OkResult outputResult =
          Assert.IsType<OkResult>(result);

        outputResult.StatusCode.Should().Be(200);
      }
    }

    public class HttpSetupCalls : ReferralControllerTests
    {
      [Fact]
      public async Task Get_NumberWhiteListException_TrueNoNumbers()
      {
        //Arrange
        ArcusOptions options = TestConfiguration.CreateArcusOptions().Value;
        options.IsNumberWhiteListEnabled = true;
        options.NumberWhiteList = new List<string>();
        options.ReturnLimit = 600;

        var classToTest = new ReferralCallController(
          _mockHelper.Object,
          _mockChatBotService.Object,
          Options.Create(options),
          _mockMapper.Object);

        int expected = StatusCodes.Status500InternalServerError;

        //Act
        var result = await classToTest.Get();

        //Assert
        result.Should().NotBeNull();
        var outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Get_NumberWhiteListException_FalseWithNumbers()
      {
        //Arrange
        ArcusOptions options = TestConfiguration.CreateArcusOptions().Value;
        options.IsNumberWhiteListEnabled = false;
        options.NumberWhiteList = new List<string> { "+447000000000" };
        options.ReturnLimit = 600;
        var classToTest = new ReferralCallController(
          _mockHelper.Object,
          _mockChatBotService.Object,
          Options.Create(options),
          _mockMapper.Object);

        int expected = StatusCodes.Status500InternalServerError;

        //Act
        var result = await classToTest.Get();

        //Assert
        result.Should().NotBeNull();
        var outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task Get_NumberWhiteListException_TrueNumbersNotInList()
      {
        //Arrange
        ArcusOptions options = TestConfiguration.CreateArcusOptions().Value;
        options.IsNumberWhiteListEnabled = true;
        options.NumberWhiteList = new List<string> { "+447000000000" };
        options.ReturnLimit = 600;
        var mockService = new Mock<ChatBotService>(
          _mockContext.Object,
          _mockMapper.Object,
          _mockOptions.Object,
          null);

        mockService.Setup(x => x.GetReferralCallList(
          It.IsAny<GetReferralCallListRequest>()))
            .Returns(Task.FromResult(new GetReferralCallListResponse()
            {
              Arcus = new ArcusCall
              {
                Callees = new List<Callee>()
                {
                  new Callee
                  {
                    PrimaryPhone = "+447000000001",
                    SecondaryPhone = "+447000000001"
                  }
                }
              }
            }));

        var classToTest = new ReferralCallController(
          _mockHelper.Object,
          mockService.Object,
          Options.Create(options),
          _mockMapper.Object);

        int expected = StatusCodes.Status500InternalServerError;

        //Act
        var result = await classToTest.Get();

        //Assert
        result.Should().NotBeNull();
        var outputResult = Assert.IsType<ObjectResult>(result);
        outputResult.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task JsonToReferalCallRequiredIdMissingInvalid()
      {
        //Arrange
        ArcusOptions options = TestConfiguration.CreateArcusOptions().Value;
        options.IsNumberWhiteListEnabled = false;
        options.NumberWhiteList = new List<string>();
        options.ReturnLimit = 600;
        Mock<IOptions<ArcusOptions>> mockOptions =
          new Mock<IOptions<ArcusOptions>>();

        mockOptions.Setup(x => x.Value).Returns(options);

        var classToTest = new ReferralCallController(
          _mockHelper.Object,
          _mockChatBotService.Object,
          mockOptions.Object,
          _mockMapper.Object);

        int expected = (int)HttpStatusCode.OK;
        //Act
        var result = await classToTest.Get();

        //Assert
        Assert.NotNull(result);
        var outputResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(expected, outputResult.StatusCode);
      }

      [Fact]
      public async Task JsonToReferalCallIsValid()
      {
        //Arrange
        ArcusOptions options = TestConfiguration.CreateArcusOptions().Value;
        options.IsNumberWhiteListEnabled = false;
        options.NumberWhiteList = new List<string>();
        options.ContactFlowName = "NHS Weight Management Service";
        options.ReturnLimit = 600;
        Mock<IOptions<ArcusOptions>> mockOptions =
          new Mock<IOptions<ArcusOptions>>();

        mockOptions.Setup(x => x.Value).Returns(options);

        var classToTest = new ReferralCallController(
          _mockHelper.Object,
          _mockChatBotService.Object,
          mockOptions.Object,
          _mockMapper.Object);

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
           .Protected()
           // Setup the PROTECTED method to mock
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           // prepare the expected response of the mocked http call
           .ReturnsAsync(new HttpResponseMessage()
           {
             StatusCode = HttpStatusCode.OK,
             Content = new StringContent("[{'id':1,'value':'1'}]"),
           })
           .Verifiable();

        // use real http client with mocked handler here
        var httpClient = new HttpClient(handlerMock.Object)
        {
          BaseAddress = new Uri("http://test.com/"),
        };

        var expected = "1 call(s) prepared and 2 call(s) added to contact " +
          $"flow {options.ContactFlowName}. There were 0 duplicate and 0 " +
          "invalid number(s) processed.";

        //Act
        var result = await classToTest.Get();

        //Assert
        Assert.NotNull(result);
        var outputResult = Assert.IsType<OkObjectResult>(result);

        Assert.True(outputResult.StatusCode == 200);
        Assert.Equal(expected, outputResult.Value);
      }
    }
  }
}

