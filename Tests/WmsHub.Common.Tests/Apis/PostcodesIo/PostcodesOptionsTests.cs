using FluentAssertions;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Tests.PostcodesIo;

public class PostcodesOptionsTests : AModelsBaseTests
{
  public class PostcodeIoUrl
  {
    [Fact]
    public void BaseUrl_AddsMissingSlashToEnd()
    {
      // Arrange.
      string noslash = "http://postcodes.io";
      string expected = $"{noslash}/";
      PostcodesIoServiceOptions options = new();

      // Act.
      options.BaseUrl = noslash;

      // Assert.
      options.BaseUrl.Should().Be(expected);
    }

    [Fact]
    public void PostcodePath_AddsMissingSlashToEnd()
    {
      // Arrange.
      string noslash = "http://postcodes.io";
      string expected = $"{noslash}/";
      PostcodesIoServiceOptions options = new();

      // Act.
      options.PostcodesPath = noslash;

      // Assert.
      options.PostcodesPath.Should().Be(expected);
    }
  }
}