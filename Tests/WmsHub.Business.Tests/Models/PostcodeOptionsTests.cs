using FluentAssertions;
using System.Linq;
using WmsHub.Business.Models;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class PostcodeOptionsTests : AModelsBaseTests
  {
    public class PostcodeIoUrl : TextOptionsTests
    {
      [Fact]
      public void AddsMissingSlashToEnd()
      {
        //Arrange
        var noslash = "http://postcodes.io";
        var expected = $"{noslash}/";
        var postcodeOptions = new PostcodeOptions();

        //Act
        postcodeOptions.PostcodeIoUrl = noslash;

        //Assert
        postcodeOptions.PostcodeIoUrl.Should().Be(expected);
      }
    }
  }
}