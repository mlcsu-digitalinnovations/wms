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

using static WmsHub.Common.Helpers.Constants.MessageTemplateConstants;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Business.Exceptions;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class TextServiceTests : ServiceTestsBase, IDisposable
{
  private const int MaxDaysSinceInitialContactToSendTextMessage3 = 35;
  private readonly DatabaseContext _context;
  private readonly Mock<DatabaseContext> _mockContext = new();
  private readonly Mock<LinkIdService> _mockLinkIdService = new();
  private TextService _textService;
  private readonly Mock<TestNotificationClient> _moqClient = new();
  private readonly Mock<TextNotificationHelper> _mockHelper;
  private readonly Mock<SmsNotificationResponse> _mockResponse = new();
  private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new();
  private readonly Mock<TextOptions> _mockTextOptions = new();


  public TextServiceTests(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
    : base(serviceFixture, testOutputHelper)
  {
    _context = new DatabaseContext(_serviceFixture.Options);

    MessageTimelineOptions messageTimelineOptions = new()
    {
      MaxDaysSinceInitialContactToSendTextMessage3 = MaxDaysSinceInitialContactToSendTextMessage3,
      MinHoursSincePreviousContactToSendTextMessage1 = Constants.HOURS_BEFORE_NEXT_STAGE,
      MinHoursSincePreviousContactToSendTextMessage2 = Constants.HOURS_BEFORE_NEXT_STAGE,
      MinHoursSincePreviousContactToSendTextMessage3 = Constants.HOURS_BEFORE_TEXTMESSAGE3
    };

    _mockTextOptions.Object.MessageTimelineOptions = messageTimelineOptions;
    _mockTextOptions.Object.SmsApiKey = "TestSmsApiKey";
    _mockTextOptions.Object.SmsSenderId = Guid.NewGuid().ToString();
    _mockTextOptions.Setup(x => x
      .GetTemplateIdFor(It.IsAny<string>()))
      .Returns(Guid.NewGuid());

    _mockHelper = new Mock<TextNotificationHelper>(
      Options.Create(_mockTextOptions.Object));

    _mockDateTimeProvider.Setup(dtp => dtp.UtcNow).Returns(DateTimeOffset.UtcNow);

    _textService = new TextService(
      Options.Create(_mockTextOptions.Object),
      _mockHelper.Object,
      _mockContext.Object,
      _mockDateTimeProvider.Object,
      _mockLinkIdService.Object);

    CleanUp();
  }

  public void Dispose()
  {
    CleanUp();
    GC.SuppressFinalize(this);
  }

  private void CleanUp()
  {
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.TextMessages.RemoveRange(_context.TextMessages);
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
      // Arrange.
      SmsMessage sms = new()
      {
        ClientReference = DateTime.Now.Date.ToString("yyyyMMdd"),
        MobileNumber = mobileNumber,
        Personalisation = new Dictionary<string, dynamic>
      {
          {"title", "Mr"},
          {"surname", "Test"},
          {"link", "https://www.midlandsandlancashirecsu.nhs.uk/"}
      },
        TemplateId = _mockTextOptions
          .Object.GetTemplateIdFor("templateId").ToString()
      };

      _moqClient.Setup(x => x.SendSmsAsync(It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Dictionary<string, dynamic>>(),
        It.IsAny<string>(),
        It.IsAny<string>()
        )).Returns(Task.FromResult(_mockResponse.Object));

      _mockHelper.Setup(x => x.TextClient).Returns(_moqClient.Object);

      _textService = new TextService(
        Options.Create(_mockTextOptions.Object),
        _mockHelper.Object,
        _mockContext.Object,
        _mockDateTimeProvider.Object,
        _mockLinkIdService.Object);

      // Act.
      var response = await _textService.SendSmsMessageAsync(sms);

      // Assert.
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
      // Arrange.
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
        TemplateId = _mockTextOptions
          .Object.GetTemplateIdFor("templateId").ToString()
      };

      // Act.
      var ex = await Assert.ThrowsAsync<ValidationException>(
        () => _textService.SendSmsMessageAsync(sms));

      // Assert.
      Assert.Equal(expectedExceptionMessage, ex.Message);
    }

    [Fact]
    public async Task SendSmsMessage_NotifyClientException_Test()
    {
      // Arrange.
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
        TemplateId = _mockTextOptions
          .Object.GetTemplateIdFor("templateId").ToString()
      };
      var exception = new NotifyClientException("Test");

      _moqClient.Setup(x => x.SendSmsAsync(It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Dictionary<string, dynamic>>(),
        It.IsAny<string>(),
        It.IsAny<string>()
        )).Throws(exception);

      _mockHelper.Setup(x => x.TextClient).Returns(_moqClient.Object);

      _textService = new TextService(
        Options.Create(_mockTextOptions.Object),
        _mockHelper.Object,
       _mockContext.Object,
       _mockDateTimeProvider.Object,
       _mockLinkIdService.Object);

      // Act.
      var ex = await Assert.ThrowsAsync<NotifyClientException>(
       () => _textService.SendSmsMessageAsync(sms));

      // Assert.
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

      _textService = new TextService(
        Options.Create(_mockTextOptions.Object),
        _mockHelper.Object,
        _context,
        _mockDateTimeProvider.Object,
        _mockLinkIdService.Object)
      {
        User = GetClaimsPrincipal()
      };
    }

    [Theory]
    [InlineData(ReferralStatus.ChatBotCall1, true, 1)]
    [InlineData(ReferralStatus.ChatBotCall1, false, 0)]
    [InlineData(ReferralStatus.ChatBotTransfer, true, 1)]
    [InlineData(ReferralStatus.ChatBotTransfer, false, 0)]
    [InlineData(ReferralStatus.RmcDelayed, true, 1)]
    [InlineData(ReferralStatus.RmcDelayed, false, 0)]
    public async Task InitialContactBeyondMaximumDurationForTextMessage3ShouldNotBePrepared(
      ReferralStatus status,
      bool initialText,
      int expectedTextMessages)
    {
      // Arrange.
      ResetConfigValues();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateToDelayUntil: DateTimeOffset.UtcNow.AddDays(1),
        status: status);

      if (initialText)
      {
        referral.TextMessages = [RandomEntityCreator.CreateRandomTextMessage(
          referralStatus: ReferralStatus.TextMessage1.ToString(),
          sent: DateTimeOffset.UtcNow.AddDays(-MaxDaysSinceInitialContactToSendTextMessage3 - 7))];
      }

      referral.Calls = [RandomEntityCreator.CreateRandomChatBotCall(
        sent: DateTimeOffset.UtcNow.AddDays(-MaxDaysSinceInitialContactToSendTextMessage3 - 1))];

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      // Act & Assert
      await ShouldNotBePrepared_ActAssert(expectedTextMessages, 0, referral, 1);
    }

    [Fact]
    public async Task LinkIdServiceAlreadyRunningThrowsProcessAlreadyRunningException()
    {
      // Arrange.
      ResetConfigValues();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      _mockLinkIdService.Setup(x => x.GetUnusedLinkIdBatchAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ThrowsAsync(new ProcessAlreadyRunningException());

      int minutesToDelay = 1;
      _mockTextOptions.Object.PrepareMessageDelayMinutes = minutesToDelay;

      DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
      _mockDateTimeProvider.Setup(x => x.UtcNow).Returns(dateTimeOffset);

      string expectedMessageTemplate = $"*{dateTimeOffset.AddMinutes(minutesToDelay)}*";

      // Act.
      Func<Task<int>> result = () => _textService.PrepareMessagesToSend();

      // Assert.
      await result.Should().ThrowAsync<ProcessAlreadyRunningException>()
        .WithMessage(expectedMessageTemplate);
    }

    [Fact]
    public async Task RmcDelayedBeforeDateToDelayUntilShouldNotBePrepared()
    {
      // Arrange.
      ResetConfigValues();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateToDelayUntil: DateTimeOffset.UtcNow.AddDays(1),
        status: ReferralStatus.RmcDelayed);

      referral.TextMessages = [RandomEntityCreator.CreateRandomTextMessage(
        referralStatus: ReferralStatus.TextMessage1.ToString(),
        sent: DateTimeOffset.UtcNow.AddHours(-Constants.HOURS_BEFORE_TEXTMESSAGE3 - 1))];

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      // Act & Assert
      await ShouldNotBePrepared_ActAssert(1, 0, referral);

    }

    [Theory]
    [InlineData(
      ReferralStatus.New,
      ReferralStatus.TextMessage1,
      false,
      false)]
    [InlineData(
      ReferralStatus.CancelledDuplicateTextMessage,
      ReferralStatus.CancelledDuplicateTextMessage,
      true,
      false)]
    [InlineData(
      ReferralStatus.ChatBotCall1,
      ReferralStatus.TextMessage3,
      true,
      true)]
    [InlineData(
      ReferralStatus.ProviderRejectedTextMessage,
      ReferralStatus.ProviderRejectedTextMessage,
      true,
      false)]
    [InlineData(
      ReferralStatus.ProviderTerminatedTextMessage,
      ReferralStatus.ProviderTerminatedTextMessage,
      true,
      false)]
    [InlineData(
      ReferralStatus.TextMessage1,
      ReferralStatus.TextMessage2,
      true,
      false)]
    [InlineData(
      ReferralStatus.ChatBotTransfer,
      ReferralStatus.TextMessage3,
      true,
      true)]
    [InlineData(
      ReferralStatus.RmcDelayed,
      ReferralStatus.TextMessage3,
      true,
      true)]
    public async Task ShouldBePrepared_Update(
      ReferralStatus initialStatus,
      ReferralStatus expectedStatus,
      bool createTextMessages,
      bool createCalls)
    {
      // Arrange.
      ResetConfigValues();
      int expectedResponse = 1;
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateToDelayUntil: DateTimeOffset.UtcNow.AddDays(-1),
        status: initialStatus);

      if (createTextMessages)
      {
        referral.TextMessages = [RandomEntityCreator.CreateRandomTextMessage(
          referralStatus: ReferralStatus.TextMessage1.ToString(),
          sent: DateTimeOffset.UtcNow.AddHours(-Constants.HOURS_BEFORE_TEXTMESSAGE3 - 1))];
      }

      if (createCalls)
      {
        referral.Calls = [
          RandomEntityCreator.CreateRandomChatBotCall(
            sent: DateTimeOffset.UtcNow.AddHours(-Constants.HOURS_BEFORE_TEXTMESSAGE3 - 1))
        ];
      }

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      TextMessage expectedTextMessage = new()
      {
        // ServiceUserLinkId - Not NULL
        // Id - Not Empty
        IsActive = true,
        // ModifiedAt - After referral.ModifiedAt
        ModifiedByUserId = Guid.Parse(TEST_USER_ID),
        Number = referral.Mobile,
        Outcome = null,
        Received = null,
        // Referral - Not NULL
        ReferralId = referral.Id,
        Sent = default,
        ReferralStatus = expectedStatus.ToString()
      };

      _mockLinkIdService.Setup(x => x.GetUnusedLinkIdBatchAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync([LinkIdService.GenerateDummyId(12)]);

      // Act.
      int response = await _textService.PrepareMessagesToSend();

      // Assert.
      AssertShouldBePrepared(
        expectedStatus,
        referral,
        expectedResponse,
        expectedTextMessage,
        response);
    }

    [Theory]
    [InlineData(
      ReferralStatus.New,
      ReferralStatus.TextMessage1,
      false,
      false)]
    [InlineData(
      ReferralStatus.CancelledDuplicateTextMessage,
      ReferralStatus.CancelledDuplicateTextMessage,
      true,
      false)]
    [InlineData(
      ReferralStatus.ChatBotCall1,
      ReferralStatus.TextMessage3,
      true,
      true)]
    [InlineData(
      ReferralStatus.ProviderRejectedTextMessage,
      ReferralStatus.ProviderRejectedTextMessage,
      true,
      false)]
    [InlineData(
      ReferralStatus.ProviderTerminatedTextMessage,
      ReferralStatus.ProviderTerminatedTextMessage,
      true,
      false)]
    [InlineData(
      ReferralStatus.TextMessage1,
      ReferralStatus.TextMessage2,
      true,
      false)]
    [InlineData(
      ReferralStatus.ChatBotTransfer,
      ReferralStatus.TextMessage3,
      true,
      true)]
    [InlineData(
      ReferralStatus.RmcDelayed,
      ReferralStatus.TextMessage3,
      true,
      true)]
    public async Task RepeatPrepare_ShouldNotCreateDuplicates(
      ReferralStatus initialStatus,
      ReferralStatus expectedStatus,
      bool createTextMessages,
      bool createCalls)
    {
      // Arrange.
      ResetConfigValues();
      int expectedFirstResponse = 1;
      int expectedSecondResponse = 0;
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        dateToDelayUntil: DateTimeOffset.UtcNow.AddDays(-1),
        status: initialStatus);

      if (createTextMessages)
      {
        referral.TextMessages = [RandomEntityCreator.CreateRandomTextMessage(
          referralStatus: ReferralStatus.TextMessage1.ToString(),
          sent: DateTimeOffset.UtcNow.AddHours(-Constants.HOURS_BEFORE_TEXTMESSAGE3 - 1))];
      }

      if (createCalls)
      {
        referral.Calls = [
          RandomEntityCreator.CreateRandomChatBotCall(
          sent: DateTimeOffset.UtcNow.AddHours(-Constants.HOURS_BEFORE_TEXTMESSAGE3 - 1))
        ];
      }
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      TextMessage expectedTextMessage = new()
      {
        // ServiceUserLinkId - Not NULL
        // Id - Not Empty
        IsActive = true,
        // ModifiedAt - After referral.ModifiedAt
        ModifiedByUserId = Guid.Parse(TEST_USER_ID),
        Number = referral.Mobile,
        Outcome = null,
        Received = null,
        // Referral - Not NULL
        ReferralId = referral.Id,
        Sent = default,
        ReferralStatus = expectedStatus.ToString()
      };

      _mockTextOptions.Object.PrepareMessageDelayMinutes = 0;

      _mockLinkIdService.Setup(x => x.GetUnusedLinkIdBatchAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync([LinkIdService.GenerateDummyId(12)]);

      // Act.
      int firstResponse = await _textService.PrepareMessagesToSend();
      int secondResponse = await _textService.PrepareMessagesToSend();

      // Assert.
      AssertShouldBePrepared(
        expectedStatus,
        referral,
        expectedFirstResponse,
        expectedTextMessage,
        firstResponse);

      secondResponse.Should().Be(expectedSecondResponse);
      _context.TextMessages
        .Where(t => t.ReferralId == referral.Id)
        .Where(t => t.ReferralStatus == expectedStatus.ToString())
        .Should()
        .HaveCount(expectedFirstResponse);
    }

    [Fact]
    public async Task ProcessAlreadyRunning_ThrowsException()
    {
      // Arrange.
      int delayMinutes = 5;
      int lastRunDifferenceMinutes = 1;
      DateTimeOffset lastRun = new(2023, 11, 7, 12, 0, 0, TimeSpan.Zero);
      DateTimeOffset currentTime = lastRun.AddMinutes(lastRunDifferenceMinutes);

      ConfigurationValue lastRunConfig = _context.ConfigurationValues
        .SingleOrDefault(c => c.Id == Constants.MessageServiceConstants.CONFIG_TEXT_TIME);
      if (lastRunConfig != null)
      {
        lastRunConfig.Value = lastRun.ToString();
      }
      else
      {
        lastRunConfig = new()
        {
          Id = Constants.MessageServiceConstants.CONFIG_TEXT_TIME,
          Value = lastRun.ToString()
        };
        _context.ConfigurationValues.Add(lastRunConfig);
      }

      _context.SaveChanges();

      _mockDateTimeProvider.Setup(dtp => dtp.UtcNow).Returns(currentTime);
      _mockTextOptions.Object.PrepareMessageDelayMinutes = delayMinutes;

      try
      {
        // Act.
        await _textService.PrepareMessagesToSend();
      }
      catch (Exception ex)
      {
        // Assert.
        ex.Should().BeOfType<ProcessAlreadyRunningException>()
          .Subject.Message.Should().Be(
            $"The prepare text messages process was run {lastRunDifferenceMinutes} " +
            $"minutes ago and can't be re-run until {lastRun.AddMinutes(delayMinutes)}.");
      }
    }


    private void ResetConfigValues()
    {
      ConfigurationValue configurationValue = _context.ConfigurationValues
        .SingleOrDefault(t => t.Id == Constants.MessageServiceConstants.CONFIG_TEXT_TIME);

      if (configurationValue != null)
      {
        _context.ConfigurationValues.Remove(configurationValue);
      }
      _context.SaveChanges();
    }

    [Fact]
    public async Task SingleReferral_ShouldBePrepared_Update()
    {
      // Arrange.
      ResetConfigValues();
      int expectedResponse = 1;
      ReferralStatus expectedStatus = ReferralStatus.TextMessage1;

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.New);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      TextMessage expectedTextMessage = new()
      {
        // ServiceUserLinkId - Not NULL
        // Id - Not Empty
        IsActive = true,
        // ModifiedAt - After referral.ModifiedAt
        ModifiedByUserId = Guid.Parse(TEST_USER_ID),
        Number = referral.Mobile,
        Outcome = null,
        Received = null,
        // Referral - Not NULL
        ReferralId = referral.Id,
        Sent = default,
        ReferralStatus = expectedStatus.ToString()
      };

      _mockLinkIdService.Setup(x => x.GetUnusedLinkIdBatchAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(new List<string>() { LinkIdService.GenerateDummyId(12) });

      // Act.
      int response = await _textService.PrepareMessagesToSend(referral.Id);

      // Assert.
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
        .Excluding(r => r.Calls)
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
        .Excluding(t => t.ServiceUserLinkId)
        .Excluding(t => t.Id)
        .Excluding(t => t.ModifiedAt)
        .Excluding(t => t.Referral));
      textMessage.ServiceUserLinkId.Should().NotBeNull();
      textMessage.Id.Should().NotBeEmpty();
      textMessage.ModifiedAt.Should().BeAfter(referral.ModifiedAt);
      textMessage.Referral.Should().NotBeNull();
    }

    public static TheoryData<ReferralStatus, int> ShouldNotBePreparedData()
    {
      TheoryData<ReferralStatus, int> data = [];
      IEnumerable<ReferralStatus> allStatuses =
        (IEnumerable<ReferralStatus>)Enum.GetValues(typeof(ReferralStatus));

      IEnumerable<ReferralStatus> excludedStatuses =
      [
        ReferralStatus.ChatBotCall1,
        ReferralStatus.ChatBotTransfer,
        ReferralStatus.New,
        ReferralStatus.RmcDelayed
      ];

      excludedStatuses =
        excludedStatuses.Union(allStatuses.Where(s => s.ToString().Contains("TextMessage")));

      foreach (ReferralStatus referralStatus in allStatuses.Except(excludedStatuses))
      {
        data.Add(referralStatus, 0);
      }

      return data;
    }

    [Theory]
    [MemberData(nameof(ShouldNotBePreparedData))]
    public async Task ShouldNotBePrepared_Update(
      ReferralStatus status, 
      int expectedTextMessagesCount)
    {
      // Arrange.
      ResetConfigValues();
      int expectedResponse = 0;

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        isActive: true,
        status: status);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      await ShouldNotBePrepared_ActAssert(expectedTextMessagesCount, expectedResponse, referral);
    }

    [Theory]
    [InlineData(ReferralStatus.TextMessage1)]
    [InlineData(ReferralStatus.TextMessage2)]
    public async Task ShouldNotBePrepared_BecausePreviousTextMessageSentWithinStageWindow(
      ReferralStatus referralStatus)
    {
      // Arrange.
      ResetConfigValues();
      int expectedResponse = 0;
      int expectedTextMessagesCount = 1;

      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: referralStatus,
        textMessages: [
          RandomEntityCreator.CreateRandomTextMessage(
            isActive: true,
            sent: DateTimeOffset.Now.AddHours(- Constants.HOURS_BEFORE_NEXT_STAGE + 24).Date)
        ]);

      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();
      _context.ChangeTracker.Clear();

      await ShouldNotBePrepared_ActAssert(expectedTextMessagesCount, expectedResponse, referral);
    }

    private async Task ShouldNotBePrepared_ActAssert(
      int expectedTextMessagesCount, 
      int expectedResponse, 
      Referral referral,
      int expectedCallsCount = 0)
    {
      // Act.
      int response = await _textService.PrepareMessagesToSend();

      // Assert.
      response.Should().Be(expectedResponse);

      Referral updatedReferral = _context.Referrals
        .Include(r => r.Calls)
        .Include(r => r.TextMessages)
        .Single(r => r.Id == referral.Id);

      updatedReferral.Should().BeEquivalentTo(referral, o => o
        .Excluding(r => r.Audits)
        .Excluding(r => r.Calls)
        .Excluding(r => r.TextMessages));

      updatedReferral.Audits.Should().BeNull();
      updatedReferral.Calls.Should().HaveCount(expectedCallsCount);
      updatedReferral.TextMessages.Should().HaveCount(expectedTextMessagesCount);
    }

    [Fact]
    public async Task SingleReferralDoesNotExist_ShouldNotBePrepared()
    {
      // Arrange.
      ResetConfigValues();
      int expectedResponse = 0;

      // Act.
      int response = await _textService.PrepareMessagesToSend(Guid.NewGuid());

      // Assert.
      response.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task NonInitialTextMessageReusesServiceUserLinkId()
    {
      // Arrange.
      ResetConfigValues();
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.TextMessage1);
      DateTimeOffset sent = DateTimeOffset.Now.AddHours(-49);
      TextMessage textMessage1 = new()
      {
        ServiceUserLinkId = LinkIdService.GenerateDummyId(),
        Id = Guid.NewGuid(),
        IsActive = true,
        Number = referral.Mobile,
        ReferralStatus = ReferralStatus.TextMessage1.ToString(),
        ReferralId = referral.Id,
        Sent = sent
      };

      referral.TextMessages = new() { textMessage1 };
      _context.Referrals.Add(referral);
      await _context.SaveChangesAsync();

      // Act.
      int result = await _textService.PrepareMessagesToSend();

      // Assert.
      result.Should().Be(1);
      _context.TextMessages
        .Should().HaveCount(2)
        .And.Subject
        .Where(t => t.ReferralId == referral.Id)
        .Where(t => t.ReferralStatus == ReferralStatus.TextMessage2.ToString())
        .SingleOrDefault()
        .Should().NotBeNull()
        .And.BeOfType<TextMessage>()
        .Subject.ServiceUserLinkId.Should().Be(textMessage1.ServiceUserLinkId);
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
        () => _textService.ReferralMobileNumberInvalidAsync(null));
    }

    [Fact]
    public async Task ReferralMobileNumberInvalid_InvalidStatus_Test()
    {
      // Arrange.
      StatusType expectedStatus = StatusType.Invalid;
      string expectedError = "TextMessage Id is not a Guid.";
      CallbackRequest request = new CallbackRequest
      {
        Id = "ZZXZZ"
      };

      _textService = new TextService(
        Options.Create(_mockTextOptions.Object),
        _mockHelper.Object,
       _mockContext.Object,
       _mockDateTimeProvider.Object,
       _mockLinkIdService.Object);

      // Act.
      var response =
        await _textService.ReferralMobileNumberInvalidAsync(request);

      // Assert.
      Assert.Equal(expectedStatus, response.ResponseStatus);
      Assert.Equal(expectedError, response.Errors[0].ToString());
    }

    [Fact]
    public async Task ReferralMobileNumberInvalid__UpdateSuccess_Test()
    {
      // Arrange.
      Guid referralId = Guid.NewGuid();
      string expectedStatus = Enums.ReferralStatus.TextMessage2.ToString();
      bool expectedIsMobileValid = false;

      string reference = SetLocalData(referralId);
      CallbackRequest request = new CallbackRequest
      {
        Id = referralId.ToString(),
        Reference = reference,
      };

      _textService = new TextService(
        Options.Create(_mockTextOptions.Object),
        _mockHelper.Object,
        _context,
        _mockDateTimeProvider.Object,
        _mockLinkIdService.Object)
      {
        User = GetClaimsPrincipal()
      };

      // Act.
      var response =
        await _textService.ReferralMobileNumberInvalidAsync(request);

      var updatedReferral = _context.Referrals.Single(r => r.Id == referralId);

      // Assert.
      updatedReferral.IsMobileValid.Should().Be(expectedIsMobileValid);
      updatedReferral.Status.Should().Be(expectedStatus);
    }

    private string SetLocalData(Guid refId)
    {
      Entities.Referral newReferral = _serviceFixture.Mapper.
        Map<Entities.Referral>(ServiceFixture.ValidReferralEntity);
      newReferral.Id = refId;
      _context.Referrals.Add(newReferral);

      TextMessage tm = RandomEntityCreator.CreateRandomTextMessage(
        isActive: true,
        sent: DateTime.Now.AddDays(-3),
        outcome: "sent",
        referralId: newReferral.Id
        );
      _context.TextMessages.Add(tm);
      _context.SaveChanges();
      return tm.Id.ToString();
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
      string emailReplyToId = null,
      string oneClickUnsubscribeURL = null)
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
  public class TextServiceIntegrationTest : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly Mock<ILinkIdService> _mockLinkIdService = new();
    private readonly TextService _classToTest;
    private readonly TextOptions _options = new TextOptions()
    {
      SmsTemplates = new List<SmsTemplate>
      {
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_FAILEDTOCONTACT_SMS),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_GENERAL_FIRST),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_GENERAL_SECOND),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_GP_FIRST),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_GP_SECOND),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_MSK_FIRST),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_MSK_SECOND),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_NONGP_DECLINED),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_NONGP_REJECTED),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_NONGP_TERMINATED),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_PHARMACY_FIRST),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_PHARMACY_SECOND),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_SELF_CANCELLEDDUPLICATE),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_SELF_FIRST),
        new SmsTemplate(Guid.NewGuid(), TEMPLATE_SELF_SECOND)
      }
    };

    private readonly DateTimeProvider _dateTimeProvider = new();
    private readonly Mock<IOptions<TextOptions>> _mockOptions =
      new Mock<IOptions<TextOptions>>();

    private readonly Mock<ITextNotificationHelper> _mockHelper =
      new Mock<ITextNotificationHelper>();
    public TextServiceIntegrationTest(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _mockOptions.Setup(t => t.Value).Returns(_options);
      _context = new DatabaseContext(_serviceFixture.Options);
      _classToTest = new TextService(
        _mockOptions.Object,
        _mockHelper.Object,
        _context,
        _dateTimeProvider,
        _mockLinkIdService.Object)
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
      // Arrange.
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
public class TextServiceIntegrationTests : ServiceTestsBase
{
  private readonly DatabaseContext _context;
  private readonly Mock<ILinkIdService> _mockLinkIdService = new();
  private readonly Mock<TextNotificationHelper> _mockHelper;
  protected readonly Mock<IMapper> _mockMapper = new();
  private readonly TextService _classToTest;
  protected readonly DateTimeProvider _dateTimeProvider = new();

  public TextServiceIntegrationTests(ServiceFixture serviceFixture) : base(
    serviceFixture)
  {
    _mockHelper =
      new Mock<TextNotificationHelper>(
        TestConfiguration.CreateTextOptions());
    _context = new DatabaseContext(_serviceFixture.Options);

    _classToTest = new TextService(
      TestConfiguration.CreateTextOptions(),
      _mockHelper.Object,
      _context,
      _dateTimeProvider,
      _mockLinkIdService.Object)
    {
      User = GetClaimsPrincipal()
    };
  }

  [Fact()]
  public async Task CallBackAsyncTest()
  {
    // Arrange.
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
    // Act.
    Assert.Null(referral.IsMobileValid);
    CallbackResponse result =
      await _classToTest.CallBackAsync(request);
    // Assert.
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

  private static readonly Mock<ILinkIdService> _mockLinkIdService = new();

  private static readonly TextOptions _options =
    TestConfiguration.CreateTextOptions().Value;

  public TextServiceTestBase()
  {
    _mockContext.Setup(t => t.SaveChangesAsync(CancellationToken.None))
      .Verifiable();
  }

  public class TextServiceUnitTest : TextService
  {
    public TextServiceUnitTest()
      : base(
        TestConfiguration.CreateTextOptions(),
        null,
        _mockContext.Object,
        new DateTimeProvider(),
        _mockLinkIdService.Object)
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

      [Theory]
      [InlineData(ReferralSource.GpReferral, ReferralStatus.ProviderCompleted)]
      [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
      [InlineData(ReferralSource.Pharmacy, ReferralStatus.Complete)]
      [InlineData(ReferralSource.GeneralReferral, ReferralStatus.Complete)]
      [InlineData(ReferralSource.Msk, ReferralStatus.Complete)]
      [InlineData(ReferralSource.ElectiveCare, ReferralStatus.Complete)]
      public async Task ValidProviderRejectedTextMessage(
        ReferralSource referralSource,
        ReferralStatus expectedStatus)
      {
        // Arrange.

        Guid textMessageId = Guid.NewGuid();
        _mockSmsMessage.Setup(t => t.LinkedTextMessage)
          .Returns(textMessageId);
        _mockTextMessage.Object.Id = textMessageId;
        _mockReferral.Object.NumberOfContacts = 0;
        _mockReferral.Object.MethodOfContact = 0;
        _mockReferral.Object.Status =
          ReferralStatus.ProviderRejectedTextMessage.ToString();
        _mockReferral.Object.StatusReason = "test";
        _mockReferral.Object.ReferralSource = referralSource.ToString();
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
            .Be(expectedStatus.ToString());
        }
        catch (Exception ex)
        {
          Assert.Fail($"Unexpected exception: {ex.Message} ");
        }

      }

      [Fact]
      public async Task Valid_FailedToContact()
      {
        // Arrange.

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
            .Be(ReferralStatus.Complete.ToString());
        }
        catch (Exception ex)
        {
          Assert.Fail($"Unexpected exception: {ex.Message} ");
        }

      }

      [Fact]
      public async Task Valid_CancelledDuplicate()
      {
        // Arrange.
        ReferralStatus expectedStatus = ReferralStatus.Complete;
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
          Assert.Fail($"Unexpected exception: {ex.Message} ");
        }

      }

      [Theory]
      [InlineData(ReferralSource.GpReferral, ReferralStatus.ProviderCompleted)]
      [InlineData(ReferralSource.SelfReferral, ReferralStatus.Complete)]
      [InlineData(ReferralSource.Pharmacy, ReferralStatus.Complete)]
      [InlineData(ReferralSource.GeneralReferral, ReferralStatus.Complete)]
      [InlineData(ReferralSource.Msk, ReferralStatus.Complete)]
      [InlineData(ReferralSource.ElectiveCare, ReferralStatus.Complete)]
      public async Task ValidProviderDeclinedTextMessageProviderCompleted(
        ReferralSource referralSource,
        ReferralStatus expectedStatus)
      {
        // Arrange.
        Guid textMessageId = Guid.NewGuid();
        _mockSmsMessage.Setup(t => t.LinkedTextMessage).Returns(textMessageId);
        _mockTextMessage.Object.Id = textMessageId;
        _mockReferral.Object.NumberOfContacts = 0;
        _mockReferral.Object.MethodOfContact = 0;
        _mockReferral.Object.Status = ReferralStatus.ProviderDeclinedTextMessage.ToString();
        _mockReferral.Object.StatusReason = "test";
        _mockReferral.Object.ReferralSource = referralSource.ToString();
        _mockReferral.Setup(t => t.TextMessages).Returns(
          new EditableList<TextMessage> { _mockTextMessage.Object });
        int expectedNumberOfContacts = 1;
        int expectedMethofOfContact = (int)MethodOfContact.TextMessage;
        string expectedOutcome = "SENT";
        
        // Act.
        await UpdateMessageRequestAsync(_mockSmsMessage.Object);

        // Assert.
        _mockReferral.Object.NumberOfContacts.Should().Be(expectedNumberOfContacts);
        _mockReferral.Object.MethodOfContact.Should().Be(expectedMethofOfContact);
        _mockTextMessage.Object.Outcome.Should().Be(expectedOutcome);
        _mockReferral.Object.Status.Should().Be(expectedStatus.ToString());
      }

      [Theory]
      [InlineData(ReferralSource.GpReferral, ReferralStatus.ProviderTerminated)]
      [InlineData(ReferralSource.SelfReferral, ReferralStatus.ProviderTerminated)]
      [InlineData(ReferralSource.Pharmacy, ReferralStatus.ProviderTerminated)]
      [InlineData(ReferralSource.GeneralReferral, ReferralStatus.ProviderTerminated)]
      [InlineData(ReferralSource.Msk, ReferralStatus.ProviderTerminated)]
      [InlineData(ReferralSource.ElectiveCare, ReferralStatus.ProviderTerminated)]
      public async Task ValidProviderTerminated(
        ReferralSource referralSource,
        ReferralStatus expectedStatus)
      {
        // Arrange.

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
          Assert.Fail($"Unexpected exception: {ex.Message} ");
        }
      }

      protected internal override async Task<Referral> GetReferralWithTextMessage(Guid id)
      {
        return await Task.FromResult(_mockReferral.Object);
      }
    }

    public class GetReferralTemplateIdTests : TextServiceUnitTest
    {
      [Theory]
      [InlineData(ReferralStatus.TextMessage1, TEMPLATE_DYNAMIC_SOURCE_REFERRAL_FIRST)]
      [InlineData(ReferralStatus.TextMessage2, TEMPLATE_DYNAMIC_SOURCE_REFERRAL_SECOND)]
      [InlineData(ReferralStatus.TextMessage3, TEMPLATE_DYNAMIC_SOURCE_REFERRAL_THIRD)]
      [InlineData(ReferralStatus.CancelledDuplicateTextMessage, TEMPLATE_SELF_CANCELLEDDUPLICATE)]
      [InlineData(ReferralStatus.FailedToContactTextMessage, TEMPLATE_FAILEDTOCONTACT_SMS)]
      [InlineData(ReferralStatus.ProviderDeclinedTextMessage, TEMPLATE_NONGP_DECLINED)]
      [InlineData(ReferralStatus.ProviderRejectedTextMessage, TEMPLATE_NONGP_REJECTED)]
      [InlineData(ReferralStatus.ProviderTerminatedTextMessage, TEMPLATE_NONGP_TERMINATED)]
      public void GetReferralId_ReturnsDynamicTemplateId(ReferralStatus status, string template)
      {
        // Arrange.
        SmsTemplate expected = _options.SmsTemplates.Single(t => t.Name == template);
        Referral referral = RandomEntityCreator.CreateRandomReferral(status: status);

        // Act.
        string result = GetReferralTemplateId(referral);

        // Assert.
        result.Should().NotBeNullOrWhiteSpace().And.Be(expected.Id.ToString());
      }
    }
  }
}
