using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Business.Services;
using WmsHub.ChatBot.Api.Clients;
using WmsHub.ChatBot.Api.Controllers;
using WmsHub.ChatBot.Api.Models;
using WmsHub.Common.Exceptions;
using Xunit;

namespace WmsHub.ChatBot.Api.Tests
{
  public class ChatBotApiControllerTests : ReferralCallController
  {

    private readonly dynamic _call = new JObject();
    private readonly Mock<DatabaseContext> _mockContext =
      new Mock<DatabaseContext>();
    private static readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    private static readonly Mock<IOptions<ArcusOptions>> _mockOptions =
      new Mock<IOptions<ArcusOptions>>();
    private readonly Mock<ArcusOptions> _mockSettings =
      new Mock<ArcusOptions>();

    private static readonly Mock<IArcusClientHelper> _mockClientHelper = new();

    private readonly Mock<ChatBotService> _mockChatBotService;

    private readonly dynamic _transfer = new JObject();
    private readonly Mock<UpdateReferralTransferRequest>
      _mockTransferRequest = new Mock<UpdateReferralTransferRequest>(
        "test",
        "+447123700001",
        (DateTimeOffset)DateTime.UtcNow);
    private const string CONTACT_FLOW_NAME = "NHS Weight Management";

    private Mock<UpdateReferralTransferResponse> _mockTransferResponse;

    public ChatBotApiControllerTests() :
      base(_mockClientHelper.Object, null, _mockOptions.Object,
        _mockMapper.Object)
    {
      _mockSettings.Object.ReturnLimit = 600;
      _mockSettings.Object.ContactFlowName = CONTACT_FLOW_NAME;
      _mockOptions.Setup(x => x.Value).Returns(_mockSettings.Object);

      _mockChatBotService = new Mock<ChatBotService>(
        _mockContext.Object,
        _mockMapper.Object,
        _mockOptions.Object,
        null);

      
    }

    protected override void LogInformation(string message)
    {
      Assert.True(true, message);
    }

    protected override ChatBotService Service
    {
      get
      {
        return _mockChatBotService.Object;
      }
    }

    public class PostChatBotTransferTests : ChatBotApiControllerTests
    {
      public PostChatBotTransferTests()
      { }

      [Fact]
      public async Task JsonToReferralTransfer_RequiredPhoneInvalid()
      {
        //Arrange
        int expectedStatus = 400;
        var expected = "The field Number must match the " +
                       "regular expression '^\\+[0-9]+$'.";

        _transfer.Outcome = "Test";
        _transfer.Number = "7395700001";
        _transfer.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_transfer);

        TransferRequest request = JsonConvert
          .DeserializeObject<TransferRequest>(json);

        _mockTransferResponse =
          new Mock<UpdateReferralTransferResponse>(_mockTransferRequest.Object);

        _mockTransferResponse.Setup(x => x.ResponseStatus)
          .Returns(WmsHub.Business.Enums.StatusType.Invalid);

        _mockTransferResponse.Setup(x => x.GetErrorMessage())
          .Returns(expected);

        _mockChatBotService.Setup(x => x.UpdateReferralTransferRequestAsync(
            It.IsAny<UpdateReferralTransferRequest>()))
          .Returns(Task.FromResult(_mockTransferResponse.Object));

        //Act
        var result = await PostChatBotTransfer(request);

        //Assert
        result.Should().NotBeNull();
        BadRequestObjectResult outputResult =
          Assert.IsType<BadRequestObjectResult>(result);
        outputResult.StatusCode.Should().Be(expectedStatus);
        outputResult.Value.Should().Be(expected);
      }

      [Fact]
      public async Task JsonToReferralTransfer_NoReferralFound()
      {
        //Arrange
        string telephone = "+7567700001";
        int expectedStatus = 400;
        var expected = "No referral found with associated phone number " +
                       $"{telephone}.";

        _transfer.Outcome = "Test";
        _transfer.Number = telephone;
        _transfer.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_transfer);

        TransferRequest request = JsonConvert
          .DeserializeObject<TransferRequest>(json);

        _mockTransferResponse =
          new Mock<UpdateReferralTransferResponse>(_mockTransferRequest.Object);

