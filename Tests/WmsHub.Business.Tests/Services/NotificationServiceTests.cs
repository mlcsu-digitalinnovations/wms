using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;
using WmsHub.Business.Services;
using Xunit;

namespace WmsHub.Business.Tests.Services;

[Collection("Service collection")]
public class NotificationServiceTests
  : ServiceTestsBase, IDisposable
{
  private readonly DatabaseContext _context;
  protected readonly IConfiguration _configuration;
  protected readonly Mock<ILogger> _loggerMock;
  private  HttpClient _httpClient;
  private readonly INotificationService _notificationService;
  private  Mock<HttpMessageHandler> _httpMessageHandlerMock;
  private Mock<IOptions<NotificationOptions>> _mockOptions = new();
  private Mock<NotificationOptions> _notificationOptionsMock = new();

  public NotificationServiceTests(ServiceFixture serviceFixture) 
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
    _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
    _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
    {
      BaseAddress = new Uri("http://test.com/")
    };

    _notificationOptionsMock.Object.NotificationApiUrl =
      "https://localtest.com";
    _notificationOptionsMock.Object.Endpoint = "test";

    _mockOptions.Setup(t => t.Value).Returns(_notificationOptionsMock.Object);

    _notificationService = new NotificationService(
      _context,
      _loggerMock.Object,
      _httpClient,
      _mockOptions.Object);
  }

  [Fact]
  public void WhenOptionsIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      string expectedMessage = 
        "Value cannot be null. (Parameter 'IOptions is null.')";

      // Act.
      Action action = () =>
      {
        NotificationService service = new(
        _context,
        _loggerMock.Object,
        new HttpClient(),
        null);
      };

      // Assert.
      action.Should()
        .Throw<ArgumentNullException>()
        .WithMessage(expectedMessage);
    }
  }

  [Fact]
  public void WhenNotificationOptionsIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      string expectedMessage =
        "Value cannot be null. (Parameter 'NotificationOptions is null.')";
      _mockOptions.Setup(t=>t.Value).Returns((NotificationOptions)null);

      // Act.
      Action action = () =>
      {
        NotificationService service = new(
          _context,
          _loggerMock.Object,
          new HttpClient(),
          _mockOptions.Object);
      };

      // Assert.
      action.Should()
        .Throw<ArgumentNullException>()
        .WithMessage(expectedMessage);
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
        NotificationService service = new(
          _context,
          null,
          new HttpClient(),
          _mockOptions.Object);
      };

      // Assert.
      action.Should().Throw<ArgumentNullException>();
    }
  }

  [Fact]
  public void WhenHttpClientIsNullShouldThrowArgumentNullException()
  {
    using (new AssertionScope())
    {
      // Arrange.
      // Act.
      Action action = () =>
      {
        NotificationService service = new(
          _context,
          _loggerMock.Object,
          null,
          _mockOptions.Object);
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

  public class GetEmailHistory(ServiceFixture serviceFixture)
    : NotificationServiceTests(serviceFixture)
  {
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task NotificationProxyFailureLogsErrorAndThrowsException(
      HttpStatusCode statusCode)
    {
      // Arrange.
      string clientReference = Guid.NewGuid().ToString();
      HttpResponseMessage notificationProxyResponse = new(statusCode);

      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(notificationProxyResponse);

      // Act.
      Func<Task<HttpResponseMessage>> result = 
        () => _notificationService.GetEmailHistory(clientReference);

      // Assert.
      await result.Should().ThrowAsync<NotificationProxyException>();
      _loggerMock.Verify(x => x.Error(It.IsAny<string>()));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task NotificationProxySuccessOrNotFoundReturnsHttpResponseMessage(
      HttpStatusCode statusCode)
    {
      // Arrange.
      string clientReference = Guid.NewGuid().ToString();
      string expectedUri = $"{_notificationOptionsMock.Object.NotificationApiUrl}/email" +
        $"?clientReference={clientReference}";

      HttpResponseMessage notificationProxyResponse = new(statusCode)
      {
        Content = new StringContent("Expected content")
      };

      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.Is<HttpRequestMessage>(x => x.RequestUri == new Uri(expectedUri)),
          ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(notificationProxyResponse);

      // Act.
      HttpResponseMessage response = await _notificationService.GetEmailHistory(clientReference);

      // Assert.
      response.Should().Be(notificationProxyResponse);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public async Task NullOrWhiteSpaceClientReferenceThrowsException(string clientReference)
    {
      // Arrange.

      // Act.
      Func<Task<HttpResponseMessage>> result =
        () => _notificationService.GetEmailHistory(clientReference);

      // Assert.
      await result.Should().ThrowAsync<ArgumentException>();
    }
  }

  public class SendNotificationAsync : NotificationServiceTests
  {
    public SendNotificationAsync(ServiceFixture serviceFixture) 
      : base(serviceFixture)
    {
    }

    [Fact]
    public async Task WhenSendNotificationCalledWithNullRequest()
    {
      // Arrange.
      INotificationService service = new NotificationService(
        _context,
        _loggerMock.Object, 
        new HttpClient(),
      _mockOptions.Object);

      // Assert.
      using (new AssertionScope())
      {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        await service.SendNotificationAsync(null));
      }
    }

    [Fact]
    public async Task 
      WhenRequestHavingNullClientReference()
    {
      // Arrange.
      INotificationService service = new NotificationService(
        _context,
        _loggerMock.Object, 
        new HttpClient(),
      _mockOptions.Object);

      // Assert.
      using (new AssertionScope())
      {
         await Assert.ThrowsAsync<ValidationException>(async () => 
          await service.SendNotificationAsync(new SmsPostRequest 
          { 
            ClientReference = null,
            Mobile = "+447595000000",
            SenderId = "Sender Id",
            TemplateId = "Template Id" 
          }));
      }
    }

    [Fact]
    public async Task
      WhenRequestClientReferenceGreaterThan200()
    {
      // Arrange.
      INotificationService service = new NotificationService(
        _context,
        _loggerMock.Object, 
        new HttpClient(),
      _mockOptions.Object);

      // Assert.
      using (new AssertionScope())
      {
        await Assert.ThrowsAsync<ValidationException>(async () =>
          await service.SendNotificationAsync(new SmsPostRequest
          {
            ClientReference = "ClientReferenceClientReferenceClientReference" +
              "ClientReferenceClientReferenceClientReferenceClientReference" +
              "ClientReferenceClientReferenceClientReferenceClientReference" +
              "ClientReferenceClientReferenceClientReferenceClientReference" +
              "ClientReferenceClientReferenceClientReferenceClientReference",
            Mobile = "+447595000000",
            SenderId = "Sender Id",
            TemplateId = "Template Id"
          }));
      }
    }

    [Fact]
    public async Task
      WhenRequestHavingNullMobileNumber()
    {
      // Arrange.
      INotificationService service = new NotificationService(
        _context,
        _loggerMock.Object, 
        new HttpClient(), 
        _mockOptions.Object);

      // Assert.
      using (new AssertionScope())
      {
        await Assert.ThrowsAsync<ValidationException>(async () =>
          await service.SendNotificationAsync(new SmsPostRequest
          {
            ClientReference = "Client Reference",
            Mobile = null,
            SenderId = "Sender Id",
            TemplateId = "Template Id"
          }));
      }
    }

    [Fact]
    public async Task WhenSending()
    {
      // Arrange.
      _httpMessageHandlerMock
        .Protected()      
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          Content = new StringContent("{ \"status\": \"Created\" }"),
          StatusCode = HttpStatusCode.Created
        });

      // Act.
      SmsPostResponse response = await _notificationService
        .SendNotificationAsync(new SmsPostRequest
        {
          ClientReference = Guid.NewGuid().ToString(),
          Mobile = "+447000000001",
          TemplateId = "5087F0AB-0459-4566-BDFE-50089CA84976",
          Personalisation = new Dictionary<string, dynamic>
          {
            { "givenName", "GivenName" },
            { "link", $"link/NotificationKey" }
          }
        });

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.ResponseStatus
          .Should()
          .Be(Enums.ReferralQuestionnaireStatus.Sending);
      }
    }

    [Fact]
    public async Task WhenTooManyRequests()
    {
      // Arrange.
      _httpMessageHandlerMock
       .Protected()
       .Setup<Task<HttpResponseMessage>>(
           "SendAsync",
           ItExpr.IsAny<HttpRequestMessage>(),
           ItExpr.IsAny<CancellationToken>())
       .ReturnsAsync(new HttpResponseMessage
       {
         Content = new StringContent("{ \"status\": \"Rate limit\" }"),
         StatusCode = HttpStatusCode.TooManyRequests
       });

      // Act.
      SmsPostResponse response = await _notificationService
        .SendNotificationAsync(new SmsPostRequest
        {
          ClientReference = Guid.NewGuid().ToString(),
          Mobile = "+447000004291",
          TemplateId = "5087F0AB-0459-4566-BDFE-50089CA84976",
          Personalisation = new Dictionary<string, dynamic>
          {
            { "givenName", "GivenName" },
            { "link", $"link/NotificationKey" }
          }
        });

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.ResponseStatus
          .Should()
          .Be(Enums.ReferralQuestionnaireStatus.TechnicalFailure);
      }
    }

    [Fact]
    public async Task WhenInternalServerError()
    {
      // Arrange.
      _httpMessageHandlerMock
       .Protected()
       .Setup<Task<HttpResponseMessage>>(
           "SendAsync",
           ItExpr.IsAny<HttpRequestMessage>(),
           ItExpr.IsAny<CancellationToken>())
       .ReturnsAsync(new HttpResponseMessage
       {
         Content = 
          new StringContent("{ \"status\": \"InternalServerError\" }"),
         StatusCode = HttpStatusCode.InternalServerError
       });

      // Act.
      SmsPostResponse response = await _notificationService
        .SendNotificationAsync(new SmsPostRequest
        {
          ClientReference = Guid.NewGuid().ToString(),
          Mobile = "+447000005001",
          TemplateId = "5087F0AB-0459-4566-BDFE-50089CA84976",
          Personalisation = new Dictionary<string, dynamic>
          {
            { "givenName", "GivenName" },
            { "link", $"link/NotificationKey" }
          }
        });

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.ResponseStatus
          .Should()
          .Be(Enums.ReferralQuestionnaireStatus.TechnicalFailure);
      }
    }

    [Fact]
    public async Task WhenBadRequest()
    {
      // Arrange.
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          Content = new StringContent(
            "{ \"errors\":{\"Mobile\": " + 
            "[\"The Mobile field is not a valid UK mobile phone number.\"]} }"
          ),
          StatusCode = HttpStatusCode.BadRequest
        });

      // Act.
      SmsPostResponse response = await _notificationService
        .SendNotificationAsync(new SmsPostRequest
        {
          ClientReference = "Client Reference",
          Mobile = "07595000000",
          SenderId = Guid.NewGuid().ToString(),
          TemplateId = Guid.NewGuid().ToString(),
          Personalisation = new Dictionary<string, dynamic>()
        });

      // Assert.
      using (new AssertionScope())
      {
        response.Should().NotBeNull();
        response.ResponseStatus
          .Should()
          .Be(Enums.ReferralQuestionnaireStatus.PermanentFailure);
        response.GetNotificationErrors[0]
          .Should()
          .Be("The Mobile field is not a valid UK mobile phone number.");
      }
    }
  }

  public class SendEmailMessageAsyncTests : NotificationServiceTests
  {
    public SendEmailMessageAsyncTests(ServiceFixture serviceFixture) 
      : base(serviceFixture)
    {
    }

    public class ValidationResults_isValid_False : SendEmailMessageAsyncTests
    {
      private NotificationService _classToTest;
      private NotificationOptions _options = new ();

      public ValidationResults_isValid_False(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpMessageHandlerMock
       .Protected()
       .Setup<Task<HttpResponseMessage>>(
           "SendAsync",
           ItExpr.IsAny<HttpRequestMessage>(),
           ItExpr.IsAny<CancellationToken>())
       .ReturnsAsync(new HttpResponseMessage
       {
         Content = new StringContent(
           "{ \"errors\":{\"ExpectedException\": " +
           "[\"Expected ValidationException.\"]} }"
         ),
         StatusCode = HttpStatusCode.BadRequest
       });

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
          BaseAddress = new Uri("http://test.com/")
        };

        _options.NotificationApiUrl = "https://test.com";
        _mockOptions.Setup(t=>t.Value).Returns(_options);

        _classToTest = new NotificationService(
          _context,
          _loggerMock.Object,
          _httpClient,
          _mockOptions.Object);
      }

      [Fact]
      public async Task ClientReference_is_null()
      {
        // Arrange.
        string expectedErrorMessage = "ClientReference cannot be null.";
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          templateId: Guid.NewGuid().ToString());
        message.ClientReference = null;

        // Act.
        Func<Task> act = async () => 
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using(new AssertionScope())
        {
          await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedErrorMessage);
        }
      }

      [Fact]
      public async Task ClientReference_Guid_Is_Empty()
      {
        // Arrange.
        string expectedErrorMessage =
          "ClientReference cannot have an empty Guid.";
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          clientReference: Guid.Empty.ToString(),
          templateId: Guid.NewGuid().ToString());

        // Act.
        Func<Task> act = async () =>
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedErrorMessage);
        }
      }

      [Fact]
      public async Task ClientReference_is_Not_Guid()
      {
        // Arrange.
        string expectedErrorMessage =
          "ClientReference cannot be null.";
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          clientReference: "Not Guid",
          templateId: Guid.NewGuid().ToString());

        // Act.
        Func<Task> act = async () =>
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedErrorMessage);
        }
      }

      [Fact]
      public async Task TempaleteId_Guid_Is_Empty()
      {
        // Arrange.
        string expectedErrorMessage =
          "TemplateId cannot have an empty Guid.";
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          templateId: Guid.Empty.ToString());

        // Act.
        Func<Task> act = async () =>
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedErrorMessage);
        }
      }

      [Fact]
      public async Task Contact_is_null()
      {
        // Arrange.
        string expectedErrorMessage =
          "EmailTo field is required.";
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          templateId: Guid.NewGuid().ToString());
        message.EmailTo = null;

        // Act.
        Func<Task> act = async () =>
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedErrorMessage);
        }
      }

      [Fact]
      public async Task Contact_is_Not_Email()
      {
        // Arrange.
        string expectedErrorMessage =
          "The EmailTo field is not a valid e-mail address.";
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          templateId: Guid.NewGuid().ToString());
        message.EmailTo = "Peter at Home";

        // Act.
        Func<Task> act = async () =>
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedErrorMessage);
        }
      }


      [Fact]
      public async Task Personalisation_is_empty()
      {
        // Arrange.
        string expectedErrorMessage =
          "Personalisation does not contain any values.";
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          templateId: Guid.NewGuid().ToString());
        message.Personalisation = new Dictionary<string, dynamic>();

        // Act.
        Func<Task> act = async () =>
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          await act.Should().ThrowAsync<ValidationException>()
            .WithMessage(expectedErrorMessage);
        }
      }


    }

    public class ValidationResults_IsValid_True : SendEmailMessageAsyncTests
    {
      private NotificationService _classToTest;
      private NotificationOptions _options = new();

      public ValidationResults_IsValid_True(ServiceFixture serviceFixture)
        : base(serviceFixture)
      {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
          BaseAddress = new Uri("http://test.com/")
        };

        _options.NotificationApiUrl = "https://test.com";
        _mockOptions.Setup(t => t.Value).Returns(_options);

        _classToTest = new NotificationService(
          _context,
          _loggerMock.Object,
          _httpClient,
          _mockOptions.Object);
      }

      [Fact]
      public async Task BadRequest_NotficationProxyEmailException()
      {
        // Arrange.
        string expectedErrorMessage =
          "{ \"errors\":{\"ExpectedException\": [" +
          "\"Expected ValidationException.\"]} }";
        _httpMessageHandlerMock
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            Content = new StringContent(expectedErrorMessage),
            StatusCode = HttpStatusCode.BadRequest
          });
        
        string ExpectedException = typeof(ValidationException).Name;
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          clientReference: Guid.NewGuid().ToString(), 
          templateId: Guid.NewGuid().ToString());

        // Act.
        Func<Task> act = async () =>
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          await act.Should().ThrowAsync<NotificationProxyException>()
            .WithMessage(expectedErrorMessage);
        }
      }

      [Fact]
      public async Task Success_HttpReponse()
      {
        // Arrange.
        string expectedMessage = "Test was successfull.";
        _httpMessageHandlerMock
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            Content = new StringContent(expectedMessage),
            StatusCode = HttpStatusCode.OK
          });
        
        MessageQueue message = ServiceFixture.CreateMessageQueue(
          clientReference: Guid.NewGuid().ToString(), 
          templateId: Guid.NewGuid().ToString());

        // Act.
        HttpResponseMessage response =
          await _classToTest.SendMessageAsync(message);

        // Assert.
        using (new AssertionScope())
        {
          response.StatusCode.Should().Be(HttpStatusCode.OK);
          string result = await response.Content.ReadAsStringAsync();
          result.Should().Be(expectedMessage);
        }
      }
    }
  }
}
