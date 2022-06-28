using AutoMapper;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Common.Helpers;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;
using Referral = WmsHub.Business.Entities.Referral;
using TextMessage = WmsHub.Business.Entities.TextMessage;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class TextServiceTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly Mock<DatabaseContext> _mockContext =
      new Mock<DatabaseContext>();
    private readonly Mock<IOptions<TextOptions>> _mockSettings =
      new Mock<IOptions<TextOptions>>();
    private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    private TextService _classToTest;
    private readonly Mock<TestNotificationClient> _moqClient =
      new Mock<TestNotificationClient>();
    private readonly Mock<TextNotificationHelper> _mockHelper;
    private readonly Mock<SmsNotificationResponse> _mockResponse =
      new Mock<SmsNotificationResponse>();

    public TextServiceTests(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      _context = new DatabaseContext(_serviceFixture.Options);

      _mockSettings.Setup(x =>
          x.Value.SmsApiKey).Returns(Environment.GetEnvironmentVariable(
            "WmsHub.GovUkNotify.Api_TextSettings:SmsApiKey"));
      _mockSettings.Setup(x =>
          x.Value.SmsSenderId).Returns(Environment.GetEnvironmentVariable(
            "WmsHub.GovUkNotify.Api_TextSettings:SmsSenderId"));
      _mockSettings.Setup(x => x.Value.GetTemplateIdFor(It.IsAny<string>()))
        .Returns(Guid.NewGuid());

      _mockHelper = new Mock<TextNotificationHelper>(_mockSettings.Object);

      _classToTest = new TextService(
        _mockSettings.Object,
        _mockHelper.Object,
        _mockMapper.Object,
        _mockContext.Object);

    }

    public class SendSmsMessageAsync : TextServiceTests
    {
      public SendSmsMessageAsync(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Theory]
      [InlineData("07715427599")]
      [InlineData("+447715427599")]
      public async Task SendSmsMessage_Test(string mobileNumber)
      {
        //Arrange
        var sms = new SmsMessage()
        {
          ClientReference = DateTime.Now.Date.ToString("yyyyMMdd"),
          MobileNumber = mobileNumber,
          Personalisation = new Dictionary<string, dynamic>
        {
            {"title", "Mr"},
            {"surname", "Test"},
            {"link", "https://www.midlandsandlancashirecsu.nhs.uk/"}
        },
          TemplateId = _mockSettings.Object.Value.GetTemplateIdFor(
            "templateId").ToString()
        };

        _moqClient.Setup(x => x.SendSmsAsync(It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<Dictionary<string, dynamic>>(),
          It.IsAny<string>(),
          It.IsAny<string>()
          )).Returns(Task.FromResult(_mockResponse.Object));

        _mockHelper.Setup(x => x.TextClient).Returns(_moqClient.Object);

        _classToTest = new TextService(_mockSettings.Object,
          _mockHelper.Object,
          _mockMapper.Object,
          _mockContext.Object);

        //Act
        var response = await _classToTest.SendSmsMessageAsync(sms);

        //Assert
        Assert.NotNull(response);
        Assert.Equal("SmsNotificationResponseProxy", response.GetType().Name);
      }

      [Theory]
      [InlineData(null, "The MobileNumber field is required.")]
      [InlineData("", "The MobileNumber field is required.")]
      [InlineData("error", "The MobileNumber field is invalid. " +
        "The MobileNumber field is too short.")]
      [InlineData("11", "The MobileNumber field is too short.")]
      [InlineData("11111rr11uu11111", "The MobileNumber field is invalid.")]
      public async Task SendSmsMessage_InvalidMobileNumber_Test(
        string mobileNumber, string expectedExceptionMessage)
      {
        //Arrange
        var sms = new SmsMessage()
        {
          ClientReference = DateTime.Now.Date.ToString("yyyyMMdd"),
          MobileNumber = mobileNumber,
          Personalisation = new Dictionary<string, dynamic>
        {
            {"title", "Mr"},
            {"surname", "Test"},
            {"link", "https://www.midlandsandlancashirecsu.nhs.uk/"}
        },
          TemplateId = _mockSettings.Object.Value.GetTemplateIdFor(
            "templateId").ToString()
        };

        //Act
        var ex = await Assert.ThrowsAsync<ValidationException>(
          () => _classToTest.SendSmsMessageAsync(sms));

        //Assert
        Assert.Equal(expectedExceptionMessage, ex.Message);
      }

      [Fact]
      public async Task SendSmsMessage_NotifyClientException_Test()
      {
        //Arrange
        string expectedExceptionMessage = "Test";
        var sms = new SmsMessage()
        {
          ClientReference = DateTime.Now.Date.ToString("yyyyMMdd"),
          MobileNumber = "+447777123456",
          Personalisation = new Dictionary<string, dynamic>
        {
            {"title", "Mr"},
            {"surname", "Test"},
            {"link", "https://www.midlandsandlancashirecsu.nhs.uk/"}
        },
          TemplateId = _mockSettings.Object.Value.GetTemplateIdFor(
           "templateId").ToString()
        };
        var exception = new NotifyClientException("Test");

        _moqClient.Setup(x => x.SendSmsAsync(It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<Dictionary<string, dynamic>>(),
          It.IsAny<string>(),
          It.IsAny<string>()
          )).Throws(exception);

        _mockHelper.Setup(x => x.TextClient).Returns(_moqClient.Object);

        _classToTest = new TextService(_mockSettings.Object,
          _mockHelper.Object,
          _mockMapper.Object,
         _mockContext.Object);

        //Act
        var ex = await Assert.ThrowsAsync<NotifyClientException>(
         () => _classToTest.SendSmsMessageAsync(sms));

        //Assert
        Assert.Equal(expectedExceptionMessage, ex.Message);
      }

    }

    public class PrepareMessagesToSend : TextServiceTests
    {
      public PrepareMessagesToSend(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      {
        _context.Referrals.RemoveRange(_context.Referrals);
        _context.SaveChanges();

        _classToTest = new TextService(
          _mockSettings.Object,
          _mockHelper.Object,
          _serviceFixture.Mapper,
          _context)
        {
          User = GetClaimsPrincipal()
        };
      }

      public static TheoryData<ReferralStatus, Referral> ShouldBePreparedData()
      {
        return new()
        {
          { ReferralStatus.FailedToContactTextMessage,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.FailedToContactTextMessage,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(),
                RandomEntityCreator.CreateRandomTextMessage()
              })},

          { ReferralStatus.ProviderRejectedTextMessage,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.ProviderRejectedTextMessage,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(),
                RandomEntityCreator.CreateRandomTextMessage()
              })},

          { ReferralStatus.ProviderTerminatedTextMessage,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.ProviderTerminatedTextMessage,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(),
                RandomEntityCreator.CreateRandomTextMessage()
              })},

          { ReferralStatus.CancelledDuplicateTextMessage,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.CancelledDuplicateTextMessage,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(),
                RandomEntityCreator.CreateRandomTextMessage()
              })},

          { ReferralStatus.TextMessage1,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.New) },

          { ReferralStatus.TextMessage1,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.New,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(
                  isActive: false,
                  sent: default)
              })},

          { ReferralStatus.TextMessage2,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.TextMessage1) },

          { ReferralStatus.TextMessage2,
            RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.TextMessage1,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(
                  isActive: false,
                  sent: DateTimeOffset.Now)
              })},
        };
      }

      [Theory]
      [MemberData(nameof(ShouldBePreparedData))]
      public async Task ShouldBePrepared_Update(
        ReferralStatus expectedStatus,
        Referral referral)
      {
        // arrange
        int expectedResponse = 1;

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        TextMessage expectedTextMessage = new()
        {
          // Base36DateSent - Not NULL
          // Id - Not Empty
          IsActive = true,
          // ModifiedAt - After referral.ModifiedAt
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          Number = referral.Mobile,
          Outcome = null,
          Received = null,
          // Referral - Not NULL
          ReferralId = referral.Id,
          Sent = default
        };

        // act
        var response = await _classToTest.PrepareMessagesToSend();

        // assert
        AssertShouldBePrepared(
          expectedStatus,
          referral,
          expectedResponse,
          expectedTextMessage,
          response);
      }

      [Fact]
      public async Task SingleReferral_ShouldBePrepared_Update()
      {
        // arrange
        int expectedResponse = 1;
        ReferralStatus expectedStatus = ReferralStatus.TextMessage1;

        var referral = RandomEntityCreator.CreateRandomReferral(
          status: ReferralStatus.New);

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        TextMessage expectedTextMessage = new()
        {
          // Base36DateSent - Not NULL
          // Id - Not Empty
          IsActive = true,
          // ModifiedAt - After referral.ModifiedAt
          ModifiedByUserId = Guid.Parse(TEST_USER_ID),
          Number = referral.Mobile,
          Outcome = null,
          Received = null,
          // Referral - Not NULL
          ReferralId = referral.Id,
          Sent = default
        };

        // act
        var response = await _classToTest.PrepareMessagesToSend(referral.Id);

        // assert
        AssertShouldBePrepared(
          expectedStatus,
          referral,
          expectedResponse,
          expectedTextMessage,
          response);
      }

      private void AssertShouldBePrepared(
        ReferralStatus expectedStatus,
        Referral referral,
        int expectedResponse,
        TextMessage expectedTextMessage,
        int response)
      {
        response.Should().Be(expectedResponse);

        var updatedReferral = _context.Referrals
          .Include(r => r.TextMessages)
          .Single(r => r.Id == referral.Id);

        updatedReferral.Should().BeEquivalentTo(referral, o => o
          .Excluding(r => r.Audits)
          .Excluding(r => r.ModifiedAt)
          .Excluding(r => r.ModifiedByUserId)
          .Excluding(r => r.Status)
          .Excluding(r => r.TextMessages));

        updatedReferral.Audits.Should().HaveCount(1);
        updatedReferral.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        updatedReferral.ModifiedByUserId.Should().Be(TEST_USER_ID);
        updatedReferral.Status.Should().Be(expectedStatus.ToString());

        var textMessage = updatedReferral.TextMessages
          .OrderByDescending(t => t.ModifiedAt)
          .First();
        textMessage.Should().BeEquivalentTo(expectedTextMessage, o => o
          .Excluding(t => t.Base36DateSent)
          .Excluding(t => t.Id)
          .Excluding(t => t.ModifiedAt)
          .Excluding(t => t.Referral));
        textMessage.Base36DateSent.Should().NotBeNull();
        textMessage.Id.Should().NotBeEmpty();
        textMessage.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
        textMessage.Referral.Should().NotBeNull();
      }

      public static TheoryData<Referral, int> ShouldNotBePreparedData()
      {
        TheoryData<Referral, int> data = new()
        {
          { RandomEntityCreator.CreateRandomReferral(
              isActive: false,
              status: ReferralStatus.FailedToContactTextMessage),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              isActive: false,
              status: ReferralStatus.ProviderRejectedTextMessage),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              isActive: false,
              status: ReferralStatus.ProviderTerminatedTextMessage),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              isActive: false,
              status: ReferralStatus.CancelledDuplicateTextMessage),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              isActive: false,
              status: ReferralStatus.New),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              providerId: Guid.NewGuid(),
              status: ReferralStatus.New),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.New,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(
                  isActive: true,
                  outcome: "SENT")
              }),
            1},

          { RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.New,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(
                  isActive: true,
                  sent: default)
              }),
            1},

          { RandomEntityCreator.CreateRandomReferral(
              isActive: false,
              status: ReferralStatus.TextMessage1),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              providerId: Guid.NewGuid(),
              status: ReferralStatus.TextMessage1),
            0},

          { RandomEntityCreator.CreateRandomReferral(
              status: ReferralStatus.TextMessage1,
              textMessages: new List<TextMessage>()
              {
                RandomEntityCreator.CreateRandomTextMessage(
                  isActive: true,
                  sent: DateTimeOffset.Now
                    .AddHours(- Constants.HOURS_BEFORE_NEXT_STAGE + 24)
                    .Date)
              }),
            1},
        };

        foreach (var referralStatus in Enum.GetValues(typeof(ReferralStatus)))
        {
          if (referralStatus is not ReferralStatus.New
            && !referralStatus.ToString().Contains("TextMessage"))
          {
            data.Add(
              RandomEntityCreator.CreateRandomReferral(
                providerId: Guid.NewGuid(),
                status: ReferralStatus.TextMessage1),
              0);
          }
        }

        return data;
      }

      [Theory]
      [MemberData(nameof(ShouldNotBePreparedData))]
      public async Task ShouldNotBePrepared_Update(
        Referral referral,
        int expectedTextMessagesCount)
      {
        // arrange
        _log.Information($"Status: {referral.Status}");
        int expectedResponse = 0;

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // act
        var response = await _classToTest.PrepareMessagesToSend();

        // assert
        response.Should().Be(expectedResponse);

        var updatedReferral = _context.Referrals
          .Include(r => r.TextMessages)
          .Single(r => r.Id == referral.Id);

        updatedReferral.Should().BeEquivalentTo(referral, o => o
          .Excluding(r => r.Audits)
          .Excluding(r => r.TextMessages));

        updatedReferral.Audits.Should().BeNull();
        updatedReferral.TextMessages.Should()
          .HaveCount(expectedTextMessagesCount);
      }

      [Fact]
      public async Task SingleReferralDoesNotExist_ShouldNotBePrepared()
      {
        // arrange
        int expectedResponse = 0;

        // act
        var response = await _classToTest.PrepareMessagesToSend(
          Guid.NewGuid());

        // assert
        response.Should().Be(expectedResponse);
      }
    }

    public class ReferralMobileNumberInvalid : TextServiceTests
    {
      public ReferralMobileNumberInvalid(ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
      { }

      [Fact]
      public async Task ReferralMobileNumberInvalid_ArgumentNullException_Test()
      {
        await Assert.ThrowsAsync<ArgumentNullException>(
          () => _classToTest.ReferralMobileNumberInvalidAsync(null));
      }

      [Fact]
      public async Task ReferralMobileNumberInvalid_InvalidStatus_Test()
      {
        //Arrange
        StatusType expectedStatus = StatusType.Invalid;
        string expectedError = "Referral Id is not a Guid.";
        CallbackRequest request = new CallbackRequest
        {
          Id = "ZZXZZ"
        };

        _classToTest = new TextService(_mockSettings.Object,
          _mockHelper.Object,
          _mockMapper.Object,
         _mockContext.Object);

        //Act
        var response =
          await _classToTest.ReferralMobileNumberInvalidAsync(request);

        //Assert
        Assert.Equal(expectedStatus, response.ResponseStatus);
        Assert.Equal(expectedError, response.Errors[0].ToString());
      }

      [Fact]
      public async Task ReferralMobileNumberInvalid__UpdateSuccess_Test()
      {
        //Arrange
        Guid refrralId = Guid.NewGuid();
        string expectedStatus = Enums.ReferralStatus.TextMessage2.ToString();
        bool expectedIsMobileValid = false;
        CallbackRequest request = new CallbackRequest
        {
          Id = refrralId.ToString()
        };
        SetLocalData(refrralId);

        _classToTest = new TextService(_mockSettings.Object,
          _mockHelper.Object,
          _mockMapper.Object,
          _context)
        {
          User = GetClaimsPrincipal()
        };

        //Act
        var response =
          await _classToTest.ReferralMobileNumberInvalidAsync(request);

        var updatedReferral = _context.Referrals.Single(r => r.Id == refrralId);

        //Assert
        updatedReferral.IsMobileValid.Should().Be(expectedIsMobileValid);
        updatedReferral.Status.Should().Be(expectedStatus);
      }

      private void SetLocalData(Guid refId)
      {
        Entities.Referral newReferral = _serviceFixture.Mapper.
          Map<Entities.Referral>(ServiceFixture.VALID_REFERRAL_ENTITY);
        newReferral.Id = refId;
        _context.Referrals.Add(newReferral);
        _context.SaveChanges();
      }
    }

    public class TestNotificationClient : IAsyncNotificationClient
    {
      public Tuple<string, string> ExtractServiceIdAndApiKey(string fromApiKey)
      {
        throw new NotImplementedException();
      }

      public Task<Notify.Models.Responses.TemplatePreviewResponse>
        GenerateTemplatePreviewAsync(
        string templateId,
        Dictionary<string, dynamic> personalisation = null)
      {
        throw new NotImplementedException();
      }

      public Task<string> GET(string url)
      {
        throw new NotImplementedException();
      }

      public Task<TemplateList> GetAllTemplatesAsync(
        string templateType = "")
      {
        throw new NotImplementedException();
      }

      public Task<Notification> GetNotificationByIdAsync(string notificationId)
      {
        throw new NotImplementedException();
      }

      public Task<NotificationList> GetNotificationsAsync(
        string templateType = "",
        string status = "",
        string reference = "",
        string olderThanId = "",
        bool includeSpreadsheetUploads = false)
      {
        throw new NotImplementedException();
      }

      public Task<ReceivedTextListResponse> GetReceivedTextsAsync(
        string olderThanId = "")
      {
        throw new NotImplementedException();
      }

      public Task<TemplateResponse> GetTemplateByIdAndVersionAsync(
        string templateId, int version = 0)
      {
        throw new NotImplementedException();
      }

      public Task<TemplateResponse> GetTemplateByIdAsync(string templateId)
      {
        throw new NotImplementedException();
      }

      public string GetUserAgent()
      {
        throw new NotImplementedException();
      }

      public Task<string> MakeRequest(
        string url,
        HttpMethod method,
        HttpContent content = null)
      {
        throw new NotImplementedException();
      }

      public Task<string> POST(string url, string json)
      {
        throw new NotImplementedException();
      }

      public Task<EmailNotificationResponse> SendEmailAsync(
        string emailAddress,
        string templateId,
        Dictionary<string, dynamic> personalisation = null,
        string clientReference = null,
        string emailReplyToId = null)
      {
        throw new NotImplementedException();
      }

      public Task<LetterNotificationResponse> SendLetterAsync(
        string templateId,
        Dictionary<string, dynamic> personalisation,
        string clientReference = null)
      {
        throw new NotImplementedException();
      }

      public Task<LetterNotificationResponse> SendPrecompiledLetterAsync(
        string clientReference,
        byte[] pdfContents,
        string postage)
      {
        throw new NotImplementedException();
      }

      public virtual Task<SmsNotificationResponse> SendSmsAsync(
        string mobileNumber,
        string templateId,
        Dictionary<string, dynamic> personalisation = null,
        string clientReference = null,
        string smsSenderId = null)
      {
        throw new NotImplementedException();
      }

      public Uri ValidateBaseUri(string baseUrl)
      {
        throw new NotImplementedException();
      }

    }

    [Collection("Service collection")]
    public class TextServiceIntergrationTest : ServiceTestsBase
    {
      private readonly DatabaseContext _context;
      private readonly TextService _classToTest;
      private readonly TextOptions _options = new TextOptions()
      {
        SmsTemplates = new List<SmsTemplate>
        {
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_FAILEDTOCONTACT),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_GENERAL_FIRST),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_GENERAL_SECOND),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_GP_FIRST),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_GP_SECOND),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_MSK_FIRST),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_MSK_SECOND),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_NONGP_DECLINED),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_NONGP_REJECTED),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_NONGP_TERMINATED),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_PHARMACY_FIRST),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_PHARMACY_SECOND),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_SELF_CANCELLEDDUPLICATE),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_SELF_FIRST),
          new SmsTemplate(
            Guid.NewGuid(),
            TextOptions.TEMPLATE_SELF_SECOND)
        }
      };
      private readonly Mock<IOptions<TextOptions>> _mockOptions =
        new Mock<IOptions<TextOptions>>();

      private readonly Mock<ITextNotificationHelper> _mockHelper =
        new Mock<ITextNotificationHelper>();
      public TextServiceIntergrationTest(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _mockOptions.Setup(t => t.Value).Returns(_options);
        _context = new DatabaseContext(_serviceFixture.Options);
        _classToTest = new TextService(_mockOptions.Object, _mockHelper.Object,
          _serviceFixture.Mapper, _context)
        {
          User = GetClaimsPrincipal()
        };

        _context.Referrals.RemoveRange(_context.Referrals);
        _context.TextMessages.RemoveRange(_context.TextMessages);
      }

      [Fact]
      public async Task GetMessagesToSendAsync_ProviderReferredToEReferrals()
      {
        string number = "+447715427599";
        ReferralStatus status = ReferralStatus.ProviderRejectedTextMessage;
        Referral referralToTest = RandomEntityCreator
          .CreateRandomReferral(status: status, mobile: number);
        _context.Referrals.Add(referralToTest);

        TextMessage txtMsg = RandomEntityCreator.CreateRandomTextMessage(
          number: number,
          isActive: true);
        txtMsg.Sent = default;
        txtMsg.ReferralId = referralToTest.Id;

        _context.TextMessages.Add(txtMsg);
        await _context.SaveChangesAsync();

        //act
        IEnumerable<ISmsMessage> result =
          await _classToTest.GetMessagesToSendAsync();
        //assert
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        referralToTest.Status.Should()
          .Be(ReferralStatus.ProviderRejectedTextMessage.ToString());
      }

      [Fact]
      public async Task GetMessagesToSendAsyncFailedToContact()
      {
        string number = "+447715427599";
        ReferralStatus status = ReferralStatus.FailedToContactTextMessage;
        Entities.Referral referralToTest = RandomEntityCreator
          .CreateRandomReferral(status: status, mobile: number);
        _context.Referrals.Add(referralToTest);

        Entities.TextMessage txtMsg = RandomEntityCreator
          .CreateRandomTextMessage(number: number,
            isActive: true);
        txtMsg.Sent = default;
        txtMsg.ReferralId = referralToTest.Id;

        _context.TextMessages.Add(txtMsg);
        await _context.SaveChangesAsync();

        //act
        IEnumerable<ISmsMessage> result =
          await _classToTest.GetMessagesToSendAsync();
        //assert
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        referralToTest.Status.Should()
          .Be(ReferralStatus.FailedToContactTextMessage.ToString());
      }

      [Fact]
      public async Task GetMessagesToSendAsyncProviderCompleted()
      {
        string number = "+447715427599";
        ReferralStatus status = ReferralStatus.ProviderTerminatedTextMessage;
        Entities.Referral referralToTest = RandomEntityCreator
          .CreateRandomReferral(status: status, mobile: number);
        _context.Referrals.Add(referralToTest);

        Entities.TextMessage txtMsg = RandomEntityCreator
          .CreateRandomTextMessage(number: number,
            isActive: true);
        txtMsg.Sent = default;
        txtMsg.ReferralId = referralToTest.Id;

        _context.TextMessages.Add(txtMsg);
        await _context.SaveChangesAsync();

        //act
        IEnumerable<ISmsMessage> result =
          await _classToTest.GetMessagesToSendAsync();
        //assert
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        referralToTest.Status.Should()
          .Be(ReferralStatus.ProviderTerminatedTextMessage.ToString());
      }

      [Fact]
      public async Task GetMessagesToSendAsyncCancelledDuplicate()
      {
        //Arrange
        var env = Environment.GetEnvironmentVariable("");
        string number = "+447715427599";
        ReferralStatus status = ReferralStatus.CancelledDuplicateTextMessage;
        string reason = "Duplicate NHS number found in UBRN abcde123456.";
        Entities.Referral referralToTest = RandomEntityCreator
          .CreateRandomReferral(status: status, mobile: number,
            statusReason: reason);
        _context.Referrals.Add(referralToTest);

        Entities.TextMessage txtMsg = RandomEntityCreator
          .CreateRandomTextMessage(number: number,
            isActive: true);
        txtMsg.Sent = default;
        txtMsg.ReferralId = referralToTest.Id;

        _context.TextMessages.Add(txtMsg);
        await _context.SaveChangesAsync();

        //act
        IEnumerable<ISmsMessage> result =
          await _classToTest.GetMessagesToSendAsync();
        //assert
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        referralToTest.Status.Should()
          .Be(ReferralStatus.CancelledDuplicateTextMessage.ToString());
      }
    }
  }

  [Collection("Service collection")]
  public class TextServiceIntergrationTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly Mock<TextNotificationHelper> _mockHelper;
    protected readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
    private readonly TextService _classToTest;

    public TextServiceIntergrationTests(ServiceFixture serviceFixture) : base(
      serviceFixture)
    {
      _mockHelper =
        new Mock<TextNotificationHelper>(
          TestConfiguration.CreateTextOptions());
      _context = new DatabaseContext(_serviceFixture.Options);

      _classToTest = new TextService(
        TestConfiguration.CreateTextOptions(),
        _mockHelper.Object,
        _mockMapper.Object,
        _context)
      {
        User = GetClaimsPrincipal()
      };
    }

    [Fact()]
    public async Task CallBackAsyncTest()
    {
      //Arrange
      Guid userid = Guid.NewGuid();
      Referral referral = RandomEntityCreator.CreateRandomReferral();
      TextMessage message =
        RandomEntityCreator.CreateRandomTextMessage();
      referral.TextMessages = new List<TextMessage> { message };
      int? expectedMethodOfContact = referral.MethodOfContact;
      int? expectedNumContacts = referral.NumberOfContacts;
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();


      ICallbackRequest request = new CallbackRequest
      {
        Id = "UUID",
        Reference = message.Id.ToString(),
        To = message.Number,
        Status = CallbackRequestHelper.CallbackStatus.Delivered,
        CreatedAt = DateTimeOffset.Now,
        CompletedAt = DateTimeOffset.Now,
        SentAt = DateTimeOffset.Now,
        NotificationType = CallbackRequestHelper.NotificationType.TextMessage
      };
      //Act
      Assert.Null(referral.IsMobileValid);
      CallbackResponse result =
        await _classToTest.CallBackAsync(request);
      //Assert
      result.ResponseStatus.Should().Be(StatusType.Valid);
      referral.IsMobileValid.Should().BeTrue();
      referral.MethodOfContact.Should().Be(expectedMethodOfContact);
      referral.NumberOfContacts.Should().Be(expectedNumContacts);
    }

  }

  public class TextServiceTestBase
  {
    private static readonly Mock<DatabaseContext> _mockContext =
      new Mock<DatabaseContext>();

    private static readonly TextOptions _options =
      TestConfiguration.CreateTextOptions().Value;

    public TextServiceTestBase()
    {
      _mockContext.Setup(t => t.SaveChangesAsync(CancellationToken.None))
        .Verifiable();
    }

    public class TextServiceUnitTest : TextService
    {
      public TextServiceUnitTest() :
        base(
          TestConfiguration.CreateTextOptions(),
          null,
          null,
          _mockContext.Object)
      { }

      protected override void UpdateModified(BaseEntity entity)
      {
        return;
      }

      public class UpdateMessageRequestAsyncTests : TextServiceUnitTest
      {
        private readonly Mock<Entities.Referral> _mockReferral =
          new Mock<Entities.Referral>();

        private readonly Mock<ISmsMessage> _mockSmsMessage = new Mock<ISmsMessage>();

        private readonly Mock<Entities.TextMessage> _mockTextMessage =
          new Mock<Entities.TextMessage>();

        [Fact]
        public async Task Valid()
        {
          //Arrange

          Guid textMessageId = Guid.NewGuid();
          _mockSmsMessage.Setup(t => t.LinkedTextMessage)
            .Returns(textMessageId);
          _mockTextMessage.Object.Id = textMessageId;
          _mockReferral.Object.NumberOfContacts = 0;
          _mockReferral.Object.MethodOfContact = 0;
          _mockReferral.Setup(t => t.TextMessages).Returns(
            new EditableList<Entities.TextMessage> { _mockTextMessage.Object });
          int expectedNumberOfContacts = 1;
          int expectedMethofOfContact = (int)MethodOfContact.TextMessage;
          string expectedOutcome = "SENT";
          //act
          try
          {
            await UpdateMessageRequestAsync(_mockSmsMessage.Object);

            Assert.True(true);
            _mockReferral.Object.NumberOfContacts.Should()
              .Be(expectedNumberOfContacts);
            _mockReferral.Object.MethodOfContact.Should()
              .Be(expectedMethofOfContact);
            _mockTextMessage.Object.Outcome.Should().Be(expectedOutcome);
          }
          catch (Exception ex)
          {
            Assert.True(false, $"Unexpected exception: {ex.Message} ");
          }
        }

        [Fact]
        public async Task Valid_ProviderRejectedToEReferrals()
        {
          //Arrange

          Guid textMessageId = Guid.NewGuid();
          _mockSmsMessage.Setup(t => t.LinkedTextMessage)
            .Returns(textMessageId);
          _mockTextMessage.Object.Id = textMessageId;
          _mockReferral.Object.NumberOfContacts = 0;
          _mockReferral.Object.MethodOfContact = 0;
          _mockReferral.Object.Status =
            ReferralStatus.ProviderRejectedTextMessage.ToString();
          _mockReferral.Object.StatusReason = "test";
          _mockReferral.Object.ReferralSource =
            ReferralSource.SelfReferral.ToString();
          _mockReferral.Setup(t => t.TextMessages).Returns(
            new EditableList<Entities.TextMessage> { _mockTextMessage.Object });
          int expectedNumberOfContacts = 1;
          int expectedMethofOfContact = (int)MethodOfContact.TextMessage;
          string expectedOutcome = "SENT";
          //act
          try
          {
            await UpdateMessageRequestAsync(_mockSmsMessage.Object);

            Assert.True(true);
            _mockReferral.Object.NumberOfContacts.Should()
              .Be(expectedNumberOfContacts);
            _mockReferral.Object.MethodOfContact.Should()
              .Be(expectedMethofOfContact);
            _mockTextMessage.Object.Outcome.Should().Be(expectedOutcome);
            _mockReferral.Object.Status.Should()
              .Be(ReferralStatus.Complete.ToString());
          }
          catch (Exception ex)
          {
            Assert.True(false, $"Unexpected exception: {ex.Message} ");
          }

        }

        [Fact]
        public async Task Valid_FailedToContact()
        {
          //Arrange

          Guid textMessageId = Guid.NewGuid();
          _mockSmsMessage.Setup(t => t.LinkedTextMessage)
            .Returns(textMessageId);
          _mockTextMessage.Object.Id = textMessageId;
          _mockReferral.Object.NumberOfContacts = 0;
          _mockReferral.Object.MethodOfContact = 0;
          _mockReferral.Object.Status =
            ReferralStatus.FailedToContactTextMessage.ToString();
          _mockReferral.Object.StatusReason = "test";
          _mockReferral.Object.ReferralSource =
            ReferralSource.SelfReferral.ToString();
          _mockReferral.Setup(t => t.TextMessages).Returns(
            new EditableList<Entities.TextMessage> { _mockTextMessage.Object });
          int expectedNumberOfContacts = 1;
          int expectedMethofOfContact = (int)MethodOfContact.TextMessage;
          string expectedOutcome = "SENT";
          //act
          try
          {
            await UpdateMessageRequestAsync(_mockSmsMessage.Object);

            Assert.True(true);
            _mockReferral.Object.NumberOfContacts.Should()
              .Be(expectedNumberOfContacts);
            _mockReferral.Object.MethodOfContact.Should()
              .Be(expectedMethofOfContact);
            _mockTextMessage.Object.Outcome.Should().Be(expectedOutcome);
            _mockReferral.Object.Status.Should()
              .Be(ReferralStatus.FailedToContact.ToString());
          }
          catch (Exception ex)
          {
            Assert.True(false, $"Unexpected exception: {ex.Message} ");
          }

        }

        [Fact]
        public async Task Valid_CancelledDuplicate()
        {
          //Arrange
          ReferralStatus expectedStatus = ReferralStatus.CancelledDuplicate;
          ReferralSource referralSource = ReferralSource.SelfReferral;
          Guid textMessageId = Guid.NewGuid();
          _mockSmsMessage.Setup(t => t.LinkedTextMessage)
            .Returns(textMessageId);
          _mockTextMessage.Object.Id = textMessageId;
          _mockReferral.Object.NumberOfContacts = 0;
          _mockReferral.Object.MethodOfContact = 0;
          _mockReferral.Object.Status =
            ReferralStatus.CancelledDuplicateTextMessage.ToString();
          _mockReferral.Object.StatusReason = "test";
          _mockReferral.Object.ReferralSource = referralSource.ToString();
          _mockReferral.Setup(t => t.TextMessages).Returns(
            new EditableList<TextMessage> { _mockTextMessage.Object });
          int expectedNumberOfContacts = 1;
          int expectedMethofOfContact = (int)MethodOfContact.TextMessage;
          string expectedOutcome = "SENT";
          //act
          try
          {
            await UpdateMessageRequestAsync(_mockSmsMessage.Object);

            Assert.True(true);
            _mockReferral.Object.NumberOfContacts.Should()
              .Be(expectedNumberOfContacts);
            _mockReferral.Object.MethodOfContact.Should()
              .Be(expectedMethofOfContact);
            _mockTextMessage.Object.Outcome.Should().Be(expectedOutcome);
            _mockReferral.Object.Status.Should().Be(expectedStatus.ToString());
          }
          catch (Exception ex)
          {
            Assert.True(false, $"Unexpected exception: {ex.Message} ");
          }

        }

        [Theory]
        [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
        [InlineData(ReferralSource.GpReferral, ReferralStatus.ProviderCompleted)]
        public async Task Valid_ProviderTerminated(
          ReferralSource referralSource,
          ReferralStatus expectedStatus)
        {
          //Arrange

          Guid textMessageId = Guid.NewGuid();
          _mockSmsMessage.Setup(t => t.LinkedTextMessage)
            .Returns(textMessageId);
          _mockTextMessage.Object.Id = textMessageId;
          _mockReferral.Object.NumberOfContacts = 0;
          _mockReferral.Object.MethodOfContact = 0;
          _mockReferral.Object.Status =
            ReferralStatus.ProviderTerminatedTextMessage.ToString();
          _mockReferral.Object.StatusReason = "test";
          _mockReferral.Object.ReferralSource = referralSource.ToString();
          _mockReferral.Setup(t => t.TextMessages).Returns(
            new EditableList<TextMessage> { _mockTextMessage.Object });
          int expectedNumberOfContacts = 1;
          int expectedMethofOfContact = (int)MethodOfContact.TextMessage;
          string expectedOutcome = "SENT";
          //act
          try
          {
            await UpdateMessageRequestAsync(_mockSmsMessage.Object);

            Assert.True(true);
            _mockReferral.Object.NumberOfContacts.Should()
              .Be(expectedNumberOfContacts);
            _mockReferral.Object.MethodOfContact.Should()
              .Be(expectedMethofOfContact);
            _mockTextMessage.Object.Outcome.Should().Be(expectedOutcome);
            _mockReferral.Object.Status.Should().Be(expectedStatus.ToString());
          }
          catch (Exception ex)
          {
            Assert.True(false, $"Unexpected exception: {ex.Message} ");
          }

        }

        protected override async Task<Entities.Referral>
          GetReferralWithTextMessage(Guid id)
        {
          return _mockReferral.Object;
        }
      }

      public class GetReferralTemplateIdTests : TextServiceUnitTest
      {
        private readonly Mock<Entities.Referral> _referral = new();
        private readonly string _gpSource = ReferralSource.GpReferral.ToString();

        private readonly string _srSource =
          ReferralSource.SelfReferral.ToString();

        private readonly string _prSource = ReferralSource.Pharmacy.ToString();

        public class GetReferralTests : GetReferralTemplateIdTests
        {
          [Fact]
          public void TextMessage1_TemplateFirstMessage_Expected()
          {
            //Arrange
            SmsTemplate expected =
              _options.SmsTemplates
                .FirstOrDefault(t => t.Name == "GpReferralFirst");
            _referral.Object.ReferralSource = _gpSource;
            _referral.Object.Status = ReferralStatus.TextMessage1.ToString();
            //act
            string result = GetReferralTemplateId(_referral.Object);
            //assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be(expected.Id.ToString());
          }

          [Fact]
          public void TextMessage2_TemplateSecondMessage_Expected()
          {
            //Arrange
            SmsTemplate expected =
              _options.SmsTemplates
                .FirstOrDefault(t => t.Name == "GpReferralSecond");
            _referral.Object.ReferralSource = _gpSource;
            _referral.Object.Status = ReferralStatus.TextMessage2.ToString();
            //act
            string result = GetReferralTemplateId(_referral.Object);
            //assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be(expected.Id.ToString());
          }
        }

        public class GetSelfReferralTests : GetReferralTemplateIdTests
        {
          [Fact]
          public void TextMessage1_TemplateFirstMessage_Expected()
          {
            //Arrange
            SmsTemplate expected =
              _options.SmsTemplates
                .FirstOrDefault(t => t.Name == "StaffReferralFirstMessage");
            _referral.Object.ReferralSource = _srSource;
            _referral.Object.Status = ReferralStatus.TextMessage1.ToString();
            //act
            string result = GetReferralTemplateId(_referral.Object);
            //assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be(expected.Id.ToString());
          }

          [Fact]
          public void TextMessage2_TemplateSecondMessage_Expected()
          {
            //Arrange
            SmsTemplate expected =
              _options.SmsTemplates
                .FirstOrDefault(t => t.Name == "StaffReferralSecondMessage");
            _referral.Object.ReferralSource = _srSource;
            _referral.Object.Status = ReferralStatus.TextMessage2.ToString();
            //act
            string result = GetReferralTemplateId(_referral.Object);
            //assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be(expected.Id.ToString());
          }
        }

        public class GetPharmacyReferralTests : GetReferralTemplateIdTests
        {
          [Fact]
          public void TextMessage1_TemplateFirstMessage_Expected()
          {
            //Arrange
            SmsTemplate expected =
              _options.SmsTemplates
                .FirstOrDefault(t => t.Name == "PharmacyReferralFirst");
            _referral.Object.ReferralSource = _prSource;
            _referral.Object.Status = ReferralStatus.TextMessage1.ToString();
            //act
            string result = GetReferralTemplateId(_referral.Object);
            //assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be(expected.Id.ToString());
          }

          [Fact]
          public void TextMessage2_TemplateSecondMessage_Expected()
          {
            //Arrange
            SmsTemplate expected =
              _options.SmsTemplates
                .FirstOrDefault(t => t.Name == "PharmacyReferralSecond");
            _referral.Object.ReferralSource = _prSource;
            _referral.Object.Status = ReferralStatus.TextMessage2.ToString();
            //act
            string result = GetReferralTemplateId(_referral.Object);
            //assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be(expected.Id.ToString());
          }
        }

        public class GetCancelledDUplicateReferralTests :
          GetReferralTemplateIdTests
        {
          [Fact]
          public void CancelledDuplicate_TextMessage_Expected()
          {
            //Arrange
            SmsTemplate expected = _options.SmsTemplates
              .FirstOrDefault(t => t.Name == "StaffReferralCancelledDuplicate");
            _referral.Object.ReferralSource = _srSource;
            _referral.Object.Status =
              ReferralStatus.CancelledDuplicateTextMessage.ToString();
            //act
            string result = GetReferralTemplateId(_referral.Object);
            //assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be(expected.Id.ToString());
          }

        }
      }
    }
  }
}
