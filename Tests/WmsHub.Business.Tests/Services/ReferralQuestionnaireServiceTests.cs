using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class ReferralQuestionnaireServiceTests
  : ServiceTestsBase, IDisposable
{
  protected readonly DatabaseContext _context;
  protected readonly IReferralQuestionnaireService _service;
  protected readonly IConfiguration _configuration;
  protected readonly Mock<ILinkIdService> _mockLinkIdService;
  protected readonly Mock<INotificationService> _notificationServiceMock;
  protected readonly Mock<IOptions<QuestionnaireNotificationOptions>> 
    _mockOptions = new();
  protected readonly Mock<ILogger> _loggerMock;

  public ReferralQuestionnaireServiceTests(ServiceFixture serviceFixture)
    : base(serviceFixture)
  {
    string dateString = new DateTimeOffset(2021, 4, 1, 0, 0, 0, TimeSpan.Zero)
      .Date
      .AddDays(1)
      .AddMilliseconds(-1)
      .ToString("yyyy-MM-ddTHH:mm:ss zzz");
    Dictionary<string, string> inMemorySettings = new()
    {
      { "SmsRetryAttempts", "5" },
      { "TemplateGpReferral", "TemplateGpReferral" },
      { "TemplateSelfReferral", "TemplateSelfReferral" },
      { "NotificationQuestionnaireLink", "NotificationQuestionnaireLink" },
      { "QuestionnaireExpiryDays", "28" },
      { "CreateQuestionnaireFromDate", dateString }
    };

    _configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(inMemorySettings)
      .Build();

    _context = new DatabaseContext(_serviceFixture.Options);
    _mockLinkIdService = new();
    _notificationServiceMock = new();
    _loggerMock = new();

    QuestionnaireNotificationOptions options = new()
    {
      Endpoint = "test",
      NotificationApiKey = Guid.NewGuid().ToString(),
      NotificationApiUrl = "https://localtest.com",
      NotificationEmailLink = "https://localtest.com/test",
      NotificationSenderId = "Hsenderid",
      NotificationQuestionnaireLink = "https://questionlink.com"
    };

    _mockOptions.Setup(t => t.Value).Returns(options);

    _service = new ReferralQuestionnaireService(
      _context,
      _configuration,
      _mockLinkIdService.Object,
      _mockOptions.Object,
      _notificationServiceMock.Object,
      _loggerMock.Object)
    {
      User = GetClaimsPrincipal()
    };

    CleanUp();
  }

  [Fact]
  public void
    WhenConfigurationIsNullShouldThrowArgumentNullException()
  {
    // Assert.
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        ReferralQuestionnaireService service = new(
          _context,
          null,
          _mockLinkIdService.Object,
          _mockOptions.Object,
          _notificationServiceMock.Object,
          _loggerMock.Object);
      };

      // Assert.
      action.Should().Throw<ArgumentNullException>();
    }
  }

  [Fact]
  public void
    WhenNotificationServiceIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        ReferralQuestionnaireService service = new(
          _context,
          _configuration,
          _mockLinkIdService.Object,
          _mockOptions.Object,
          null,
          _loggerMock.Object);
      };

      // Assert.
      action.Should().Throw<ArgumentNullException>();
    }
  }

  [Fact]
  public void
    WhenLoggerIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        ReferralQuestionnaireService service = new(
          _context,
          _configuration,
          _mockLinkIdService.Object,
          _mockOptions.Object,
          _notificationServiceMock.Object,
          null);
      };

      // Assert.
      action.Should().Throw<ArgumentNullException>();
    }
  }

  public void Dispose()
  {
    CleanUp();
  }

  protected void AddRandomQuestionnairesInDatabase()
  {
    _context.Add(RandomEntityCreator.CreateRandomQuestionnaire(
      type: QuestionnaireType.CompleteProT1,
      id: Guid.NewGuid()));
    _context.Add(RandomEntityCreator.CreateRandomQuestionnaire(
      type: QuestionnaireType.CompleteProT2and3,
      id: Guid.NewGuid()));
    _context.Add(RandomEntityCreator.CreateRandomQuestionnaire(
      type: QuestionnaireType.CompleteSelfT1,
      id: Guid.NewGuid()));
    _context.Add(RandomEntityCreator.CreateRandomQuestionnaire(
      type: QuestionnaireType.CompleteSelfT2and3,
      id: Guid.NewGuid()));
    _context.Add(RandomEntityCreator.CreateRandomQuestionnaire(
      type: QuestionnaireType.NotCompleteProT1and2and3,
      id: Guid.NewGuid()));
    _context.Add(RandomEntityCreator.CreateRandomQuestionnaire(
      type: QuestionnaireType.NotCompleteSelfT1and2and3,
      id: Guid.NewGuid()));

    _context.SaveChanges();
  }

  protected void AddRandomReferralsInDatabase()
  {
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "07595470000",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.Complete.ToString(),
      referralSource: ReferralSource.GpReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.Low.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "+447000000001",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.Complete.ToString(),
      referralSource: ReferralSource.GpReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.Medium.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "+447000000002",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.Complete.ToString(),
      referralSource: ReferralSource.GpReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.High.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "+447000000004",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.Complete.ToString(),
      referralSource: ReferralSource.SelfReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.Low.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "+447000004292",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.Complete.ToString(),
      referralSource: ReferralSource.SelfReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.Medium.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "+447700900003",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.Complete.ToString(),
      referralSource: ReferralSource.SelfReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.High.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "+447000005001",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
      referralSource: ReferralSource.GpReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.Medium.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "+447000004291",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
      referralSource: ReferralSource.SelfReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.High.ToString()));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "07595000008",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
      referralSource: ReferralSource.SelfReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.High.ToString(),
      consentForFutureContactForEvaluation: false));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "07595000009",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
      referralSource: ReferralSource.SelfReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.High.ToString(),
      consentForFutureContactForEvaluation: false));
    _context.Add(RandomEntityCreator.CreateRandomReferral(
      id: Guid.NewGuid(),
      mobile: "07595000009",
      isActive: true,
      programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
      referralSource: ReferralSource.SelfReferral,
      status: ReferralStatus.Complete,
      triagedCompletionLevel: TriageLevel.High.ToString(),
      consentForFutureContactForEvaluation: true,
      dateOfReferral:
        new DateTimeOffset(2021, 4, 1, 0, 0, 0, TimeSpan.Zero)));

    _context.SaveChanges();
  }
  
  protected void CleanUp()
  {
    _context.Referrals.RemoveRange(_context.Referrals);
    _context.Questionnaires.RemoveRange(_context.Questionnaires);
    _context.ReferralQuestionnaires
      .RemoveRange(_context.ReferralQuestionnaires);
    _context.SaveChanges();
    _context.Referrals.Count().Should().Be(0);
    _context.Questionnaires.Count().Should().Be(0);
    _context.ReferralQuestionnaires.Count().Should().Be(0);
  }

  public class ReferralQuestionnaireServiceConstructor
    : ReferralQuestionnaireServiceTests
  {
    public ReferralQuestionnaireServiceConstructor(
      ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public void ReferralQuestionnaireServiceInstantiate()
    {
      // Assert.
      _service.Should().NotBeNull();
    }
  }

  public class CreateAsync : ReferralQuestionnaireServiceTests
  {
    public CreateAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task CreateReferralQuestionnaires()
    {
      // Arrange.
      AddRandomQuestionnairesInDatabase();
      AddRandomReferralsInDatabase();

      // Act.
      CreateReferralQuestionnaireResponse response =
        await _service.CreateAsync(null, 100, DateTimeOffset.Now.Date);

      // Assert.
      using (new AssertionScope())
      {
        response.Status.Should().Be(CreateQuestionnaireStatus.Valid);
        response.NumberOfQuestionnairesCreated.Should().Be(9);

        List<Entities.ReferralQuestionnaire> rqCompleteProT1 =
          Get(QuestionnaireType.CompleteProT1);
        rqCompleteProT1.Count.Should().Be(1);
        rqCompleteProT1.First().ModifiedByUserId.Should().NotBeEmpty();
        rqCompleteProT1.First().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);

        List<Entities.ReferralQuestionnaire> rqCompleteProT2and3 =
          Get(QuestionnaireType.CompleteProT2and3);
        rqCompleteProT2and3.Count.Should().Be(2);
        rqCompleteProT2and3.First().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqCompleteProT2and3.First().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);
        rqCompleteProT2and3.Last().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqCompleteProT2and3.Last().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);

        List<Entities.ReferralQuestionnaire> rqCompleteSelfT1 =
          Get(QuestionnaireType.CompleteSelfT1);
        rqCompleteSelfT1.Count.Should().Be(1);
        rqCompleteSelfT1.First().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqCompleteSelfT1.First().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);

        List<Entities.ReferralQuestionnaire> rqCompleteSelfT2and3 =
          Get(QuestionnaireType.CompleteSelfT2and3);
        rqCompleteSelfT2and3.Count.Should().Be(2);
        rqCompleteSelfT2and3.First().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqCompleteSelfT2and3.First().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);
        rqCompleteSelfT2and3.Last().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqCompleteSelfT2and3.Last().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);

        List<Entities.ReferralQuestionnaire> rqNotCompleteProT1and2and3 =
          Get(QuestionnaireType.NotCompleteProT1and2and3);
        rqNotCompleteProT1and2and3.Count.Should().Be(1);
        rqNotCompleteProT1and2and3.First().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqNotCompleteProT1and2and3.First().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);

        List<Entities.ReferralQuestionnaire> rqNotCompleteSelfT1and2and3 =
          Get(QuestionnaireType.NotCompleteSelfT1and2and3);
        rqNotCompleteSelfT1and2and3.Count.Should().Be(2);
        rqNotCompleteSelfT1and2and3.First().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqNotCompleteSelfT1and2and3.First().ModifiedByUserId.Should()
          .Be(TEST_USER_ID); 
        rqNotCompleteSelfT1and2and3.Last().ModifiedByUserId.Should()
          .NotBeEmpty();
        rqNotCompleteSelfT1and2and3.Last().ModifiedByUserId.Should()
          .Be(TEST_USER_ID);
      }

      CleanUp();
    }

    [Fact]
    public async Task
      ShouldNotCreateReferralQuestionnairesForReferralsBeforeFromDate()
    {
      // Arrange.
      AddRandomQuestionnairesInDatabase();

      _context.Add(RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07595000009",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.DidNotComplete.ToString(),
        referralSource: ReferralSource.SelfReferral,
        triagedCompletionLevel: TriageLevel.High.ToString(),
        consentForFutureContactForEvaluation: true,
        dateOfReferral:
          new DateTimeOffset(2021, 4, 1, 0, 0, 0, TimeSpan.Zero).Date));

      _context.SaveChanges();

      // Act.
      CreateReferralQuestionnaireResponse response =
        await _service.CreateAsync(null, 100, DateTimeOffset.Now.Date);

      // Assert.
      using (new AssertionScope())
      {
        response.Status.Should().Be(CreateQuestionnaireStatus.Valid);
        response.NumberOfQuestionnairesCreated.Should().Be(0);

        _context.ReferralQuestionnaires.ToList().Should().BeEmpty();
      }

      CleanUp();
    }

    [Fact]
    public async Task BadRequestWhenToDateLessThanFromDate()
    {
      // Arrange.
      DateTimeOffset fromDate = new DateTimeOffset(
        2022, 7, 1, 0, 0, 0, TimeSpan.Zero)
        .Date
        .AddDays(1)
        .AddMilliseconds(-1);
      string expectedDetail = $"ToDate must be after {fromDate}.";
      AddRandomQuestionnairesInDatabase();
      AddRandomReferralsInDatabase();

      // Act.
      CreateReferralQuestionnaireResponse response =
        await _service.CreateAsync(
          fromDate,
          100,
          new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero));

      // Assert.
      using (new AssertionScope())
      {
        response.Status.Should().Be(CreateQuestionnaireStatus.BadRequest);
        response.Errors.Should().NotBeEmpty();
        response.Errors[0].Should().Be(expectedDetail);
      }

      CleanUp();
    }

    [Fact]
    public async Task BadRequestWhenToDateLessThanConfiguredFromDate()
    {
      // Arrange.
      DateTimeOffset configuredDate = new DateTimeOffset(
        2021, 4, 1, 0, 0, 0, TimeSpan.Zero)
        .Date
        .AddDays(1)
        .AddMilliseconds(-1);
      string expectedDetail = $"ToDate must be after {configuredDate}.";
      AddRandomQuestionnairesInDatabase();
      AddRandomReferralsInDatabase();

      // Act.
      CreateReferralQuestionnaireResponse response =
        await _service.CreateAsync(
          null,
          100,
          new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero));

      // Assert.
      using (new AssertionScope())
      {
        response.Status.Should().Be(CreateQuestionnaireStatus.BadRequest);
        response.Errors.Should().NotBeEmpty();
        response.Errors[0].Should().Be(expectedDetail);
      }

      CleanUp();
    }

    [Fact]
    public async Task BadRequestWhenFromDateLessThanConfiguredFromDate()
    {
      // Arrange.
      string expectedDetail = $"From date must be after 01/04/2021 23:59:59.";
      AddRandomQuestionnairesInDatabase();
      AddRandomReferralsInDatabase();

      // Act.
      CreateReferralQuestionnaireResponse response =
        await _service.CreateAsync(
          new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
          100,
          new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero));

      // Assert.
      using (new AssertionScope())
      {
        response.Status.Should().Be(CreateQuestionnaireStatus.BadRequest);
        response.Errors.Should().NotBeEmpty();
        response.Errors[0].Should().Be(expectedDetail);
      }

      CleanUp();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(251)]
    public async Task BadRequestWhenMaxNumberToCreateOutOfRange(int maxNumberToCreate)
    {
      // Arrange.
      string expectedDetail = "MaxNumberToCreate must be in the range 1 to 250.";
      AddRandomQuestionnairesInDatabase();
      AddRandomReferralsInDatabase();

      // Act.
      CreateReferralQuestionnaireResponse response =
        await _service.CreateAsync(
          null,
          maxNumberToCreate,
          DateTimeOffset.Now.Date);

      // Assert.
      response.Status.Should().Be(CreateQuestionnaireStatus.BadRequest);
      response.Errors.Should().NotBeEmpty().And.ContainSingle(expectedDetail);

      CleanUp();
    }

    [Fact]
    public async Task ConflictWhenProcessAlreadyRunning()
    {
      const string CONFIG_ID = "WmsHub_ReferralQuestionnaireService_IsRunning:CreateAsync";

      // Arrange.
      ConfigurationValue isRunningConfig = _context.ConfigurationValues
        .SingleOrDefault(c => c.Id == CONFIG_ID);

      if (isRunningConfig == default)
      {
        _context.ConfigurationValues.Add(new ConfigurationValue()
        {
          Id = CONFIG_ID,
          Value = "True"
        });
      }
      else
      {
        isRunningConfig.Value = "True";
      }

      await _context.SaveChangesAsync();

      string expectedDetail = "Process is already running.";
      AddRandomQuestionnairesInDatabase();
      AddRandomReferralsInDatabase();

      // Act.
      CreateReferralQuestionnaireResponse response =
        await _service.CreateAsync(
          null, 100, DateTimeOffset.Now.Date);

      // Assert.
      response.Status.Should().Be(CreateQuestionnaireStatus.Conflict);
      response.Errors.Should().NotBeEmpty().And.ContainSingle(expectedDetail);

      isRunningConfig.Value = "False";
      await _context.SaveChangesAsync();

      CleanUp();
    }
  }

  public class SendAsync : ReferralQuestionnaireServiceTests
  {
    public SendAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task WhenNoReferralQuestionairesToSend()
    {
      // Act.
      SendReferralQuestionnaireResponse response =
        await _service.SendAsync();

      // Assert.
      using (new AssertionScope())
      {
        response.NoQuestionnairesToSend.Should().Be(true);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenReferralQuestionairesToSend()
    {
      // Arrange.
      string invalidMobileNumber =
        "The Mobile field is not a valid UK mobile phone number.";
      AddRandomQuestionnairesInDatabase();
      AddRandomReferralsInDatabase();
      await _service.CreateAsync(null, 100, DateTimeOffset.Now.Date);
      _notificationServiceMock
        .Setup(x => x.SendNotificationAsync(It.IsAny<SmsPostRequest>()))
        .ReturnsAsync((SmsPostRequest request) =>
        {
          switch (request.Mobile)
          {
            case "+447000000001":
            case "+447000000002":
            case "+447000000004":
            case "+447700900003":
            return new SmsPostResponse
            {
              ClientReference = request.ClientReference,
              Status = "Created"
            };
            case "+447000004292":
            case "+447000004291":
            return new SmsPostResponse
            {
              ClientReference = request.ClientReference,
              Status = "TechnicalFailure"
            };
            case "+447000005001":
            return new SmsPostResponse
            {
              ClientReference = request.ClientReference,
              Status = "TechnicalFailure"
            };
            default:
            SmsPostResponse response = new()
            {
              ClientReference = request.ClientReference,
              Status = "PermanentFailure",
            };
            response.GetNotificationErrors
              .Add(invalidMobileNumber);

            return response;
          }
        });

      // Act.
      SendReferralQuestionnaireResponse response =
        await _service.SendAsync();

      // Assert.
      using (new AssertionScope())
      {
        response.NoQuestionnairesToSend.Should().Be(false);
        response.NumberOfReferralQuestionnairesFailed.Should().Be(5);
        response.NumberOfReferralQuestionnairesSent.Should().Be(4);
      }

      CleanUp();
    }
  }

  public class StartAsync : ReferralQuestionnaireServiceTests
  {
    public StartAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Theory]
    [InlineData(ReferralQuestionnaireStatus.Delivered)]
    [InlineData(ReferralQuestionnaireStatus.Started)]
    [InlineData(ReferralQuestionnaireStatus.Sending)]
    public async Task
      WhenNotificationKeyAndQuestionnaireTypeIsValid(
        ReferralQuestionnaireStatus status)
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: status);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      StartReferralQuestionnaire startReferralQuestionnaire =
        await _service.StartAsync(referralQuestionnaire.NotificationKey);

      // Assert.
      using (new AssertionScope())
      {
        ReferralQuestionnaire savedEntity = Get(referralQuestionnaire.Id);

        startReferralQuestionnaire.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.Valid);
        savedEntity.Status.Should().Be(ReferralQuestionnaireStatus.Started);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenExpired()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Delivered,
          delivered: DateTime.Now.AddDays(-(Constants.QUESTIONNAIRE_EXPIRY_DAYS + 1)));
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();
      _loggerMock.Setup(x => x.Warning(It.IsAny<string>())).Verifiable();

      // Act.
      StartReferralQuestionnaire startReferralQuestionnaire =
        await _service.StartAsync(referralQuestionnaire.NotificationKey);

      // Assert.
      using (new AssertionScope())
      {
        startReferralQuestionnaire.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.Expired);

        _loggerMock.Verify(x => x.Warning(It.IsAny<string>()), Times.Once);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenStatusCompleted()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Completed);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      StartReferralQuestionnaire startReferralQuestionnaire =
        await _service.StartAsync(referralQuestionnaire.NotificationKey);

      // Assert.
      using (new AssertionScope())
      {
        startReferralQuestionnaire.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.Completed);
        startReferralQuestionnaire.Status
          .Should()
          .Be(ReferralQuestionnaireStatus.Completed);
      }

      CleanUp();
    }

    [Theory]
    [InlineData(ReferralQuestionnaireStatus.TechnicalFailure)]
    [InlineData(ReferralQuestionnaireStatus.PermanentFailure)]
    [InlineData(ReferralQuestionnaireStatus.TemporaryFailure)]
    [InlineData(ReferralQuestionnaireStatus.Created)]
    public async Task
      WhenStatusNotStartedOrDeliveredOrCompleted(
      ReferralQuestionnaireStatus status)
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: status);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      StartReferralQuestionnaire startReferralQuestionnaire =
        await _service.StartAsync(referralQuestionnaire.NotificationKey);

      // Assert.
      using (new AssertionScope())
      {
        startReferralQuestionnaire.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.NotDelivered);
        startReferralQuestionnaire.Status
          .Should()
          .Be(status);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenNotificationKeyNotFound()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Delivered);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      StartReferralQuestionnaire startReferralQuestionnaire =
        await _service.StartAsync(
          "Notification key Not Found");

      // Assert.
      using (new AssertionScope())
      {
        startReferralQuestionnaire.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.NotificationKeyNotFound);
      }

      CleanUp();
    }
  }

  public class CompleteAsync : ReferralQuestionnaireServiceTests
  {
    const string VALID_ANSWERS_START =
      "[{\"QuestionId\":1," +
      "\"a\":\"StronglyAgree\"," +
      "\"b\":\"Agree\"," +
      "\"c\":\"NeitherAgreeOrDisagree\"}," +
      "{\"QuestionId\":2," +
      "\"a\":\"StronglyAgree\"," +
      "\"b\":\"Agree\"," +
      "\"c\":\"NeitherAgreeOrDisagree\"" +
      ",\"d\":\"Disagree\"" +
      ",\"e\":\"StronglyDisagree\"," +
      "\"f\":\"StronglyAgree\"," +
      "\"g\":\"Agree\"," +
      "\"h\":\"NeitherAgreeOrDisagree\"}," +
      "{\"QuestionId\":3," +
      "\"a\":\"StronglyAgree\"," +
      "\"b\":\"Agree\"," +
      "\"c\":\"NeitherAgreeOrDisagree\"," +
      "\"d\":\"Disagree\"}," +
      "{\"QuestionId\":4," +
      "\"a\":\"VeryGood\"}," +
      "{\"QuestionId\":5," +
      "\"a\":\"some random\"}," +
      "{\"QuestionId\":6," +
      "\"a\":\"\"},";

    const string VALID_ANSWERS = VALID_ANSWERS_START +
      "{\"QuestionId\":7," +
      "\"a\":\"true\"," +
      "\"b\":\"test.questionnaire@nhs.net\"," +
      "\"c\":\"+447715427599\"," +
      "\"d\":\"TestGiven\"," +
      "\"e\":\"TestFamily\"" +
      "}]";

    public CompleteAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task WhenNotificationKeyAndQuestionnaireTypeIsValid()
    {
      // Arrange.
      string expectedAnswers = VALID_ANSWERS;
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "+447596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Started);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      CompleteQuestionnaireResponse completeReferralQuestionnaireResponse =
        await _service.CompleteAsync(new CompleteQuestionnaire
        {
          NotificationKey = referralQuestionnaire.NotificationKey,
          QuestionnaireType = questionnaire.Type,
          Answers = expectedAnswers,
        });

      // Assert.
      using (new AssertionScope())
      {
        completeReferralQuestionnaireResponse.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.Valid);

        ReferralQuestionnaire savedEntity = Get(referralQuestionnaire.Id);
        savedEntity.Answers.Should().Be(expectedAnswers);
        savedEntity.ConsentToShare.Should().BeTrue();
        savedEntity.Email.Should().Be("test.questionnaire@nhs.net");
        savedEntity.FamilyName.Should().Be("TestFamily");
        savedEntity.GivenName.Should().Be("TestGiven");
        savedEntity.Mobile.Should().Be("+447715427599");
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenNotificationKeyNotFound()
    {
      // Arrange.
      string expectedAnswers = VALID_ANSWERS;
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "+447596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Started);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      CompleteQuestionnaireResponse
        completeReferralQuestionnaireResponse =
        await _service.CompleteAsync(new CompleteQuestionnaire
        {
          NotificationKey = "Notification key Not Found",
          QuestionnaireType = questionnaire.Type,
          Answers = expectedAnswers,
        });

      // Assert.
      using (new AssertionScope())
      {
        completeReferralQuestionnaireResponse.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.NotificationKeyNotFound);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenBadRequest()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "+447596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Started);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      CompleteQuestionnaireResponse
        completeReferralQuestionnaireResponse =
        await _service.CompleteAsync(new CompleteQuestionnaire
        {
          NotificationKey = "Notification key",
          QuestionnaireType = questionnaire.Type,
          Answers = "[]",
        });

      // Assert.
      using (new AssertionScope())
      {
        completeReferralQuestionnaireResponse.ValidationState
          .Should().Be(ReferralQuestionnaireValidationState.BadRequest);

        completeReferralQuestionnaireResponse.GetQuestionnaireTypeErrors.Count
          .Should().Be(1);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenQuestionnaireTypeIncorrect()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "+447596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Started);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      CompleteQuestionnaireResponse
        completeReferralQuestionnaireResponse =
        await _service.CompleteAsync(new CompleteQuestionnaire
        {
          NotificationKey = referralQuestionnaire.NotificationKey,
          QuestionnaireType = QuestionnaireType.CompleteProT1,
          Answers = VALID_ANSWERS,
        });

      // Assert.
      using (new AssertionScope())
      {
        completeReferralQuestionnaireResponse.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.QuestionnaireTypeIncorrect);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenIncorrectStatus()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "+447596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Sending);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      CompleteQuestionnaireResponse
        completeReferralQuestionnaireResponse =
        await _service.CompleteAsync(new CompleteQuestionnaire
        {
          NotificationKey = referralQuestionnaire.NotificationKey,
          QuestionnaireType = questionnaire.Type,
          Answers = VALID_ANSWERS,
        });

      // Assert.
      using (new AssertionScope())
      {
        completeReferralQuestionnaireResponse.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.IncorrectStatus);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenRequestIsNull()
    {
      // Arrange.
      // Act.
      CompleteQuestionnaireResponse
        completeReferralQuestionnaireResponse =
          await _service.CompleteAsync(null);

      // Assert.
      using (new AssertionScope())
      {
        completeReferralQuestionnaireResponse.ValidationState
          .Should()
          .Be(ReferralQuestionnaireValidationState.BadRequest);
      }

      CleanUp();
    }
  }

  public class CallbackAsync : ReferralQuestionnaireServiceTests
  {
    public CallbackAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Theory]
    [InlineData(NotificationProxyCallbackRequestStatus.Delivered)]
    [InlineData(NotificationProxyCallbackRequestStatus.TemporaryFailure)]
    [InlineData(NotificationProxyCallbackRequestStatus.TechnicalFailure)]
    [InlineData(NotificationProxyCallbackRequestStatus.PermanentFailure)]
    public async Task WhenClientReferenceAndStatusIsValid(
      NotificationProxyCallbackRequestStatus requestStatus)
    {
      // Arrange.
      DateTime statusAt = DateTime.Now;
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Sending);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      NotificationCallbackStatus status =
        await _service.CallbackAsync(new NotificationProxyCallback
        {
          Id = "id",
          ClientReference = referralQuestionnaire.Id.ToString(),
          Status = requestStatus,
          StatusAt = statusAt
        });

      ReferralQuestionnaire savedEntity = Get(referralQuestionnaire.Id);
      // Assert.
      using (new AssertionScope())
      {
        status.Should().Be(NotificationCallbackStatus.Success);
        switch (requestStatus)
        {
          case NotificationProxyCallbackRequestStatus.TechnicalFailure:
          savedEntity.TechnicalFailure.Should().Be(statusAt);
          savedEntity.Status.Should().Be(
            ReferralQuestionnaireStatus.TechnicalFailure);
          break;
          case NotificationProxyCallbackRequestStatus.TemporaryFailure:
          savedEntity.TemporaryFailure.Should().Be(statusAt);
          savedEntity.Status.Should().Be(
            ReferralQuestionnaireStatus.TemporaryFailure);
          break;
          case NotificationProxyCallbackRequestStatus.PermanentFailure:
          savedEntity.PermanentFailure.Should().Be(statusAt);
          savedEntity.Status.Should().Be(
            ReferralQuestionnaireStatus.PermanentFailure);
          break;
          default:
          savedEntity.Delivered.Should().Be(statusAt);
          savedEntity.Status.Should().Be(
            ReferralQuestionnaireStatus.Delivered);
          break;
        }
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenClientReferenceNotFound()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Started);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      NotificationCallbackStatus status =
        await _service.CallbackAsync(new NotificationProxyCallback
        {
          Id = "id",
          ClientReference = "ClientReference",
          Status = NotificationProxyCallbackRequestStatus.Delivered,
          StatusAt = DateTime.Now
        });

      // Assert.
      using (new AssertionScope())
      {
        status.Should().Be(NotificationCallbackStatus.NotFound);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenStatusUnknown()
    {
      // Arrange.
      Questionnaire questionnaire =
        RandomEntityCreator.CreateRandomQuestionnaire(
          type: QuestionnaireType.CompleteSelfT1,
          id: Guid.NewGuid());
      _context.Add(questionnaire);

      Entities.Referral referral = RandomEntityCreator.CreateRandomReferral(
        id: Guid.NewGuid(),
        mobile: "07596000000",
        isActive: true,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral,
        triagedCompletionLevel: TriageLevel.Low.ToString());
      _context.Add(referral);

      ReferralQuestionnaire referralQuestionnaire =
        RandomEntityCreator.CreateRandomReferralQuestionnaire(
          id: Guid.NewGuid(),
          referralId: referral.Id,
          questionnaireId: questionnaire.Id,
          notificationKey: "Notification key",
          status: ReferralQuestionnaireStatus.Started);
      _context.Add(referralQuestionnaire);
      _context.SaveChanges();

      // Act.
      NotificationCallbackStatus status =
        await _service.CallbackAsync(new NotificationProxyCallback
        {
          Id = "id",
          ClientReference = referralQuestionnaire.Id.ToString(),
          Status = NotificationProxyCallbackRequestStatus.NotDefined,
          StatusAt = DateTime.Now
        });

      // Assert.
      using (new AssertionScope())
      {
        status.Should().Be(NotificationCallbackStatus.Unknown);
      }

      CleanUp();
    }

    [Fact]
    public async Task WhenRequestIsNull()
    {
      // Arrange.
      // Act.
      NotificationCallbackStatus status =
        await _service.CallbackAsync(null);

      // Assert.
      using (new AssertionScope())
      {
        status.Should().Be(NotificationCallbackStatus.BadRequest);
      }

      CleanUp();
    }
  }

  private ReferralQuestionnaire Get(Guid id)
  {
    return _context.ReferralQuestionnaires.Single(
      q => q.Id == id);
  }

  private List<Entities.ReferralQuestionnaire> Get(QuestionnaireType type)
  {
    Guid questionnaireId = _context.Questionnaires.First(
      q => q.Type == type).Id;

    return _context.ReferralQuestionnaires.Where(
      rq => rq.QuestionnaireId == questionnaireId
    ).ToList();
  }
}
