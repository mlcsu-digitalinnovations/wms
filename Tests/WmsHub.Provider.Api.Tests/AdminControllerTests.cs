using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ProviderRejection;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Provider.Api.Controllers;
using Xunit;

namespace WmsHub.ProviderApi.Tests
{
  public class AdminControllerTests : TestSetup
  {
    private Business.Models.Provider _provider;
    private AdminController _classToTest;
    private Mock<ProviderAdminResponse> _mockProviderAdminResponse;
    private IEnumerable<Business.Models.Provider> _emptyProviders =
            new List<Business.Models.Provider>();
    private IEnumerable<ProviderRequest> _validProviderRequsts;
    

    public AdminControllerTests()
    {
      _classToTest = new AdminController(_mockService.Object);
      _provider = new Business.Models.Provider
      {
        Id = _providerId,
        IsActive = true,
        Level1 = true,
        Level2 = true,
        Level3 = true,
        Name = "Test",
        Summary = "Test",
        Website = "Test",
        Logo = "Test",
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId = _providerId
      };
      _validProviderRequsts =
         new List<ProviderRequest>
         {
          new ProviderRequest
          {
            Id = _provider.Id,
            Level1 = _provider.Level1,
            Level2 = _provider.Level2,
            Level3 = _provider.Level3,
            Logo = _provider.Logo,
            Name = _provider.Name,
            Summary = _provider.Summary,
            Summary2 = _provider.Summary2,
            Summary3 = _provider.Summary3,
            Website = _provider.Website
          }
         };

      _mockProviderAdminResponse =
        new Mock<ProviderAdminResponse>();
      _mockProviderAdminResponse.Object.Providers = _validProviderRequsts;

      _mockProviderAdminResponse.Setup(x => x.ResponseStatus)
        .Returns(Business.Enums.StatusType.Valid);
      _mockProviderAdminResponse.Setup(x => x.Errors)
        .Returns(new List<string>());
    }

    public class AdminUserAllowed : AdminControllerTests
    {
      [Fact]
      public async Task UserIsAdminGetReturnOk()
      {
        //Arrange
        int expected = 200;
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
          .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                      new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
                      new Claim(ClaimTypes.Name, "Provider_admin")
                      // other required and custom claims
                  }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
                                    new DefaultHttpContext { User = user };
        //Act
        var response = await _classToTest.Get();
        //Assert
        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);
        OkObjectResult result =
                      response as OkObjectResult;
        Assert.Equal(expected, result.StatusCode);
      }

      [Fact]
      public async Task UserIsProviderGetReturnNUnauthorised()
      {
        //Arrange
        int expected = 401;
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                            new Claim(ClaimTypes.NameIdentifier, "Provider"),
                            new Claim(ClaimTypes.Name, "Provider")
                            // other required and custom claims
                        }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
                                    new DefaultHttpContext { User = user };

        //Act

        var response = await _classToTest.Get();

