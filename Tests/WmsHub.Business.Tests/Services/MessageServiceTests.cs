using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.MessageService;
using WmsHub.Business.Services;
using WmsHub.Common.Helpers;
using Xunit;
using static WmsHub.Common.Helpers.Constants.MessageServiceConstants;
using static FluentAssertions.FluentActions;
using static WmsHub.Business.Enums.ReferralStatus;
using Newtonsoft.Json;
using WmsHub.Common.Extensions;
using WmsHub.Business.Tests.Helpers;
using WmsHub.Business.Extensions;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class MessageServiceTests : ServiceTestsBase, IDisposable
{
  private readonly DatabaseContext _context;
  protected readonly IConfiguration _configuration;
  protected readonly Mock<ILogger> _loggerMock = new();
  private readonly Mock<INotificationService> _mockNotificationService = new();
  private Mock<IOptions<MessageOptions>> _mockOptions = new();
  private Guid _templateId = Guid.NewGuid();

  public void Dispose()
  {
    _mockOptions = null;
    _context.Dispose();
  }

  public MessageServiceTests(ServiceFixture serviceFixture)
    : base(serviceFixture) 
  {
    _context = new DatabaseContext(_serviceFixture.Options);
    Dictionary<string, string> inMemorySettings = new()
    {
      { "NotificationApiKey", "NotificationApiKey" },
      { "NotificationApiUrl", "NotificationApiUrl" },
      { "NotificationSenderId", "NotificationSenderId" }
    };

    _configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(inMemorySettings)
    .Build();

    _loggerMock = new Mock<ILogger>();

    List<MessageTemplate> templates = new List<MessageTemplate>
      {
        new MessageTemplate()
        {
          Id = _templateId,
          Name = "FailedToContactEmailMessage",
          ExpectedPersonalisationCsv = "givenName",
          MessageTypeValue = (int)MessageType.Email
        }
      };
    MessageOptions options = new()
    {
      TemplateJson = JsonConvert.SerializeObject(templates),
      ReplyToId = Guid.NewGuid().ToString(),
      SenderId = Guid.NewGuid().ToString(),
    };
    _mockOptions.Setup(t => t.Value).Returns(options);

    Cleanup();
  }

  public class AddReferralToMessageQueueTests : MessageServiceTests
  {
    private readonly IMessageService _service;
    public AddReferralToMessageQueueTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _service = new MessageService(
        _context,
        _loggerMock.Object,
        _mockOptions.Object,
        _mockNotificationService.Object
        )
      {
        User = GetClaimsPrincipal()
      };
    }

    [Fact]
    public void ReferralIsNull_Throw_ArgumentNullException()
    {
      // Arrange.
      string expectedError = 
        "Value cannot be null. (Parameter 'Referral must be provided.')";

      // Act.
      try
      {
        _service.AddReferralToMessageQueue(null, MessageType.Email);
        throw new UnitTestExpectedExceptionNotThrownException(
          $"{nameof(ArgumentNullException)} expected but no exception was" +
          $" thrown.");
      }
      catch(Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType<ArgumentNullException>();
          ex.Message.Should().Be(expectedError);
        }
      }
    }

    [Fact]
    public void MessageType_ItemIsNull_ArgumentNullException()
    {
      // Arrange.
      string expectedError = "Value cannot be null. (Parameter 'Referral " +
        "must be provided.')";
      QueueItem queueItem  = QueueItemCreator(
        status: ReferralStatus.FailedToContactEmailMessage,
        source: ReferralSource.SelfReferral,
        mobile: "not set");

      // Act.
      try
      {
        _service.AddReferralToMessageQueue(null, MessageType.Email);
        throw new UnitTestExpectedExceptionNotThrownException(
          $"{nameof(ArgumentNullException)} expected but no exception was" +
          $" thrown.");
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType<ArgumentNullException>();
          ex.Message.Should().Be(expectedError);
        }
      }
    }

    [Fact]
    public void ValidationErrors_Throws_ArgumentNullException()
    {
      // Arrange.
      string expectedError = "Value cannot be null. (Parameter " +
        "'Referral must be provided.')";
      QueueItem queueItem = QueueItemCreator(
        status: ReferralStatus.FailedToContactEmailMessage,
        source: ReferralSource.Pharmacy,
        mobile: "not set",
        email: "Not Set");

      // Act.
      try
      {
        _service.AddReferralToMessageQueue(null, MessageType.Email);
        throw new UnitTestExpectedExceptionNotThrownException(
          $"{nameof(ArgumentNullException)} expected but no exception was" +
          $" thrown.");
      }
      catch (Exception ex)
      {
        // Assert.
        using (new AssertionScope())
        {
          ex.Should().BeOfType<ArgumentNullException>();
          ex.Message.Should().Be(expectedError);
        }
      }
    }

    [Fact]
    public async Task Valid_MessageQueueItemCreated()
    {
      // Arrange.
      QueueItem queueItem = QueueItemCreator(
        status: ReferralStatus.FailedToContactEmailMessage,
        source: ReferralSource.Pharmacy,
        mobile: "not set");

      // Act.
      _service.AddReferralToMessageQueue(queueItem, MessageType.Email);

      Entities.MessageQueue message = await _context.MessagesQueue
        .SingleOrDefaultAsync(t => t.ReferralId == queueItem.Id);
      // Assert.
      using(new AssertionScope())
      {
        message.Should().NotBeNull();
        message.SentDate.Should().BeNull();
        message.SendResult.Should().BeNullOrWhiteSpace();
        message.IsActive.Should().BeTrue();
      }
    }

  }

  public class SignatureTests : MessageServiceTests
  {
    public SignatureTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
    }

    [Fact]
    public void TextClassSignature_ContextNullException()
    {
      // Arrange.
      string expectedMessage =
        "Value cannot be null. (Parameter 'context is null.')";

      // Act.
      Invoking(() => new MessageService(null, null, null, null))
        .Should()
        .Throw<ArgumentNullException>()
        .WithMessage(expectedMessage);
    }

    [Fact]
    public void TextClassSignature_LoggerNullException()
    {
      // Arrange.
      string expectedMessage =
        "Value cannot be null. (Parameter 'logger is null.')";

      // Act.
      Invoking(() => new MessageService(_context, null, null, null))
        .Should()
        .Throw<ArgumentNullException>()
        .WithMessage(expectedMessage);
    }

    [Fact]
    public void TextClassSignature_NotificationServiceNullException()
    {
      // Arrange.
      string expectedMessage =
        "Value cannot be null. (Parameter 'notificationService is null.')";

      // Act.
      Invoking(() => new MessageService(
        _context,
        _loggerMock.Object,
        null,
        null))
        .Should()
        .Throw<ArgumentNullException>()
        .WithMessage(expectedMessage);

    }

    [Fact]
    public void TextClassSignature_IOptionsNullException()
    {
      // Arrange.
      string expectedMessage =
        "Value cannot be null. (Parameter 'IOptions is null.')";

      // Act.
      Invoking(() => new MessageService(
        _context,
        _loggerMock.Object,
        null,
        _mockNotificationService.Object))
          .Should()
          .Throw<ArgumentNullException>()
          .WithMessage(expectedMessage);

    }

    [Fact]
    public void TextClassSignature_MessageOptionsNullException()
    {
      // Arrange.
      string expectedMessage =
        "Value cannot be null. (Parameter 'MessageOptions is null.')";

      _mockOptions.Setup(t => t.Value).Returns((MessageOptions)null);

      // Act.
      Invoking(() => new MessageService(
        _context,
        _loggerMock.Object,
        _mockOptions.Object,
        _mockNotificationService.Object))
          .Should()
          .Throw<ArgumentNullException>()
          .WithMessage(expectedMessage);

    }
  }

  public class PrepareFailedToContactAsyncTests : MessageServiceTests
  {
    private readonly IMessageService _service;
    public PrepareFailedToContactAsyncTests(ServiceFixture serviceFixture) 
      : base(serviceFixture)
    {
      _service = new MessageService(
        _context,
        _loggerMock.Object,
        _mockOptions.Object,
        _mockNotificationService.Object);
    }

    [Fact]
    public async Task NoReferralsWithFailedToContact()
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator();

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using(new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(0);
        List<Entities.Referral> found = await _context.Referrals
          .Where(x => x.Status == referral.Status)
          .ToListAsync();
        found.Count.Should().Be(1);
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task SelfReferralsWithFailedToContact()
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator(
        status: FailedToContact);

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        Entities.Referral found = await _context.Referrals
          .SingleOrDefaultAsync(x => x.Id.ToString() == result[0]);
        found.Status.Should().Be(FailedToContactTextMessage.ToString());
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task SelfReferralsWithFailedToContactNoMobile()
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator(
        status:FailedToContact,
        mobile: "test");

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        Entities.Referral found = await _context.Referrals
          .SingleOrDefaultAsync(x => x.Id.ToString() == result[0]);
        found.Status.Should().Be(FailedToContactEmailMessage.ToString());
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task SelfReferralsWithFailedToContactNoEmail()
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator(
        status: FailedToContact,
        mobile: "test",
        email: "not valid");

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        Entities.Referral found = await _context.Referrals
          .SingleOrDefaultAsync(x => x.Id.ToString() == result[0]);
        found.Status.Should().Be("Exception");
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task PharmacyReferralsWithFailedToContact()
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator(
        status: FailedToContact,
        source: ReferralSource.Pharmacy);

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        Entities.Referral found = await _context.Referrals
          .SingleOrDefaultAsync(x => x.Id.ToString() == result[0]);
        found.Status.Should().Be(FailedToContactEmailMessage.ToString());
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task MskReferralsWithFailedToContact()
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator(
        status: FailedToContact,
        source: ReferralSource.Msk);

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        Entities.Referral found = await _context.Referrals
          .SingleOrDefaultAsync(x => x.Id.ToString() == result[0]);
        found.Status.Should().Be(FailedToContactEmailMessage.ToString());
      }

      // Clean up.
      Cleanup();
    }

    [Theory]
    [InlineData(ReferralSource.SelfReferral, FailedToContactTextMessage)]
    [InlineData(ReferralSource.Pharmacy, FailedToContactEmailMessage)]
    [InlineData(ReferralSource.Msk, FailedToContactEmailMessage)]
    public async Task ReferralsWithFailedToContact_Valid(
      ReferralSource source,
      ReferralStatus expected)
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator(
        status: FailedToContact,
        source: source);

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        Entities.Referral found = await _context.Referrals
          .SingleOrDefaultAsync(x => x.Id.ToString() == result[0]);
        found.Status.Should().Be(expected.ToString());
      }

      // Clean up.
      Cleanup();
    }

    [Theory]
    [InlineData(ReferralSource.GeneralReferral, FailedToContact)]
    [InlineData(ReferralSource.ElectiveCare, FailedToContact)]
    [InlineData(ReferralSource.GpReferral, FailedToContact)]
    public async Task ReferralsWithFailedToContact_NotValid(
     ReferralSource source,
     ReferralStatus expected)
    {
      // Arrange.
      Entities.Referral referral = ReferralEntityCreator(
        status: FailedToContact,
        source: source);

      // Act.
      string[] result = await _service.PrepareFailedToContactAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(0);
        Entities.Referral found = await _context.Referrals
          .SingleOrDefaultAsync(x => x.Id == referral.Id);
        found.Status.Should().Be(expected.ToString());
      }

      // Clean up.
      Cleanup();
    }

  }

  public class PrepareNewReferralsToContactAsyncTest: MessageServiceTests
  {
    private readonly IMessageService _service;
    public PrepareNewReferralsToContactAsyncTest(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _service = new MessageService(
        _context,
        _loggerMock.Object,
        _mockOptions.Object,
        _mockNotificationService.Object);
    }

    [Fact]
    public async Task PrepareNewReferralsToContactAsync_ReturnsCorrectIds()
    {
      // Arrange
      Entities.Referral referral1 = ReferralEntityCreator(
          status: FailedToContact,
          source: ReferralSource.Msk);
      Entities.Referral referral2 = ReferralEntityCreator(
          status: New,
          source: ReferralSource.GpReferral);
      Entities.Referral referral3 = ReferralEntityCreator(
          status: TextMessage1,
          source: ReferralSource.SelfReferral);
      Entities.Referral referral4 = ReferralEntityCreator(
          status: TextMessage2,
          source: ReferralSource.GpReferral);
      Entities.Referral referral5 = ReferralEntityCreator(
          status: New,
          source: ReferralSource.SelfReferral);
      Entities.Referral referral6 = ReferralEntityCreator(
          status: TextMessage1,
          source: ReferralSource.GeneralReferral);
      Entities.Referral referral7 = ReferralEntityCreator(
          status: TextMessage1,
          source: ReferralSource.ElectiveCare);

      var countBefore = await _context.Referrals
        .Where(r => r.Status == TextMessage1.ToString()).CountAsync();

      // Act
      string[] result;
      using (new AssertionScope())
      {
        result = await _service.PrepareNewReferralsToContactAsync();

        // Assert
        result.Should().HaveCount(2, 
          "because only two referrals are new and have not been added to the " +
          "message queue");
        result.Should().Contain(
          new[] { referral2.Id.ToString(), referral5.Id.ToString() }, 
          "because these are the IDs of the active, new referrals that" +
          " have not been added to the message queue");

        int countAfter = await _context.Referrals
          .Where(r => r.Status == TextMessage1.ToString()).CountAsync();
        int expectedIncrease = 2;
        (countAfter - countBefore).Should().Be(
          expectedIncrease, 
          "because the status of two new referrals should be changed to " +
          "TextMessage1");
      }

      // Assert again after the scope is exited to ensure that the
      // database was not modified
      List<Entities.Referral> referralsInDb = 
        await _context.Referrals.ToListAsync();
      referralsInDb.Should().HaveCount(7, 
        "because no new referrals should have been added to the database");

      // Clean up.
      Cleanup();
    }
  }

  public class PrepareTextMessage1ReferralsToContactAsyncTests
    : MessageServiceTests
  {
    private readonly IMessageService _service;
    public PrepareTextMessage1ReferralsToContactAsyncTests(
      ServiceFixture serviceFixture) : base(serviceFixture)
    {
      _service = new MessageService(
        _context,
        _loggerMock.Object,
        _mockOptions.Object,
        _mockNotificationService.Object);
    }

    [Fact]
    public async Task 
      PrepareTextMessage1ReferralsToContactAsync_ReturnsCorrectIds()
    {
      // Arrange
      
      string expectedCountMessage =
        $"because one referral is eligible for {TextMessage2} status";
      string expectedContainsMessage =
        $"because this referral is eligible for {TextMessage2} status";
      string assertMessage = "because no new referrals should have been " +
        "added to the database";
      DateTime now = DateTimeOffset.Now.Date;
      DateTime yesterday = now.AddDays(-1);

      Entities.Referral referral1 = ReferralEntityCreator(
          status: FailedToContact,
          source: ReferralSource.Msk);
      Entities.Referral referral2 = ReferralEntityCreator(
          status: New,
          source: ReferralSource.GpReferral);
      Entities.Referral referral3 = ReferralEntityCreator(
          status: TextMessage1,
          source: ReferralSource.SelfReferral);
      Entities.Referral referral4 = ReferralEntityCreator(
          status: TextMessage1,
          source: ReferralSource.GpReferral);
      Entities.Referral referral5 = ReferralEntityCreator(
          status: TextMessage2,
          source: ReferralSource.GpReferral);
      Entities.Referral referral6 = ReferralEntityCreator(
        status: TextMessage1,
        source: ReferralSource.GpReferral);

      string[] expectedIds = new[] { 
        referral4.Id.ToString() ,
        referral6.Id.ToString()
      };

      List<Entities.Referral> referrals = new()
        {
          referral1,
          referral2,
          referral3,
          referral4,
          referral5,
          referral6
        };

      List<Entities.MessageQueue> messages = GenerateTextMessageQueues(
        referrals, 
        MessageType.SMS);

      _context.MessagesQueue.AddRange(messages);
      await _context.SaveChangesAsync();

      Entities.MessageQueue message = messages
        .Where(t => t.ReferralId == referral3.Id)
        .First();

      message.SentDate = DateTime.Today.AddDays(-7);
      message.SendResult = "failed";
      await _context.SaveChangesAsync();

      int beforeCount = 
        _context.Referrals.Count(t => t.Status == TextMessage2.ToString());

      int expectedCount = 3;

      // Act
      string[] result;
      using (new AssertionScope())
      {
        result = await _service.PrepareTextMessage1ReferralsToContactAsync();

        // Assert
        result.Should().HaveCount(2, expectedCountMessage);
        result.Should().Contain(expectedIds, expectedContainsMessage);

        int actualCount = _context.Referrals
          .Count(r => r.Status == TextMessage2.ToString());

        actualCount.Should().Be(expectedCount, expectedCountMessage);
        (actualCount - beforeCount).Should().Be(2, expectedCountMessage);
      }

      // Assert again after the scope is exited to ensure that the
      // database was not modified
      List<Entities.Referral> referralsInDb = 
        await _context.Referrals.ToListAsync();
      referralsInDb.Should().HaveCount(6, assertMessage);

      // Clean up.
      Cleanup();
    }
  }

  public class QueueMessagesAsyncTests : MessageServiceTests
  {
    private readonly Mock<MessageService> _mockService;
    private readonly IMessageService service;

    public QueueMessagesAsyncTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _mockService = new(MockBehavior.Strict, 
        new object[] 
        { 
        _context,
        _loggerMock.Object,
        _mockOptions.Object,
        _mockNotificationService.Object
        });

      _mockService.Setup(t=>t.AddReferralToMessageQueue(
        It.IsAny<QueueItem>(), 
        It.IsAny<MessageType>()))
        .Verifiable();

      service = _mockService.Object;
    }

    [Fact]
    public async Task NoReferrals_NoQueuedItems()
    {
      // Arrange.
      string expectedCounts = "0";
      int expectedCount = 4;
      
      _context.Referrals.RemoveRange(_context.Referrals);
      await _context.SaveChangesAsync();

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedCounts);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }
    }

    [Fact]
    public async Task ReferralFailedToContact_NoQueuedItems()
    {
      // Arrange.
      string expectedCounts = "0";
      int expectedCount = 4;
      
      _context.Referrals.RemoveRange(_context.Referrals);
      await _context.SaveChangesAsync();

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedCounts);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }

      // Clean up.
      Cleanup();
    }

    [Theory]
    [InlineData(ReferralSource.GpReferral, false)]
    [InlineData(ReferralSource.SelfReferral, true)]
    [InlineData(ReferralSource.GeneralReferral, false)]
    [InlineData(ReferralSource.ElectiveCare, false)]
    [InlineData(ReferralSource.Msk, false)]
    public async Task ReferralCanceleedDuplicate_Source_QueuedTextItem(
      ReferralSource source,
      bool isValid)
    {
      // Arrange.
      string expectedCounts = "0";
      string expectedTextCount = isValid? "1": "0";
      int expectedCount = 4;

      ReferralEntityCreator(status: CancelledDuplicateTextMessage,
        source: source);

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedTextCount);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }
        

      // Clean up.
      Cleanup();
    }

    [Theory]
    [InlineData(FailedToContactTextMessage)]
    [InlineData(TextMessage1)]
    public async Task ReferralStatuses_QueuedTextItem(ReferralStatus status)
    {
      // Arrange.
      string expectedCounts = "0";
      string expectedTextCount = "1";
      int expectedCount = 4;

      ReferralEntityCreator(status: status);

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedTextCount);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }

      // Clean up.
      Cleanup();
    }

    [Theory]
    [InlineData(ProviderDeclinedTextMessage)]
    [InlineData(ProviderRejectedTextMessage)]
    [InlineData(ProviderTerminatedTextMessage)]
    public async Task ReferralStatuses_Provider_QueuedTextItem(
      ReferralStatus status)
    {
      // Arrange.
      string expectedCounts = "0";
      string expectedTextCount = "1";
      int expectedCount = 4;

      Entities.Provider provider = RandomEntityCreator.CreateRandomProvider();

      _context.Providers.Add(provider);
      _context.SaveChanges();

      ReferralEntityCreator(status: status, providerId: provider.Id);

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedTextCount);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task ReferralWithStatusOfTextMessage2_Valid()
    {
      // Arrange.
      string expectedCounts = "0";
      string expectedTextCount = "1";
      int expectedCount = 4;

      Entities.Referral referral = ReferralEntityCreator(status: TextMessage2);
      // Add previous MessageQueue for this referral

      Entities.MessageQueue message = MessageQueueCreatorAsync(
        apiKeyType: ApiKeyType.TextMessage1,
        personalisationJson: "{}",
        referralId: referral.Id,
        sendResult: "success",
        sentDate: DateTime.UtcNow.AddDays(-3),
        templateId: Guid.Empty,
        sendTo: referral.Mobile,
        type: MessageType.SMS);

      _context.MessagesQueue.Add(message);
      await _context.SaveChangesAsync();

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedTextCount);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task ReferralWithStatusOfTextMessage2_NotValidDate()
    {
      // Arrange.
      string expectedCounts = "0";
      int expectedCount = 4;

      Entities.Referral referral = ReferralEntityCreator(status: TextMessage2);
      // Add previous MessageQueue for this referral

      _ = MessageQueueCreatorAsync(
        apiKeyType: ApiKeyType.TextMessage1,
        personalisationJson: "{}",
        referralId: referral.Id,
        sendResult: "success",
        sentDate: DateTime.UtcNow.AddHours(-24),
        templateId: Guid.Empty,
        sendTo: referral.Mobile,
        type: MessageType.SMS);

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedCounts);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task Referral_QueuedEmailItem()
    {
      // Arrange.
      string expectedCounts = "0";
      string expectedEmailCount = "1";
      int expectedCount = 4;

      Entities.Referral referral = ReferralEntityCreator(
       status: FailedToContactEmailMessage);

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedEmailCount);
        result[KEY_TEXT_QUEUED].Should().Be(expectedCounts);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedCounts);
        result[KEY_VALIDATION].Should().BeNullOrWhiteSpace();
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task Referral_AddItemValidationException()
    {
      // Arrange.
      string expectedCounts = "0";
      string expectedExceptionCount = "1";
      string exceptionMessage = "Test Validation Message.";
      int expectedCount = 4;

      Entities.Referral referral = ReferralEntityCreator(
       status: FailedToContactEmailMessage);

      _mockService.Setup(t => t.AddReferralToMessageQueue(
        It.IsAny<QueueItem>(),
        It.IsAny<MessageType>()))
        .Throws(new ValidationException(exceptionMessage));

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedCounts);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedExceptionCount);
        result[KEY_VALIDATION].Should().Be(exceptionMessage);
      }

      // Clean up.
      Cleanup();
    }

    [Fact]
    public async Task Referral_AddItemTemplateNotFoundException()
    {
      // Arrange.
      string expectedCounts = "0";
      string expectedExceptionCount = "1";
      string exceptionMessage = "Test Validation Message.";
      int expectedCount = 4;

      Entities.Referral referral = ReferralEntityCreator(
       status: FailedToContactEmailMessage);

      _mockService.Setup(t => t.AddReferralToMessageQueue(
        It.IsAny<QueueItem>(),
        It.IsAny<MessageType>()))
        .Throws(new TemplateNotFoundException(exceptionMessage));

      // Act.
      Dictionary<string, string> result = await service.QueueMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_EMAILS_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_TEXT_QUEUED).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION_COUNT).Should().BeTrue();
        result.ContainsKey(KEY_VALIDATION).Should().BeTrue();

        result[KEY_EMAILS_QUEUED].Should().Be(expectedCounts);
        result[KEY_TEXT_QUEUED].Should().Be(expectedCounts);
        result[KEY_VALIDATION_COUNT].Should().Be(expectedExceptionCount);
        result[KEY_VALIDATION].Should().Be(exceptionMessage);
      }

      // Clean up.
      Cleanup();
    }
  }

  public class SendQueuedMessagesAsyncTests : MessageServiceTests
  {
    private IMessageService _service;

    public SendQueuedMessagesAsyncTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
    }

    private void SetupResponseMessage(
      bool queueIsNull = false, 
      HttpStatusCode status = HttpStatusCode.OK, 
      string error = null)
    {
      if (queueIsNull)
      {
        _mockNotificationService.Setup(
          t => t.SendMessageAsync(It.IsAny<MessageQueue>()))
            .Throws(new ValidationException(error));
      }
      else if (status == HttpStatusCode.OK)
      {
        _mockNotificationService.Setup(
          t => t.SendMessageAsync(It.IsAny<MessageQueue>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
      }
      else
      {
        _mockNotificationService.Setup(
           t => t.SendMessageAsync(It.IsAny<MessageQueue>()))
            .Throws(new NotificationProxyException(error));
      }

      _service = new MessageService(
        _context,
        _loggerMock.Object,
        _mockOptions.Object,
        _mockNotificationService.Object
        );

    }

    [Fact]
    public async Task NoMessagesToSend()
    {
      // Arrange.
      SetupResponseMessage();
      string expectedMessage = "No messages queued to send.";
      int expectedCount = 1;

      // Act.
      Dictionary<string, string> result = 
        await _service.SendQueuedMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_INFORMATION).Should().BeTrue();

        result[KEY_INFORMATION].Should().Be(expectedMessage);
      }
    }

    [Fact]
    public async Task MessagesToSend_Throw_NotificationProxyException()
    {
      // Arrange.
      Dictionary<string, dynamic> personalisation = new() {
        { "givenName", "Test"} };
      string expectedErrorMessage = "Text Error.";
      SetupResponseMessage(
        status: HttpStatusCode.BadRequest, 
        error: expectedErrorMessage
        );

      Entities.Referral referral = ReferralEntityCreator(
        status: FailedToContactEmailMessage,
        source: ReferralSource.Pharmacy);

      Entities.MessageQueue message = MessageQueueCreatorAsync(
        referralId: referral.Id,
        personalisationJson: JsonConvert.SerializeObject(personalisation),
        templateId: _templateId
        );


      _context.MessagesQueue.Add(message);
      await _context.SaveChangesAsync();

      int expectedCount = 4;

      // Act.
      Dictionary<string, string> result =
        await _service.SendQueuedMessagesAsync();

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Count().Should().Be(expectedCount);
        result.ContainsKey(KEY_TOTAL_TO_SEND).Should().BeTrue();
        result.ContainsKey(KEY_TOTAL_SENT).Should().BeTrue();
        result.ContainsKey(KEY_EXCEPTIONS).Should().BeTrue();
        result.ContainsKey(KEY_EXCEPTIONS_MESSAGE).Should().BeTrue();

        result[KEY_TOTAL_TO_SEND].Should().Be("1");
        result[KEY_TOTAL_SENT].Should().Be("0");
        result[KEY_EXCEPTIONS].Should().Be("1");
        result[KEY_EXCEPTIONS_MESSAGE].Trim().Should()
          .Be(expectedErrorMessage);
        message.SendResult.Trim().Should().Be(expectedErrorMessage);
        message.SentDate.Should().NotBeNull();
      }
    }
  }

  private Entities.Referral ReferralEntityCreator(
    ReferralStatus status = New,
    ReferralSource source = ReferralSource.SelfReferral,
    string mobile = null,
    string email = null,
    Guid? providerId = null)
  {
    Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
      email: email ?? Generators.GenerateEmail(),
      status: status,
      referralSource: source,
      mobile: mobile ?? Generators.GenerateMobile(new Random()));

    referral.ReferringOrganisationEmail =
      email ?? Generators.GenerateEmail();

    referral.ProviderId = providerId;

    _context.Referrals.Add(referral);
    _context.SaveChanges();
    _context.ChangeTracker.Clear();
    return referral;
  }

  private QueueItem QueueItemCreator(ReferralStatus status = New,
    ReferralSource source = ReferralSource.SelfReferral,
    string mobile = null,
    string email = null)
  {
    Entities.Referral referral = 
      ReferralEntityCreator(status, source, mobile, email);
    return new QueueItem()
    {
      Id = referral.Id,
      Status = referral.Status,
      Source = referral.ReferralSource,
      EmailAddress = referral.Email,
      GivenName = referral.GivenName,
      MobileNumber = referral.Mobile,
      NhsNumber = referral.NhsNumber,
      ReferringClinicianEmail = referral.ReferringClinicianEmail,
      ReferringOrganisationEmail = referral.ReferringOrganisationEmail,
      Ubrn = referral.Ubrn
    };

  }

  private List<Entities.MessageQueue> GenerateTextMessageQueues(
    List<Entities.Referral> referrals, 
    MessageType messageType = MessageType.SMS)
  {
    List<Entities.MessageQueue> messageQueues = new ();

    ReferralStatus allowed = TextMessage1 | TextMessage2;

    foreach (Entities.Referral referral in referrals)
    {
      if (!allowed.HasFlag(referral.Status.ToEnum<ReferralStatus>()))
      {
        continue;
      }

      if (referral.Status == TextMessage1.ToString())
      {
        Entities.MessageQueue messageQueue = MessageQueueCreatorAsync(
          apiKeyType:  ApiKeyType.TextMessage1,
          referralId: referral.Id,
          sentDate: DateTime.Today.AddDays(-14),
          sendResult: "success",
          type: messageType
       );

        messageQueues.Add(messageQueue);

      }
      else if (referral.Status == TextMessage2.ToString())
      {
        Entities.MessageQueue messageQueue = MessageQueueCreatorAsync(
          apiKeyType: ApiKeyType.TextMessage1,
          referralId: referral.Id,
          sentDate: DateTime.Today.AddDays(-14),
          sendResult: "success",
          type: messageType
          );

        messageQueues.Add(messageQueue);
        // Add TextMessage1 message for previous week
        Entities.MessageQueue textMessage1Message = 
          MessageQueueCreatorAsync(
            apiKeyType: ApiKeyType.TextMessage2,
            referralId: referral.Id,
            sentDate: DateTime.Today.AddDays(-7),
            sendResult: "success",
            type: messageType
            );
        messageQueues.Add(textMessage1Message);
      }
    }

    return messageQueues;
  }


  private Entities.MessageQueue MessageQueueCreatorAsync(
    ApiKeyType apiKeyType = ApiKeyType.FailedToContact,
    string personalisationJson = null,
    Guid? referralId = null,
    string sendResult = null,
    DateTime? sentDate = null,
    string sendTo = null,
    Guid? templateId = null,
    MessageType type = MessageType.Email)
  {
    return new()
    {
      ApiKeyType = apiKeyType,
      Id = Guid.NewGuid(),
      PersonalisationJson = personalisationJson,
      ReferralId = referralId ?? Guid.Empty,
      SendResult = sendResult,
      SentDate = sentDate,
      SendTo = sendTo,
      TemplateId = templateId ?? Guid.Empty,
      Type = type,
      IsActive = true
    };
  }

  private void Cleanup()
  {
    _context.Providers.RemoveRange(_context.Providers);
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.MessagesQueue.RemoveRange(_context.MessagesQueue);
    _context.SaveChanges();
    _context.ChangeTracker.Clear();
  }

}
