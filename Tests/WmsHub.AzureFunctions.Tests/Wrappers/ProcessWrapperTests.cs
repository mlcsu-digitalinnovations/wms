using FluentAssertions;
using System.Diagnostics;
using WmsHub.AzureFunctions.Wrappers;

namespace WmsHub.AzureFunctions.Tests.Wrappers;
public class ProcessWrapperTests
{
  public class Constructor : ProcessWrapperTests
  {
    [Fact]
    public void Should_SucessfullyInstansiate()
    {
      // Arrange.
      Process process = new();

      // Act.
      Action act = () => _ = new ProcessWrapper(process);

      // Assert.
      act.Should().NotThrow<Exception>();
    }

    [Fact]
    public void Should_Throw_When_ProcessParameterIsNull()
    {
      // Arrange.
      Process process = null!;

      // Act.
      Action act = () => _ = new ProcessWrapper(process);

      // Assert.
      act.Should().Throw<ArgumentNullException>().WithMessage("*process*");
    }
  }
}