        _mockTransferResponse.Setup(x => x.ResponseStatus)
          .Returns(WmsHub.Business.Enums.StatusType.Invalid);

        _mockTransferResponse.Setup(x => x.GetErrorMessage()).Returns(expected);

        _mockChatBotService.Setup(x => x.UpdateReferralTransferRequestAsync(
            It.IsAny<UpdateReferralTransferRequest>()))
          .Returns(Task.FromResult(_mockTransferResponse.Object));

        //Act
        var result = await PostChatBotTransfer(request);

        //Assert
        result.Should().NotBeNull();
        BadRequestObjectResult outputResult =
          Assert.IsType<BadRequestObjectResult>(result);
        outputResult.StatusCode.Should().Be(expectedStatus);
        outputResult.Value.Should().Be(expected);
      }

      [Fact]
      public async Task JsonToReferralTransfer_NoDistinctReferralFound()
      {
        //Arrange
        string telephone = "+7567700001";
        int expectedStatus = 400;
        var expected = "No distinct referral found " +
                       $"with associated phone number {telephone}.";
        _transfer.Outcome = "Test";
        _transfer.Number = telephone;
        _transfer.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_transfer);

        TransferRequest request = JsonConvert
          .DeserializeObject<TransferRequest>(json);

        _mockTransferResponse =
          new Mock<UpdateReferralTransferResponse>(_mockTransferRequest.Object);

        _mockTransferResponse.Setup(x => x.ResponseStatus)
          .Returns(WmsHub.Business.Enums.StatusType.Invalid);

        _mockTransferResponse.Setup(x => x.GetErrorMessage()).Returns(expected);

        _mockChatBotService.Setup(x => x.UpdateReferralTransferRequestAsync(
            It.IsAny<UpdateReferralTransferRequest>()))
          .Returns(Task.FromResult(_mockTransferResponse.Object));

        //Act
        var result = await PostChatBotTransfer(request);

