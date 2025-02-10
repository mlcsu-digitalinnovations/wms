using FluentAssertions;
using WmsHub.AzureFunctions.Factories;
using WmsHub.AzureFunctions.Services;
using WmsHub.AzureFunctions.Wrappers;

namespace WmsHub.AzureFunctions.Tests.Factories;
public class ProcessFactoryTests
{

  public class Create : ProcessFactoryTests
  {
    [Fact]
    public void Should_Return_ProcessWrapperObject()
    {
      // Arrange.
      ProcessFactory processFactory = new();

      // Act.
      IProcess process = processFactory.Create();

      // Assert.
      process.Should().NotBeNull();
      process.Should().BeOfType<ProcessWrapper>();
    }
  }
}
