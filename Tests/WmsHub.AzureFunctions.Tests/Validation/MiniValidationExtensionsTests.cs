using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using WmsHub.AzureFunctions.Validation;

namespace WmsHub.AzureFunctions.Tests.Validation;

public class MiniValidationExtensionsTests
{

  public class ValidateMiniValidation : MiniValidationExtensionsTests
  {
    [Fact]
    public void Should_RegisterMiniValidationValidateOptions()
    {
      // Arrange.
      string expectedOptionsName = "TestOptions";
      Mock<IServiceCollection> mockServices = new();

      // Capture the service descriptor that gets added to the service collection.
      ServiceDescriptor? capturedDescriptor = null;
      mockServices
        .Setup(s => s.Add(It.IsAny<ServiceDescriptor>()))
        .Callback<ServiceDescriptor>(descriptor => capturedDescriptor = descriptor);

      OptionsBuilder<TestOptions> optionsBuilder = new(mockServices.Object, expectedOptionsName);

      // Act.
      optionsBuilder.ValidateMiniValidation();

      // Assert.
      mockServices.Verify(s => s.Add(It.IsAny<ServiceDescriptor>()), Times.Once);

      capturedDescriptor.Should().NotBeNull();
      capturedDescriptor!.ServiceType.Should().Be(typeof(IValidateOptions<TestOptions>));
      capturedDescriptor.ImplementationInstance.Should()
        .BeOfType<MiniValidationValidateOptions<TestOptions>>()
        .Which.Name.Should().Be(expectedOptionsName);
    }

    private class TestOptions
    {
      // Example properties for test options.
      public string? ExampleProperty { get; set; }
    }
  }
}