        //Assert
        result.Should().NotBeNull();
        BadRequestObjectResult outputResult =
          Assert.IsType<BadRequestObjectResult>(result);
        outputResult.StatusCode.Should().Be(expectedStatus);
        outputResult.Value.Should().Be(expected);
      }

      [Fact]
      public async Task JsonToReferralTransfer_UpdateSuccess()
      {
        //Arrange
        int expectedStatus = 200;

        _transfer.Outcome = WmsHub.Business.Enums.ChatBotCallOutcome
          .TransferringToRmc.ToString();
        _transfer.Number = "7395700001";
        _transfer.TimeStamp = (DateTimeOffset)DateTime.UtcNow;

        var json = JsonConvert.SerializeObject(_transfer);

        TransferRequest request = JsonConvert
          .DeserializeObject<TransferRequest>(json);

        _mockTransferResponse =
          new Mock<UpdateReferralTransferResponse>(_mockTransferRequest.Object);

        _mockTransferResponse.Setup(x => x.ResponseStatus)
          .Returns(WmsHub.Business.Enums.StatusType.Valid);

        _mockChatBotService.Setup(x => x.UpdateReferralTransferRequestAsync(
            It.IsAny<UpdateReferralTransferRequest>()))
          .Returns(Task.FromResult(_mockTransferResponse.Object));

        //Act
        var result = await PostChatBotTransfer(request);

        //Assert
        result.Should().NotBeNull();
        OkResult outputResult = Assert.IsType<OkResult>(result);
        outputResult.StatusCode.Should().Be(expectedStatus);
      }
    }

    public class PostTests : ChatBotApiControllerTests
    {
      public PostTests() { }
      [Fact]
      public async Task ValidReferralCall_Return_OK()
      {
        //Arrange
        Mock<ReferralCall> referralCall = new();
        Mock<UpdateReferralWithCallRequest> request = new();
        Mock<UpdateReferralWithCallResponse> mockResponse = new(request.Object);

        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Valid);
        _mockMapper
          .Setup(t =>
            t.Map<ReferralCall, UpdateReferralWithCallRequest>(
              It.IsAny<ReferralCall>())).Returns(request.Object);

        _mockChatBotService
          .Setup(t =>
            t.UpdateReferralWithCall(It.IsAny<UpdateReferralWithCallRequest>()))
          .ReturnsAsync(mockResponse.Object);
       
        //Act
        var response = await Post(referralCall.Object);
        //Assert
        response.Should().BeOfType<OkResult>();
      }

      [Fact]
      public async Task InValid_Default_Id_Guid_Return_BadRequest()
      {
        //Arrange
        Mock<ReferralCall> referralCall = new();
        referralCall.Object.Id = Guid.Empty;
        List<string> errors = new List<string> {"Test Error"};
        Mock<UpdateReferralWithCallRequest> request = new();
        Mock<UpdateReferralWithCallResponse> mockResponse = new(request.Object);

        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Invalid);
        mockResponse.Setup(t => t.Errors).Returns(errors);
        _mockMapper
          .Setup(t =>
            t.Map<ReferralCall, UpdateReferralWithCallRequest>(
              It.IsAny<ReferralCall>())).Returns(request.Object);

        _mockChatBotService
          .Setup(t =>
            t.UpdateReferralWithCall(It.IsAny<UpdateReferralWithCallRequest>()))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await Post(referralCall.Object);
        //Assert
        response.Should().BeOfType<BadRequestObjectResult>();
      }

      [Fact]
      public async Task InValid_Exception_Return_StatusCode500()
      {
        //Arrange
        Mock<ReferralCall> referralCall = new();
        Mock<UpdateReferralWithCallRequest> request = new();
        Mock<UpdateReferralWithCallResponse> mockResponse = new(request.Object);

        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Valid);
        _mockMapper
          .Setup(t =>
            t.Map<ReferralCall, UpdateReferralWithCallRequest>(
              It.IsAny<ReferralCall>())).Returns(request.Object);

        _mockChatBotService
          .Setup(t =>
            t.UpdateReferralWithCall(It.IsAny<UpdateReferralWithCallRequest>()))
          .Throws(new Exception("Test"));

        //Act
        var response = await Post(referralCall.Object);
        //Assert
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(500);
      }
    }

    public class GetTests : ChatBotApiControllerTests
    {
      public GetTests()
      {
        Mock<ArcusOptions> mockOptions = new();
        mockOptions.Setup(t=>t.ValidateNumbersAgainstWhiteList(It.IsAny<List<string>>())).Verifiable();        
        mockOptions.Object.ReturnLimit = 600;
        mockOptions.Object.ContactFlowName = CONTACT_FLOW_NAME;
        _mockOptions.Setup(t => t.Value).Returns(mockOptions.Object);
        _options = mockOptions.Object;
      }

      [Fact]
      public async Task Valid_Result_Return_OK()
      {
        //Arrange        
        string expected =
          "1 call(s) prepared and 0 call(s) added to contact flow " +
          $"{CONTACT_FLOW_NAME}. There " +
          "were 0 duplicate and 0 invalid number(s) processed.";
        int numberOfCalls = 1;
        Mock<PrepareCallsForTodayResponse> mockPrepResponse = new();
        mockPrepResponse.Object.CallsPrepared = numberOfCalls;
        Mock<ICallee> mockCallee = new();
        mockCallee.Object.PrimaryPhone = "+447395701701";
        mockCallee.Object.SecondaryPhone = "+447395702702";
        IEnumerable<ICallee> callees = new[] {mockCallee.Object};
        Mock<ArcusCall> mockArcus = new();
        mockArcus.Setup(t => t.Callees).Returns(callees);
        Mock<GetReferralCallListResponse> mockResponse = new();
        mockResponse.Setup(t => t.Arcus).Returns(mockArcus.Object);
        mockResponse.Setup(t => t.Status).Returns(StatusType.Valid);


        _mockChatBotService.Setup(t => t.PrepareCallsAsync())
          .ReturnsAsync(mockPrepResponse.Object);
        
        _mockChatBotService.Setup(t => t
          .GetReferralCallList(It.IsAny<GetReferralCallListRequest>()))
          .ReturnsAsync(mockResponse.Object);
        
        HttpContent content = new StringContent("test");
        _mockClientHelper.Setup(t => t.BatchPost(It.IsAny<ArcusCall>()))
          .ReturnsAsync(new HttpResponseMessage
            {StatusCode = HttpStatusCode.Created, Content = content});

        _mockChatBotService.Setup(t =>
            t.UpdateReferralCallListSent(It.IsAny<IEnumerable<ICallee>>()))
          .Verifiable();
        //Act
        var response = await Get();
        //Assert
        response.Should().BeOfType<OkObjectResult>();
        OkObjectResult result =
          response as OkObjectResult;
        result.StatusCode.Should().Be(200);
        result.Value.Should().Be(expected);
      }

      [Fact]
      public async Task InvalidStatus_Returns_BadRequest()
      {
        //Arrange
        Mock<ArcusOptions> mockOptions = new();
        mockOptions.Object.ReturnLimit = 600;

        int numberOfCalls = 1;
        Mock<PrepareCallsForTodayResponse> mockPrepResponse = new();
        mockPrepResponse.Object.CallsPrepared = numberOfCalls;
        Mock<ICallee> mockCallee = new();
        mockCallee.Object.PrimaryPhone = "+447395701701";
        mockCallee.Object.SecondaryPhone = "+447395702702";
        IEnumerable<ICallee> callees = new[] { mockCallee.Object };
        Mock<ArcusCall> mockArcus = new();
        mockArcus.Setup(t => t.Callees).Returns(callees);
        Mock<GetReferralCallListResponse> mockResponse = new();
        mockResponse.Setup(t => t.Arcus).Returns(mockArcus.Object);
        mockResponse.Setup(t => t.Status).Returns(StatusType.Invalid);
        List<string> errors = new List<string> { "Test Error" };
        mockResponse.Setup(t => t.Errors).Returns(errors);


        _mockChatBotService.Setup(t => t.PrepareCallsAsync())
          .ReturnsAsync(mockPrepResponse.Object);
        _mockChatBotService
          .Setup(t =>
            t.GetReferralCallList(It.IsAny<GetReferralCallListRequest>()))
          .ReturnsAsync(mockResponse.Object);
        HttpContent content = new StringContent("test");
        _mockClientHelper.Setup(t => t.BatchPost(It.IsAny<ArcusCall>()))
          .ReturnsAsync(new HttpResponseMessage
            { StatusCode = HttpStatusCode.Created, Content = content });
        _mockChatBotService.Setup(t =>
            t.UpdateReferralCallListSent(It.IsAny<IEnumerable<ICallee>>()))
          .Verifiable();
        //Act
        var response = await Get();
        //Assert
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(400);
      }

      [Fact]
      public async Task NumberWhiteListException_Result_Return_500()
      {
        //Arrange
        Mock<ArcusOptions> mockOptions = new();
        mockOptions.Object.ReturnLimit = 600;
        mockOptions.Setup(t => t.ValidateNumbersAgainstWhiteList(
          It.IsAny<List<string>>()))
          .Throws(new NumberWhiteListException("Test"));
        _mockOptions.Setup(t => t.Value).Returns(mockOptions.Object);
        _options = mockOptions.Object;

        int numberOfCalls = 1;
        Mock<PrepareCallsForTodayResponse> mockPrepResponse = new();
        mockPrepResponse.Object.CallsPrepared = numberOfCalls;
        Mock<ICallee> mockCallee = new();
        mockCallee.Object.PrimaryPhone = "+447395701701";
        mockCallee.Object.SecondaryPhone = "+447395702702";
        IEnumerable<ICallee> callees = new[] { mockCallee.Object };
        Mock<ArcusCall> mockArcus = new();
        mockArcus.Setup(t => t.Callees).Returns(callees);
        Mock<GetReferralCallListResponse> mockResponse = new();
        mockResponse.Setup(t => t.Arcus).Returns(mockArcus.Object);
        mockResponse.Setup(t => t.Status).Returns(StatusType.Valid);


        _mockChatBotService.Setup(t => t.PrepareCallsAsync())
          .ReturnsAsync(mockPrepResponse.Object);
        _mockChatBotService
          .Setup(t =>
            t.GetReferralCallList(It.IsAny<GetReferralCallListRequest>()))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await Get();
        //Assert
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(500);
      }
    }
  }
}