        //Assert
        Assert.NotNull(response);
        Assert.IsType<UnauthorizedObjectResult>(response);
        UnauthorizedObjectResult result =
                      response as UnauthorizedObjectResult;
        Assert.Equal(expected, result.StatusCode);
      }

      [Fact]
      public async Task UserIsAdminGetKeyUpdateReturnOk()
      {
        //Arrange
        int expected = 200;
        Mock<ProviderResponse> mockResponse = 
          new Mock<ProviderResponse>();

        mockResponse.Object.Id = _provider.Id;
        mockResponse.Object.Name = _provider.Name;
        mockResponse.Object.Level1 = _provider.Level1;
        mockResponse.Object.Level2 = _provider.Level2;
        mockResponse.Object.Level3 = _provider.Level3;
        mockResponse.Object.Summary = _provider.Summary;
        mockResponse.Object.Summary2 = _provider.Summary2;
        mockResponse.Object.Summary3 = _provider.Summary3;
        mockResponse.Object.Logo = _provider.Logo;
        mockResponse.Object.Website = _provider.Website;

        Mock<NewProviderApiKeyResponse> mockUpdateResponse =
          new Mock<NewProviderApiKeyResponse>();

        mockUpdateResponse.Object.Id = _provider.Id;
        mockUpdateResponse.Object.Name = _provider.Name;
        mockUpdateResponse.Object.Level1 = _provider.Level1;
        mockUpdateResponse.Object.Level2 = _provider.Level2;
        mockUpdateResponse.Object.Level3 = _provider.Level3;
        mockUpdateResponse.Object.Summary = _provider.Summary;
        mockUpdateResponse.Object.Summary2 = _provider.Summary2;
        mockUpdateResponse.Object.Summary3 = _provider.Summary3;
        mockUpdateResponse.Object.Logo = _provider.Logo;
        mockUpdateResponse.Object.Website = _provider.Website;

        mockUpdateResponse.Setup(x => x.ResponseStatus)
          .Returns(Business.Enums.StatusType.Valid);
        
        mockUpdateResponse.Setup(x => x.Errors)
        .Returns(new List<string>());

        _mockService.Setup(x => x.GetProviderAsync(_providerId))
          .Returns(Task.FromResult(mockResponse.Object));

        _mockService.Setup(x => x.UpdateProviderKeyAsync
        (mockResponse.Object, It.IsAny<int>()))
          .Returns(Task.FromResult(mockUpdateResponse.Object));

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                      new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
                      new Claim(ClaimTypes.Name, "Provider_admin")
                      // other required and custom claims
                  }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
                                    new DefaultHttpContext { User = user };
        //Act
        var response = await _classToTest.GetKeyUpdate(_providerId);
        //Assert
        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);
        OkObjectResult result =
                      response as OkObjectResult;
        Assert.Equal(expected, result.StatusCode);
      }

      [Fact]
      public async Task UserIsProviderGetKeyUpdateReturnNUnauthorised()
      {
        //Arrange
        int expected = 401;
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                            new Claim(ClaimTypes.NameIdentifier, "Provider"),
                            new Claim(ClaimTypes.Name, "Provider")
                            // other required and custom claims
                        }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
                                    new DefaultHttpContext { User = user };

        //Act

        var response = await _classToTest.GetKeyUpdate(Guid.NewGuid());

        //Assert
        Assert.NotNull(response);
        Assert.IsType<UnauthorizedObjectResult>(response);
        UnauthorizedObjectResult result =
                      response as UnauthorizedObjectResult;
        Assert.Equal(expected, result.StatusCode);
      }
    }

    public class InstatiationTests : AdminControllerTests
    {
      [Fact]
      public async Task ProviderServiceIsNull()
      {

        //arrange
        int expected = 400;
        _classToTest = new AdminController(null);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                      new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
                      new Claim(ClaimTypes.Name, "Provider_admin")
                      // other required and custom claims
                  }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
                                    new DefaultHttpContext { User = user };
        //act
        var response = await _classToTest.Get();
        //Assert
        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
        BadRequestObjectResult result =
                      response as BadRequestObjectResult;
        Assert.Equal(expected, result.StatusCode);

      }
    }

    public class GetTests : AdminControllerTests
    {
      [Fact]
      public async Task NoProvidersStatusTypeNoRowsReturned()
      {
        //Arrange
        int expected = 200;
        _mockProviderAdminResponse =
          new Mock<ProviderAdminResponse>();

        _mockProviderAdminResponse.Setup(x => x.ResponseStatus)
          .Returns(Business.Enums.StatusType.NoRowsReturned);
        _mockProviderAdminResponse.Setup(x => x.Errors)
          .Returns(new List<string>());

        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
          .Returns(Task.FromResult(_mockProviderAdminResponse.Object));

        _classToTest = new AdminController(_mockService.Object);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                      new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
                      new Claim(ClaimTypes.Name, "Provider_admin")
                      // other required and custom claims
                  }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
            new DefaultHttpContext { User = user };
        //Act
        var response = await _classToTest.Get();
        //Assert
        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);
        OkObjectResult result =
                      response as OkObjectResult;
        Assert.Equal(expected, result.StatusCode);
      }

      [Fact]
      public async Task ProvidersNullStatusTypeInvalid()
      {
        //Arrange
        int expected = 400;
        _mockProviderAdminResponse =
        new Mock<ProviderAdminResponse>(null);
        var user = 
          new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
              new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
              new Claim(ClaimTypes.Name, "Provider_admin")
              // other required and custom claims
          }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
            new DefaultHttpContext
            {
              User = user
            };
        //Act
        var response = await _classToTest.Get();
        //Assert
        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
        BadRequestObjectResult result =
                      response as BadRequestObjectResult;
        Assert.Equal(expected, result.StatusCode);
      }
    }

    public class GetKeyUpdate : AdminControllerTests
    {
      [Fact]
      public async Task ProviderIsNull()
      {
        //Arrange
        string error = "Provider was null";
        int expected = 404;
        var providerResponse = new ProviderResponse
        {
          ResponseStatus =  StatusType.ProviderNotFound,
          Errors = new List<string> { error }
        };

        _mockService.Setup(x => x.GetProviderAsync(It.IsAny<Guid>()))
          .Returns(Task.FromResult(providerResponse));


        var user = 
          new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
              new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
              new Claim(ClaimTypes.Name, "Provider_admin")
              // other required and custom claims
          }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
                                    new DefaultHttpContext { User = user };
        //Act
        var response = await _classToTest.GetKeyUpdate(Guid.NewGuid());
        //Assert
        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
        BadRequestObjectResult result =
                      response as BadRequestObjectResult;
        var detail = ((result.Value as ObjectResult)
          .Value as ProblemDetails).Detail;

        var status = ((result.Value as ObjectResult)
          .Value as ProblemDetails).Status??0;
        Assert.Equal(expected, status);
        Assert.Equal(error, detail);
      }

      [Fact]
      public async Task GetProviderException()
      {
        //Arrange
        string error = "Test Exception";
        int expected = 400;
       
        _mockService.Setup(x => x.GetProviderAsync(It.IsAny<Guid>()))
          .Throws(new ArgumentException(error));


        var user = 
          new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
              new Claim(ClaimTypes.NameIdentifier, "Provider_admin"),
              new Claim(ClaimTypes.Name, "Provider_admin")
              // other required and custom claims
          }, "TestAuthentication"));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
                                    new DefaultHttpContext { User = user };
        //Act
        var response = await _classToTest.GetKeyUpdate(Guid.NewGuid());
        //Assert
        Assert.NotNull(response);
        Assert.IsType<BadRequestObjectResult>(response);
        BadRequestObjectResult result =
                      response as BadRequestObjectResult;
        var returnError = JObject.FromObject(result.Value);
        Assert.Equal(expected,result.StatusCode);
        Assert.Equal(error, returnError["message"].ToString());
      }

    }

    public class PutTests : AdminControllerTests
    {
      [Fact]
      public async Task NotProviderAdmin_Return_UnauthorizedObjectResult()
      {
        //Arrange
        int expected = 401;
        ProviderRequest providerRequest = new ProviderRequest();
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.Put(providerRequest);
        //Assert
        response.Should().NotBeNull();
        Assert.IsType<UnauthorizedObjectResult>(response);
        UnauthorizedObjectResult result =
          response as UnauthorizedObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Theory]
      [InlineData(StatusType.NotAuthorised, 
        typeof(UnauthorizedObjectResult), 401)]
      [InlineData(StatusType.CallIdDoesNotExist, 
        typeof(NotFoundResult), 404)]
      [InlineData(StatusType.OutcomeIsUnknown, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.TelephoneNumberMismatch, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.ProviderUpdateFailed, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.Invalid, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.ProviderNotFound, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.UnableToFindReferral, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.NoRowsUpdated, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.StatusIsUnknown, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.NoRowsReturned, 
        typeof(OkObjectResult), 200)]
      [InlineData(StatusType.Created, 
        typeof(CreatedAtRouteResult), 201)]
      [InlineData(StatusType.Valid, 
        typeof(OkObjectResult), 200)]
      public async Task UpdateProvider_Return_Response(
        StatusType statusType, Type type, int expected)
      {
        //Arrange
        Business.Models.Provider provider = new Business.Models.Provider()
        {
          Id = Guid.NewGuid()
        };
        ProviderRequest request = new ProviderRequest();
        Mock<ProviderResponse> mockResponse =
          new Mock<ProviderResponse>();

        mockResponse.Object.Id = provider.Id;
        mockResponse.Object.Name = provider.Name;
        mockResponse.Object.Level1 = provider.Level1;
        mockResponse.Object.Level2 = provider.Level2;
        mockResponse.Object.Level3 = provider.Level3;
        mockResponse.Object.Summary = provider.Summary;
        mockResponse.Object.Summary2 = provider.Summary2;
        mockResponse.Object.Summary3 = provider.Summary3;
        mockResponse.Object.Logo = provider.Logo;
        mockResponse.Object.Website = provider.Website;


        mockResponse.Setup(t => t.Errors).Returns(new EditableList<string>());
        mockResponse.Setup(t => t.ResponseStatus).Returns(statusType);
        
        _mockService
         .Setup(t => t.UpdateProvidersAsync(It.IsAny<ProviderRequest>()))
         .Returns(Task.FromResult(mockResponse.Object));
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
        //Act
        var response = await _classToTest.Put(request);
        //Assert
        response.Should().NotBeNull();
        Type t = response.GetType();
        t.Should().Be(type);
        t.UnderlyingSystemType.GetProperty("StatusCode").GetValue(response)
         .Should().Be(expected);
      }

      [Fact]
      public async Task UpdateProvider_Return_InternalServerError()
      {
        //Arrange
        int expected = 500;
        ProviderRequest request = new ProviderRequest();

        _mockService
         .Setup(t => t.UpdateProvidersAsync(
            It.IsAny<ProviderRequest>()))
         .Throws(new ArgumentException("This is a test"));

        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
        //Act
        var response = await _classToTest.Put(request);
        //Assert
        response.Should().NotBeNull();
        Type t = response.GetType();
        response.Should().BeOfType<ObjectResult>();
        t.UnderlyingSystemType.GetProperty("StatusCode").GetValue(response)
         .Should().Be(expected);
      }

      [Fact]
      public async Task 
        UpdateProvider_ProviderNotFoundException_Return_InternalServerError()
      {
        //Arrange
        int expected = 500;
        ProviderRequest request = new ProviderRequest();

        _mockService
         .Setup(t => t.UpdateProvidersAsync(
            It.IsAny<ProviderRequest>()))
         .Throws(new ProviderNotFoundException("This is a test"));

        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
        //Act
        var response = await _classToTest.Put(request);
        //Assert
        response.Should().NotBeNull();
        Type t = response.GetType();
        response.Should().BeOfType<ObjectResult>();
        t.UnderlyingSystemType.GetProperty("StatusCode").GetValue(response)
         .Should().Be(expected);
      }
    }

    public class PutStatusTests : AdminControllerTests
    {
      [Fact]
      public async Task NotProviderAdmin_Return_UnauthorizedObjectResult()
      {
        //Arrange
        int expected = 401;
        ProviderLevelStatusChangeRequest request =
          new ProviderLevelStatusChangeRequest
          {
            Id=Guid.NewGuid(),
            Level1 = true,
            Level2 = true,
            Level3 = true
          };
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.PutStatus(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UnauthorizedObjectResult>();
        UnauthorizedObjectResult result =
          response as UnauthorizedObjectResult;
        result.StatusCode.Should().Be(expected);
      }

      [Theory]
      [InlineData(StatusType.NotAuthorised, 
        typeof(UnauthorizedObjectResult), 401)]
      [InlineData(StatusType.CallIdDoesNotExist, 
        typeof(NotFoundResult), 404)]
      [InlineData(StatusType.OutcomeIsUnknown, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.TelephoneNumberMismatch, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.ProviderUpdateFailed, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.Invalid, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.ProviderNotFound, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.UnableToFindReferral, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.NoRowsUpdated, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.StatusIsUnknown, 
        typeof(BadRequestObjectResult), 400)]
      [InlineData(StatusType.NoRowsReturned, 
        typeof(OkObjectResult), 200)]
      [InlineData(StatusType.Created, 
        typeof(CreatedAtRouteResult), 201)]
      [InlineData(StatusType.Valid, 
        typeof(OkObjectResult), 200)]
      public async Task UpdateProviderLevel_Return_Response(
        StatusType statusType, Type type, int expected)
      {
        //Arrange
        Business.Models.Provider provider = new Business.Models.Provider()
        {
          Id = Guid.NewGuid()
        };
        ProviderLevelStatusChangeRequest request =
          new ProviderLevelStatusChangeRequest()
          {
            Id = provider.Id,
            Level1 = true,
            Level2 = true,
            Level3 = true
          };
        Mock<ProviderResponse> mockResponse =
          new Mock<ProviderResponse>();

        mockResponse.Object.Id = provider.Id;
        mockResponse.Object.Name = provider.Name;
        mockResponse.Object.Level1 = provider.Level1;
        mockResponse.Object.Level2 = provider.Level2;
        mockResponse.Object.Level3 = provider.Level3;
        mockResponse.Object.Summary = provider.Summary;
        mockResponse.Object.Summary2 = provider.Summary2;
        mockResponse.Object.Summary3 = provider.Summary3;
        mockResponse.Object.Logo = provider.Logo;
        mockResponse.Object.Website = provider.Website;

        mockResponse.Setup(t => t.Errors).Returns(new EditableList<string>());
        mockResponse.Setup(t => t.ResponseStatus).Returns(statusType);

        _mockService
         .Setup(t => t.UpdateProviderLevelsAsync(
            It.IsAny<ProviderLevelStatusChangeRequest>()))
         .Returns(Task.FromResult(mockResponse.Object));
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
        //Act
        var response = await _classToTest.PutStatus(request);
        //Assert
        response.Should().NotBeNull();
        Type t = response.GetType();
        t.Should().Be(type);
        t.UnderlyingSystemType.GetProperty("StatusCode").GetValue(response)
         .Should().Be(expected);
      }

      [Fact]
      public async Task UpdateProviderLevel_Return_InternalServerError()
      {
        //Arrange
        int expected = 500;
        Business.Models.Provider provider = new Business.Models.Provider()
        {
          Id = Guid.NewGuid()
        };
        ProviderLevelStatusChangeRequest request =
          new ProviderLevelStatusChangeRequest()
          {
            Id = provider.Id,
            Level1 = true,
            Level2 = true,
            Level3 = true
          };

        _mockService
         .Setup(t => t.UpdateProviderLevelsAsync(
            It.IsAny<ProviderLevelStatusChangeRequest>()))
         .Throws(new ArgumentException("This is a test"));

        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
        //Act
        var response = await _classToTest.PutStatus(request);
        //Assert
        response.Should().NotBeNull();
        Type t = response.GetType();
        response.Should().BeOfType<ObjectResult>();
        t.UnderlyingSystemType.GetProperty("StatusCode").GetValue(response)
         .Should().Be(expected);
      }
    }

    public class UpdateProviderAuthTests : AdminControllerTests
    {
      [Fact]
      public async Task IsUpdated_True_Return_Ok()
      {
        //Arrange
        ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest();
        int expected = 200;
        _mockService
         .Setup(t => t.UpdateProviderAuthAsync(
            It.IsAny<ProviderAuthUpdateRequest>()))
         .Returns(Task.FromResult(true));
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
        //Act
        var response = await _classToTest.UpdateProviderAuth(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkResult>();
        OkResult result =
          response as OkResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task IsUpdated_False_Return_500()
      {
        //Arrange
        ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest();
        int expected = 500;
        _mockService
         .Setup(t => t.UpdateProviderAuthAsync(
            It.IsAny<ProviderAuthUpdateRequest>()))
         .Returns(Task.FromResult(false));
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
        //Act
        var response = await _classToTest.UpdateProviderAuth(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<StatusCodeResult>();
        StatusCodeResult result =
          response as StatusCodeResult;
        result.StatusCode.Should().Be(expected);
      }

      [Fact]
      public async Task  NotProviderAdmin_Return_UnauthorizedObjectResult()
      {
        //Arrange
        int expected = 401;
        ProviderAuthUpdateRequest request = new ProviderAuthUpdateRequest();
        _mockService.Setup(x => x.GetAllActiveProvidersAsync())
         .Returns(Task.FromResult(_mockProviderAdminResponse.Object));
        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        //Act
        var response = await _classToTest.UpdateProviderAuth(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UnauthorizedObjectResult>();
        UnauthorizedObjectResult result =
          response as UnauthorizedObjectResult;
        result.StatusCode.Should().Be(expected);
      }
    }

    public class PutReferralRejectionReasonTests : AdminControllerTests
    {
      private Business.Entities.ProviderRejectionReason _entity;
      public PutReferralRejectionReasonTests()
      {
        _entity = new Business.Entities.ProviderRejectionReason
        {
          Id = Guid.NewGuid(),
          ModifiedAt = DateTimeOffset.Now,
          IsActive = true,
          Description = "This is a test description",
          Title = "TestDescription",
          ModifiedByUserId = Guid.Empty
        };

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
      }

      [Fact]
      public async Task Valid_Update_By_Id()
      {
        //Arrange
        string expected = "Update of Test description";
        ProviderRejectionReasonUpdate request =
          new ProviderRejectionReasonUpdate
          {
            Id = _entity.Id,
            Description = expected
          };
        Mock<ProviderRejectionReasonResponse> mockResponse = new();
        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Valid);
        _mockService.Setup(t => t.UpdateRejectionReasonsAsync(request))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await _classToTest.PutReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
      }

      [Fact]
      public async Task Valid_Update_by_Title()
      {
        //Arrange
        string expected = "Update of Test description";
        ProviderRejectionReasonUpdate request =
          new ProviderRejectionReasonUpdate
          {
            Title = "TestDescription",
            Description = expected
          };
        Mock<ProviderRejectionReasonResponse> mockResponse = new();
        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Valid);
        _mockService.Setup(t => t.UpdateRejectionReasonsAsync(request))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await _classToTest.PutReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
      }

      [Fact]
      public async Task Invalid_WrongUser()
      {
        //Arrange
        string expected = "Update of Test description";
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        ProviderRejectionReasonUpdate request =
          new ProviderRejectionReasonUpdate
          {
            Title = "TestDescription",
            Description = expected
          };
        Mock<ProviderRejectionReasonResponse> mockResponse = new();
        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Valid);
        _mockService.Setup(t => t.UpdateRejectionReasonsAsync(request))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await _classToTest.PutReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UnauthorizedObjectResult>();
      }

      [Fact]
      public async Task Invalid_Response_Invalid_BadRequestObjectResult()
      {
        //Arrange
        string expected = "Update of Test description";
        ProviderRejectionReasonUpdate request =
          new ProviderRejectionReasonUpdate
          {
            Title = "TestDescription",
            Description = expected
          };
        Mock<ProviderRejectionReasonResponse> mockResponse = new();
        mockResponse.Object.SetStatus(StatusType.Invalid,"Test Error");
        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Invalid);
        _mockService.Setup(t => t.UpdateRejectionReasonsAsync(request))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await _classToTest.PutReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<BadRequestObjectResult>();
      }

      [Fact]
      public async Task Invalid_ProviderRejectionReasonDoesNotExistsException()
      {
        //Arrange
        int expectedStatus = 404;
        string expected = "Update of Test description";
        ProviderRejectionReasonUpdate request =
          new ProviderRejectionReasonUpdate
          {
            Title = "TestDescription",
            Description = expected
          };
        _mockService.Setup(t => t.UpdateRejectionReasonsAsync(request))
          .Throws(new ProviderRejectionReasonDoesNotExistException("Test"));

        //Act
        var response = await _classToTest.PutReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expectedStatus);
      }

      [Fact]
      public async Task Invalid_ProviderRejectionReasonMismatchException()
      {
        //Arrange
        int expectedStatus = 409;
        string expected = "Update of Test description";
        ProviderRejectionReasonUpdate request =
          new ProviderRejectionReasonUpdate
          {
            Title = "TestDescription",
            Description = expected
          };
        _mockService.Setup(t => t.UpdateRejectionReasonsAsync(request))
          .Throws(new ProviderRejectionReasonMismatchException("Test"));

        //Act
        var response = await _classToTest.PutReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expectedStatus);
      }

      [Fact]
      public async Task Invalid_GeneralException()
      {
        //Arrange
        int expectedStatus = 500;
        string expected = "Update of Test description";
        ProviderRejectionReasonUpdate request =
          new ProviderRejectionReasonUpdate
          {
            Title = "TestDescription",
            Description = expected
          };
        _mockService.Setup(t => t.UpdateRejectionReasonsAsync(request))
          .Throws(new Exception("Test"));

        //Act
        var response = await _classToTest.PutReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expectedStatus);
      }
    }

    public class PostReferralRejectionReasonTests : AdminControllerTests
    {
      private Business.Entities.ProviderRejectionReason _entity;
      public PostReferralRejectionReasonTests()
      {
        _entity = new Business.Entities.ProviderRejectionReason
        {
          Id = Guid.NewGuid(),
          ModifiedAt = DateTimeOffset.Now,
          IsActive = true,
          Description = "This is a test description",
          Title = "TestDescription",
          ModifiedByUserId = Guid.Empty
        };

        _classToTest.ControllerContext = new ControllerContext();
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerAdminUser };
      }

      [Fact]
      public async Task Valid_AddNew()
      {
        //Arrange
        string expected = "Update of Test description";
        string title = "TestTitle";
        ProviderRejectionReasonSubmission request =
          new ProviderRejectionReasonSubmission
          {
            Title = title,
            Description = expected
          };
        Mock<ProviderRejectionReasonResponse> mockResponse = new();
        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Valid);
        _mockService.Setup(t => t.SetNewRejectionReasonsAsync(request))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await _classToTest.PostReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
      }


      [Fact]
      public async Task Invalid_WrongUser()
      {
        //Arrange
        string expected = "Update of Test description";
        _classToTest.ControllerContext.HttpContext =
          new DefaultHttpContext { User = _providerUser };
        ProviderRejectionReasonSubmission request =
          new ProviderRejectionReasonSubmission
          {
            Title = "TestDescription",
            Description = expected
          };
        Mock<ProviderRejectionReasonResponse> mockResponse = new();
        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Valid);
        _mockService.Setup(t => t.SetNewRejectionReasonsAsync(request))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await _classToTest.PostReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<UnauthorizedObjectResult>();
      }

      [Fact]
      public async Task Invalid_Response_Invalid_BadRequestObjectResult()
      {
        //Arrange
        string expected = "Update of Test description";
        ProviderRejectionReasonSubmission request =
          new ProviderRejectionReasonSubmission
          {
            Title = "TestDescription",
            Description = expected
          };
        Mock<ProviderRejectionReasonResponse> mockResponse = new();
        mockResponse.Object.SetStatus(StatusType.Invalid, "Test Error");
        mockResponse.Setup(t => t.ResponseStatus).Returns(StatusType.Invalid);
        _mockService.Setup(t => t.SetNewRejectionReasonsAsync(request))
          .ReturnsAsync(mockResponse.Object);

        //Act
        var response = await _classToTest.PostReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<BadRequestObjectResult>();
      }

      [Fact]
      public async Task Invalid_ProviderRejectionReasonAlreadyExistsException()
      {
        //Arrange
        int expectedStatus = 409;
        string expected = "Update of Test description";
        ProviderRejectionReasonSubmission request =
          new ProviderRejectionReasonSubmission
          {
            Title = "TestDescription",
            Description = expected
          };
        _mockService.Setup(t => t.SetNewRejectionReasonsAsync(request))
          .Throws(new ProviderRejectionReasonAlreadyExistsException("Test"));

        //Act
        var response = await _classToTest.PostReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expectedStatus);
      }

      [Fact]
      public async Task Invalid_GeneralException()
      {
        //Arrange
        int expectedStatus = 500;
        string expected = "Update of Test description";
        ProviderRejectionReasonSubmission request =
          new ProviderRejectionReasonSubmission
          {
            Title = "TestDescription",
            Description = expected
          };
        _mockService.Setup(t => t.SetNewRejectionReasonsAsync(request))
          .Throws(new Exception("Test"));

        //Act
        var response = await _classToTest.PostReferralRejectionReason(request);
        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<ObjectResult>();
        ObjectResult result =
          response as ObjectResult;
        result.StatusCode.Should().Be(expectedStatus);
      }
    }
  }
}
