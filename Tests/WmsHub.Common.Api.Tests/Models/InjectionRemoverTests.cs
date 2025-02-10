using FluentAssertions;
using WmsHub.Common.Extensions;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Common.Api.Tests.Models;

public class InjectionRemoverTests : ATheoryData
{
  public class InjectionRemove : InjectionRemoverTests
  {
    [Theory]
    [MemberData(nameof(SqlInjectionData))]
    public void InjectionDataRemovedFromStringValid(string inputValue, string expected)
    {
      // Arrange.
      TestClass testClass = new()
      {
        Name = inputValue
      };

      // Act.
      testClass.InjectionRemover();

      // Assert.
      testClass.Name.Should().Be(expected);
    }
  }
}

public class TestClass
{
  public string Name { get; set; }
}