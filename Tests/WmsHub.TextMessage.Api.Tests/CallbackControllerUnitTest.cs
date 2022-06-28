using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Models.Responses;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.TextMessage.Api.Controllers;
using WmsHub.TextMessage.Api.Models.Notify;
using WmsHub.TextMessage.Api.Models.Profiles;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.TextMessage.Api.Tests
{
  [Collection("Service collection")]
  public class CallbackControllerUnitTest
  {

    private readonly Mock<DatabaseContext> _mockContext = 
      new Mock<DatabaseContext>();
    private readonly Mock<ICallbackRequest> _mockCallback =
      new Mock<ICallbackRequest>();
    private readonly Mock<IOptions<TextOptions>> _mockSettings =
      new Mock<IOptions<TextOptions>>();
    private readonly Mock<TextService> _mockService;
    private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    private readonly Mock<TextNotificationHelper> _mockHelper;
    private CallbackController _classToTest;
    private readonly IMapper _mapper;

    public CallbackControllerUnitTest(ITestOutputHelper output)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
        .CreateLogger();

      _mockService = new Mock<TextService>(_mockContext.Object);
      _mockCallback.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());
      _mockCallback.Setup(x => x.CreatedAt).Returns(DateTime.UtcNow);
      _mockCallback.Setup(x => x.Status)
        .Returns(CallbackStatus.Delivered.ToString());
      _mockCallback.Setup(x => x.Reference).Returns(Guid.NewGuid().ToString());
      _mockCallback.Setup(x => x.NotificationType)
        .Returns(CallbackNotification.Sms.ToString());
      _mockCallback.Setup(x => x.To).Returns("077000");

      _mockHelper = new Mock<TextNotificationHelper>(_mockSettings.Object);
      _mockService = new Mock<TextService>(_mockSettings.Object,
        _mockHelper.Object,
        _mockMapper.Object,
        _mockContext.Object);

      var callbackProfile = new CallbackRequestProfile();
      var configuration =
        new MapperConfiguration(cfg => cfg.AddProfile(callbackProfile));
      _mapper = new Mapper(configuration);
    }

    public class CallbackRequesTests : CallbackControllerUnitTest
    {

      public CallbackRequesTests(ITestOutputHelper output) : base(output) { }

      public class CallbackIstrue : CallbackRequesTests
      {
        public CallbackIstrue(ITestOutputHelper output) : base(output)
        {
          _mockCallback.Setup(x => x.IsCallback).Returns(true);

        }

        public class ValidCallbacks : CallbackIstrue
        {
          public ValidCallbacks(ITestOutputHelper output) : base(output) { }

          [Theory]
          [InlineData(StatusType.Valid, typeof(OkObjectResult))]
          [InlineData(StatusType.CallIdDoesNotExist, typeof(NotFoundResult))]
          [InlineData(StatusType.StatusIsUnknown, 
            typeof(BadRequestObjectResult))]
          [InlineData(StatusType.UnableToFindReferral, 
            typeof(BadRequestObjectResult))]
          [InlineData(StatusType.TelephoneNumberMismatch, 
            typeof(BadRequestObjectResult))]
          public async Task StatusResponse(StatusType status, Type expected)
          {
            //Arrange
            var apiRequest = new CallbackPostRequest
            {
              Status = EnumDescriptionHelper
              .GetDescriptionFromEnum(CallbackStatus.Delivered)
            };

            var request = new CallbackRequest(_mockCallback.Object);
            var callbackResponse =
                new Mock<CallbackResponse>(request);
            callbackResponse.Setup(x => x.ResponseStatus)
              .Returns(status);
            callbackResponse.Setup(x => x.GetErrorMessage())
              .Returns("Test Fail");
            _mockService.Setup(x => x.CallBackAsync(
              It.IsAny<ICallbackRequest>()))
                .Returns(Task.FromResult(callbackResponse.Object));

            _classToTest = new CallbackController(
              _mockService.Object,
              _mapper,
              _mockSettings.Object);

            //Act
            var response = await _classToTest.Post(apiRequest);

            //Assert
            Assert.NotNull(response);
            Assert.Equal(expected.Name, response.GetType().Name);
          }
        }

        [Fact]
        public async Task ValidCallback()
        {
          var apiRequest = new CallbackPostRequest
          {
            Status = EnumDescriptionHelper
            .GetDescriptionFromEnum(CallbackStatus.Delivered)
          };

          var request = new CallbackRequest(_mockCallback.Object);
          var callbackResponse = new Mock<CallbackResponse>(request);

          callbackResponse.Setup(x => x.ResponseStatus)
            .Returns(Business.Enums.StatusType.Valid);
          _mockService.Setup(x => x.CallBackAsync(It.IsAny<ICallbackRequest>()))
            .Returns(Task.FromResult(callbackResponse.Object));

          CallbackController controller =
            new CallbackController(
              _mockService.Object,
              _mapper,
              _mockSettings.Object);

          //Act
          var response = await controller.Post(apiRequest);

          //Assert
          Assert.NotNull(response);
          Assert.True(response is OkObjectResult);
        }

        [Fact]
        public async Task ValidReplyMessage()
        {
          //Arrange
          _mockCallback.Setup(x => x.Status).Returns(
            CallbackStatus.None.ToString());
          _mockCallback.Setup(x => x.IsCallback).Returns(true);
          _mockCallback.Setup(x => x.Message).Returns("Test Message");

          var apiRequest = new CallbackPostRequest
          {
            Status = CallbackStatus.None.ToString()
          };
          var request = new CallbackRequest(_mockCallback.Object);

          _mockSettings.Setup(x => x.Value
          .GetTemplateIdFor(It.IsAny<string>()))
            .Returns(Guid.NewGuid);

          var smsResponse = new Mock<SmsNotificationResponse>();

          _mockService.Setup(x => x.SendSmsMessageAsync(
            It.IsAny<ISmsMessage>()))
              .Returns(Task.FromResult(smsResponse.Object));

          var controller = new CallbackController(
            _mockService.Object,
            _mapper,
            _mockSettings.Object);

          //Act
          var response = await controller.Post(apiRequest);

          //Assert
          Assert.NotNull(response);
          Assert.True(response is OkObjectResult);
        }
        [Theory]
        [InlineData(StatusType.Valid, typeof(OkObjectResult))]
        [InlineData(StatusType.Invalid, typeof(BadRequestObjectResult))]
        [InlineData(StatusType.UnableToFindReferral,
          typeof(BadRequestObjectResult))]
        public async Task MobileNotValid(StatusType status, Type expected)
        {
          //Arrange
          var apiRequest = new CallbackPostRequest
          {
            Status = EnumDescriptionHelper
            .GetDescriptionFromEnum(CallbackStatus.PermanentFailure)
          };

          var request = new CallbackRequest(_mockCallback.Object);
          var callbackResponse =
              new Mock<CallbackResponse>(request);
          callbackResponse.Setup(x => x.ResponseStatus)
            .Returns(status);

          _mockService.Setup(x =>
            x.ReferralMobileNumberInvalidAsync(It.IsAny<ICallbackRequest>()))
            .Returns(Task.FromResult(callbackResponse.Object));

          _classToTest = new CallbackController(
            _mockService.Object,
            _mapper,
            _mockSettings.Object);

          //Act
          var response = await _classToTest.Post(apiRequest);

          //Assert
          Assert.NotNull(response);
          Assert.Equal(expected.Name, response.GetType().Name);
        }
      }
    }
  }
}
