using System;
using WmsHub.Business.Services;
using WmsHub.Referral.Api.Controllers;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using Serilog;
using WmsHub.Referral.Api.Models.ReferralQuestionnaire;
using WmsHub.Business.Enums;
using AutoMapper;
using FluentAssertions.Execution;

namespace WmsHub.Referral.Api.Tests;

[Collection("Service collection")]
public class QuestionnaireControllerTests
  : ServiceTestsBase, IDisposable
{
  private readonly Mock<IReferralQuestionnaireService>
    _referralQuestionnaireServiceMock;
  private readonly Mock<ILogger> _loggerMock;
  private readonly Mock<IMapper> _mapperMock;
  private readonly QuestionnaireController
    _questionnaireController;

  public QuestionnaireControllerTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
  {
    _loggerMock = new Mock<ILogger>();
    _mapperMock = new Mock<IMapper>();
    _referralQuestionnaireServiceMock =
      new Mock<IReferralQuestionnaireService>();

    _loggerMock.Setup(x => x.ForContext<QuestionnaireController>())
      .Returns(_loggerMock.Object);

    _questionnaireController = new QuestionnaireController(
      _mapperMock.Object,
      _loggerMock.Object,
      _referralQuestionnaireServiceMock.Object);

    CleanUp();
  }

  [Fact]
  public void WhenMapperIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        QuestionnaireController questionnaireController = new(
          null,
          _loggerMock.Object,
          _referralQuestionnaireServiceMock.Object);
      };

      // Assert.
      action.Should().Throw<ArgumentNullException>();
    }
  }

  [Fact]
  public void WhenLoggerIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        QuestionnaireController questionnaireController = new(
          _mapperMock.Object,
          null,
          _referralQuestionnaireServiceMock.Object);
      };

      // Assert.
      action.Should().Throw<ArgumentNullException>();
    }
  }

  [Fact]
  public void WhenQuestionnaireServiveIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        QuestionnaireController questionnaireController = new(
          _mapperMock.Object,
          _loggerMock.Object,
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

  protected void CleanUp()
  {
  }

  public class CreateAsync : QuestionnaireControllerTests
  {
    public CreateAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task Ok()
    {
      // Arrange.
      Mock<IReferralQuestionnaireService> serviceMock = new();
      CreateReferralQuestionnaireResponse response = new()
      {
        NumberOfQuestionnairesCreated = 1,
        NumberOfErrors = 0,
        Errors = new(),
        Status = CreateQuestionnaireStatus.Valid
      };

      _referralQuestionnaireServiceMock
        .Setup(x => x.CreateAsync(
          It.IsAny<DateTimeOffset?>(),
          100,
          It.IsAny<DateTimeOffset>()))
        .ReturnsAsync(response);

      // Act.
      IActionResult result =
        await _questionnaireController.CreateAsync(
          new CreateQuestionnaireRequest { MaxNumberToCreate = 100 });

      // Assert.
      using (new AssertionScope())
      {
        OkObjectResult okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        CreateReferralQuestionnaireResponse resultValue =
          okResult.Value.As<CreateReferralQuestionnaireResponse>();
        resultValue.Should().NotBeNull();
        resultValue.Should().BeEquivalentTo(response);
      }
    }

    [Fact]
    public async Task InternalServerError()
    {
      //Arrange.
      string expectedDetail = $"Test Exception: {DateTimeOffset.Now}";
      Mock<IReferralQuestionnaireService> serviceMock = new();

      _referralQuestionnaireServiceMock
        .Setup(x => x.CreateAsync(
          It.IsAny<DateTimeOffset?>(),
          100,
          It.IsAny<DateTimeOffset>()))
        .ThrowsAsync(new Exception(expectedDetail));

      // Act.
      IActionResult result
        = await _questionnaireController.CreateAsync(
          new CreateQuestionnaireRequest { MaxNumberToCreate = 100});

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task BadRequest()
    {
      // Arrange.
      string expectedDetail = "ToDate must be after 01/04/2022.";
      Mock<IReferralQuestionnaireService> serviceMock = new();
      CreateReferralQuestionnaireResponse response = new()
      {
        NumberOfQuestionnairesCreated = 0,
        NumberOfErrors = 0,
        Status = CreateQuestionnaireStatus.BadRequest
      };

      response.Errors.Add(expectedDetail);

      _referralQuestionnaireServiceMock
        .Setup(x => x.CreateAsync(
          It.IsAny<DateTimeOffset?>(),
          100,
          It.IsAny<DateTimeOffset>()))
        .ReturnsAsync(response);

      // Act.
      IActionResult result =
        await _questionnaireController.CreateAsync(
          new CreateQuestionnaireRequest {MaxNumberToCreate = 100 });

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult badResult = result.As<ObjectResult>();
        badResult.Should().NotBeNull();
        badResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ProblemDetails problemDetails =
          badResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task Conflict()
    {
      // Arrange.
      string expectedDetail = "Service is already running.";
      CreateReferralQuestionnaireResponse response = new()
      {
        NumberOfQuestionnairesCreated = 0,
        NumberOfErrors = 0,
        Status = CreateQuestionnaireStatus.Conflict
      };

      response.Errors.Add(expectedDetail);

      _referralQuestionnaireServiceMock
        .Setup(x => x.CreateAsync(
          It.IsAny<DateTimeOffset?>(),
          100,
          It.IsAny<DateTimeOffset>()))
        .ReturnsAsync(response);

      // Act.
      IActionResult result =
        await _questionnaireController.CreateAsync(
          new CreateQuestionnaireRequest { MaxNumberToCreate = 100 });

      // Assert.
      _referralQuestionnaireServiceMock.Verify(x => 
        x.CreateAsync(
          It.IsAny<DateTimeOffset?>(),
          100,
          It.IsAny<DateTimeOffset>()),
        Times.Once);

      result.Should().NotBeNull().And.BeOfType<ObjectResult>()
        .Subject.StatusCode.Should().Be(StatusCodes.Status409Conflict);

      result.Should().BeOfType<ObjectResult>()
        .Subject.Value.Should().BeOfType<ProblemDetails>()
        .Subject.Detail.Should().Be(expectedDetail);
    }
  }

  public class SendAsync : QuestionnaireControllerTests
  {
    public SendAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task Ok()
    {
      // Arrange.
      SendReferralQuestionnaireResponse response = new()
      {
        NumberOfReferralQuestionnairesSent = 1,
        NumberOfReferralQuestionnairesFailed = 0
      };

      _referralQuestionnaireServiceMock.Setup(
        x => x.SendAsync()).ReturnsAsync(response);

      // Act.
      IActionResult result =
        await _questionnaireController.SendAsync();

      // Assert.
      using (new AssertionScope())
      {
        OkObjectResult outputResult = result.As<OkObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        SendReferralQuestionnaireResponse resultValue =
          outputResult.Value.As<SendReferralQuestionnaireResponse>();
        resultValue.Should().NotBeNull();
        resultValue.NumberOfReferralQuestionnairesSent.Should()
          .Be(response.NumberOfReferralQuestionnairesSent);
        resultValue.NumberOfReferralQuestionnairesFailed.Should()
          .Be(response.NumberOfReferralQuestionnairesFailed);
      }
    }

    [Fact]
    public async Task NoContent()
    {
      // Arrange.
      SendReferralQuestionnaireResponse response = new()
      {
        NumberOfReferralQuestionnairesSent = 0,
        NumberOfReferralQuestionnairesFailed = 0,
        NoQuestionnairesToSend = true
      };

      _referralQuestionnaireServiceMock.Setup(
        x => x.SendAsync()).ReturnsAsync(response);

      // Act.
      IActionResult result =
        await _questionnaireController.SendAsync();

      // Assert.
      using (new AssertionScope())
      {
        NoContentResult outputResult = result.As<NoContentResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
      }
    }

    [Fact]
    public async Task InternalServerError()
    {
      //Arrange.
      string expectedDetail = $"Test Exception: {DateTimeOffset.Now}";

      _referralQuestionnaireServiceMock.Setup(x => x.SendAsync())
        .ThrowsAsync(new Exception(expectedDetail));

      // Act.
      IActionResult result =
        await _questionnaireController.SendAsync();

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }
  }

  public class StartAsync : QuestionnaireControllerTests
  {
    public StartAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task Ok()
    {
      // Arrange.
      StartReferralQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key"
      };

      _referralQuestionnaireServiceMock.Setup(
        x => x.StartAsync(It.IsAny<string>())
      ).ReturnsAsync(new StartReferralQuestionnaire
      {
        ValidationState = ReferralQuestionnaireValidationState.Valid
      });

      // Act.
      IActionResult result =
        await _questionnaireController.StartAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        OkObjectResult outputResult = result.As<OkObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        outputResult.Value.Should().BeOfType<StartReferralQuestionnaire>();
      }
    }

    [Fact]
    public async Task InternalServerError()
    {
      //Arrange.
      string expectedDetail = $"Test Exception: {DateTimeOffset.Now}";
      StartReferralQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key"
      };
      _referralQuestionnaireServiceMock.Setup(
        x => x.StartAsync(It.IsAny<string>())
      ).ThrowsAsync(new Exception(expectedDetail));

      // Act.
      IActionResult result =
        await _questionnaireController.StartAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status500InternalServerError);

        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Be(expectedDetail);
        problemDetails.Status.Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task NotificationKeyNotFound()
    {
      //Arrange.
      StartReferralQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key"
      };

      _referralQuestionnaireServiceMock.Setup(
        x => x.StartAsync(It.IsAny<string>())
      ).ReturnsAsync(new StartReferralQuestionnaire
      {
        ValidationState =
          ReferralQuestionnaireValidationState.NotificationKeyNotFound
      });
      _loggerMock
        .Setup(x => x.Warning(It.IsAny<string>(), It.IsAny<string>()))
        .Verifiable();

      // Act.
      IActionResult result =
        await _questionnaireController.StartAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status404NotFound);

        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status404NotFound);

        _loggerMock
          .Verify(x => x.Warning(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
      }
    }

    [Theory]
    [InlineData(
      ReferralQuestionnaireValidationState.Completed, "Completed")]
    [InlineData(
      ReferralQuestionnaireValidationState.NotDelivered, "NotDelivered")]
    public async Task Conflict(
      ReferralQuestionnaireValidationState validationState,
      string message)
    {
      //Arrange.
      StartReferralQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key"
      };

      _referralQuestionnaireServiceMock.Setup(
        x => x.StartAsync(It.IsAny<string>())
      ).ReturnsAsync(new StartReferralQuestionnaire
      {
        ValidationState = validationState
      });
      _loggerMock
        .Setup(x => x.Warning(It.IsAny<string>(), It.IsAny<string>()))
        .Verifiable();

      // Act.
      IActionResult result =
        await _questionnaireController.StartAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status409Conflict);

        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Type.Should().Be(message);
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status409Conflict);

        _loggerMock
          .Verify(x => x.Warning(It.IsAny<string>(), It.IsAny<string>()),
          Times.Once);
      }
    }

    [Fact]
    public async Task ExpiredConflict()
    {
      //Arrange.
      StartReferralQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key"
      };

      _referralQuestionnaireServiceMock.Setup(
        x => x.StartAsync(It.IsAny<string>())
      ).ReturnsAsync(new StartReferralQuestionnaire
      {
        ValidationState =
          ReferralQuestionnaireValidationState.Expired
      });

      // Act.
      IActionResult result =
        await _questionnaireController.StartAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        ObjectResult outputResult = result.As<ObjectResult>();
        outputResult.Should().NotBeNull();
        outputResult.StatusCode.Should()
          .Be(StatusCodes.Status409Conflict);

        ProblemDetails problemDetails =
          outputResult.Value.As<ProblemDetails>();
        problemDetails.Type.Should().Be("Expired");
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should()
          .Be(StatusCodes.Status409Conflict);
      }
    }
  }

  public class CompleteAsync : QuestionnaireControllerTests
  {
    public CompleteAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Fact]
    public async Task Ok()
    {
      // Arrange.
      CompleteQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      };
      _mapperMock.Setup(x => x.Map<CompleteQuestionnaire>(
        It.IsAny<CompleteQuestionnaireRequest>())
      ).Returns(new CompleteQuestionnaire
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CompleteAsync(It.IsAny<CompleteQuestionnaire>())
      ).ReturnsAsync(new CompleteQuestionnaireResponse
      {
        ValidationState = ReferralQuestionnaireValidationState.Valid
      });

      // Act.
      IActionResult result =
        await _questionnaireController.CompleteAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<OkResult>();
        result.As<OkResult>().StatusCode
          .Should().Be(StatusCodes.Status200OK);
      }
    }

    [Fact]
    public async Task InternalServerError()
    {
      //Arrange.
      string expectedDetail = $"Test Exception: {DateTimeOffset.Now}";
      CompleteQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      };
      _mapperMock.Setup(x => x.Map<CompleteQuestionnaire>(
        It.IsAny<CompleteQuestionnaireRequest>())
      ).Returns(new CompleteQuestionnaire
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      });
      _referralQuestionnaireServiceMock.Setup(x => x.CompleteAsync(
        It.IsAny<CompleteQuestionnaire>())
      ).ThrowsAsync(new Exception(expectedDetail));

      // Act.
      IActionResult result =
        await _questionnaireController.CompleteAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status500InternalServerError);
        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Detail
          .Should().Be(expectedDetail);
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task NotFound()
    {
      //Arrange.
      CompleteQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      };
      _mapperMock.Setup(x => x.Map<CompleteQuestionnaire>(
        It.IsAny<CompleteQuestionnaireRequest>())
      ).Returns(new CompleteQuestionnaire
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CompleteAsync(It.IsAny<CompleteQuestionnaire>())
      ).ReturnsAsync(new CompleteQuestionnaireResponse
      {
        ValidationState =
          ReferralQuestionnaireValidationState.NotificationKeyNotFound
      });
      _loggerMock.Setup(x => x.Warning(It.IsAny<string>()));

      // Act.
      IActionResult result =
        await _questionnaireController.CompleteAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status404NotFound);
        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status404NotFound);
      }
    }

    [Fact]
    public async Task BadRequest()
    {
      //Arrange.
      CompleteQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      };
      _mapperMock.Setup(x => x.Map<CompleteQuestionnaire>(
        It.IsAny<CompleteQuestionnaireRequest>())
      ).Returns(new CompleteQuestionnaire
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CompleteAsync(It.IsAny<CompleteQuestionnaire>())
      ).ReturnsAsync(new CompleteQuestionnaireResponse
      {
        ValidationState =
          ReferralQuestionnaireValidationState.QuestionnaireTypeIncorrect
      });
      _loggerMock.Setup(x => x.Warning(It.IsAny<string>()));

      // Act.
      IActionResult result =
        await _questionnaireController.CompleteAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status400BadRequest);
        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task Conflict()
    {
      //Arrange.
      CompleteQuestionnaireRequest request = new()
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      };
      _mapperMock.Setup(x => x.Map<CompleteQuestionnaire>(
        It.IsAny<CompleteQuestionnaireRequest>())
      ).Returns(new CompleteQuestionnaire
      {
        NotificationKey = "notification key",
        QuestionnaireType = QuestionnaireType.CompleteProT1,
        Answers = "[{ \"Question 1\": \"Answer 1\" }]"
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CompleteAsync(It.IsAny<CompleteQuestionnaire>())
      ).ReturnsAsync(new CompleteQuestionnaireResponse
      {
        ValidationState =
          ReferralQuestionnaireValidationState.IncorrectStatus
      });
      _loggerMock.Setup(x => x.Warning(It.IsAny<string>()));

      // Act.
      IActionResult result =
        await _questionnaireController.CompleteAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status409Conflict);
        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status409Conflict);
      }
    }
  }

  public class CallbackAsync : QuestionnaireControllerTests
  {
    public CallbackAsync(ServiceFixture serviceFixture)
      : base(serviceFixture)
    { }

    [Theory]
    [InlineData(NotificationProxyCallbackRequestStatus.Delivered)]
    [InlineData(NotificationProxyCallbackRequestStatus.TemporaryFailure)]
    [InlineData(NotificationProxyCallbackRequestStatus.TechnicalFailure)]
    [InlineData(NotificationProxyCallbackRequestStatus.PermanentFailure)]
    public async Task Ok(NotificationProxyCallbackRequestStatus status)
    {
      // Arrange.
      NotificationProxyCallbackRequest request = new()
      {
        Id = "id",
        ClientReference = "client reference",
        Status = status,
        StatusAt = DateTime.Now
      };
      _mapperMock.Setup(x => x.Map<NotificationProxyCallback>(
        It.IsAny<NotificationProxyCallbackRequest>())
      ).Returns(new NotificationProxyCallback
      {
        Id = "id",
        ClientReference = "client reference",
        Status = status,
        StatusAt = DateTime.Now
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CallbackAsync(It.IsAny<NotificationProxyCallback>())
      ).ReturnsAsync(NotificationCallbackStatus.Success);

      // Act.
      IActionResult result =
        await _questionnaireController.CallbackAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<OkResult>();
        result.As<OkResult>()
          .StatusCode
          .Should()
          .Be(StatusCodes.Status200OK);
      }
    }

    [Fact]
    public async Task InternalServerError()
    {
      //Arrange.
      string expectedDetail = $"Test Exception: {DateTimeOffset.Now}";
      NotificationProxyCallbackRequest request = new()
      {
        Id = "id",
        ClientReference = "client reference",
        Status = NotificationProxyCallbackRequestStatus.Delivered,
        StatusAt = DateTime.Now
      };
      _mapperMock.Setup(x => x.Map<NotificationProxyCallback>(
        It.IsAny<NotificationProxyCallbackRequest>())
      ).Returns(new NotificationProxyCallback
      {
        Id = "id",
        ClientReference = "client reference",
        Status = NotificationProxyCallbackRequestStatus.Delivered,
        StatusAt = DateTime.Now
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CallbackAsync(It.IsAny<NotificationProxyCallback>())
      ).ThrowsAsync(new Exception(expectedDetail));

      // Act.
      IActionResult result =
        await _questionnaireController.CallbackAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status500InternalServerError);

        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Detail
          .Should()
          .Be(expectedDetail);
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status500InternalServerError);
      }
    }

    [Fact]
    public async Task NotFound()
    {
      // Arrange.
      NotificationProxyCallbackRequest request = new()
      {
        Id = "id",
        ClientReference = "client reference",
        Status = NotificationProxyCallbackRequestStatus.Delivered,
        StatusAt = DateTime.Now
      };
      _mapperMock.Setup(x => x.Map<NotificationProxyCallback>(
        It.IsAny<NotificationProxyCallbackRequest>())
      ).Returns(new NotificationProxyCallback
      {
        Id = "id",
        ClientReference = "client reference",
        Status = NotificationProxyCallbackRequestStatus.Delivered,
        StatusAt = DateTime.Now
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CallbackAsync(It.IsAny<NotificationProxyCallback>())
      ).ReturnsAsync(NotificationCallbackStatus.NotFound);

      // Act.
      IActionResult result =
        await _questionnaireController.CallbackAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status404NotFound);

        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status404NotFound);
      }
    }

    [Fact]
    public async Task BadRequest()
    {
      // Arrange.
      NotificationProxyCallbackRequest request = new()
      {
        Id = "id",
        ClientReference = "client reference",
        Status = NotificationProxyCallbackRequestStatus.Delivered,
        StatusAt = DateTime.Now
      };
      _mapperMock.Setup(x => x.Map<NotificationProxyCallback>(
        It.IsAny<NotificationProxyCallbackRequest>())
      ).Returns(new NotificationProxyCallback
      {
        Id = "id",
        ClientReference = "client reference",
        Status = NotificationProxyCallbackRequestStatus.Delivered,
        StatusAt = DateTime.Now
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CallbackAsync(It.IsAny<NotificationProxyCallback>())
      ).ReturnsAsync(NotificationCallbackStatus.BadRequest);

      // Act.
      IActionResult result =
        await _questionnaireController.CallbackAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status400BadRequest);

        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status400BadRequest);
      }
    }

    [Fact]
    public async Task Unknown()
    {
      // Arrange.
      NotificationProxyCallbackRequest request = new()
      {
        Id = "id",
        ClientReference = "client reference",
        StatusAt = DateTime.Now
      };
      _mapperMock.Setup(x => x.Map<NotificationProxyCallback>(
        It.IsAny<NotificationProxyCallbackRequest>())
      ).Returns(new NotificationProxyCallback
      {
        Id = "id",
        ClientReference = "client reference",
        StatusAt = DateTime.Now
      });
      _referralQuestionnaireServiceMock.Setup(
        x => x.CallbackAsync(It.IsAny<NotificationProxyCallback>())
      ).ReturnsAsync(NotificationCallbackStatus.Unknown);

      // Act.
      IActionResult result =
        await _questionnaireController.CallbackAsync(request);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeOfType<ObjectResult>();
        result.As<ObjectResult>().StatusCode
          .Should()
          .Be(StatusCodes.Status400BadRequest);
        result.As<ObjectResult>().Value
          .Should()
          .BeOfType<ProblemDetails>();
        result.As<ObjectResult>().Value
          .As<ProblemDetails>().Status
          .Should()
          .Be(StatusCodes.Status400BadRequest);
      }
    }
  }
}
