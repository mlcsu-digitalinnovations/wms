using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using WmsHub.Business.Models.AuthService;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class AuthServiceHelperTests
  {
    public class GetHeaderValueAsTests : TokenHandlerTests
    {
      private readonly Mock<IHttpContextAccessor> _mockAccessor =
        new Mock<IHttpContextAccessor>();

      [Theory]
      [InlineData("X-Azure-SocketIP", "192.168.12.12")]
      public void IsValid(string header, string expected)
      {
        //Arrange
        DefaultHttpContext context = new DefaultHttpContext();
        string headerValue = expected;
        context.Request.Headers[header] = headerValue;
        _mockAccessor.Setup(t => t.HttpContext).Returns(context);
        AuthServiceHelper.Configure(null,_mockAccessor.Object);
        //Act
        string ipAddress = AuthServiceHelper.GetHeaderValueAs<string>(header);
        //Assert
        ipAddress.Should().Be(expected);
      }

      [Theory]
      [InlineData("X-Azure-ClientIP", "192.168.12.12")]
      [InlineData("X-Forwarded-For", "127.0.0.1")]
      public void InValidHeaderNotFound(string header, string expected)
      {
        //Arrange
        DefaultHttpContext context = new DefaultHttpContext();
        string headerValue = expected;
        context.Request.Headers["Fake_client"] = headerValue;
        _mockAccessor.Setup(t => t.HttpContext).Returns(context);
        AuthServiceHelper.Configure(null, _mockAccessor.Object);
        //Act
        string ipAddress = AuthServiceHelper.GetHeaderValueAs<string>(header);
        //Assert
        ipAddress.Should().BeNull();
      }
    }
  }
}