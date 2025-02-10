using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using WmsHub.AzureFunctions.Options;
using WmsHub.Tests.Helper;

namespace WmsHub.AzureFunctions.Tests.Options;
public class SendTextMessagesOptionsTests : ABaseTests
{
  public class SectionKeyTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_BeNameOfClass()
    {
      // Arrange.
      string expectedSectionKey = $"{nameof(SendTextMessagesOptions)}";

      // Act.
      string sectionKey = SendTextMessagesOptions.SectionKey;

      // Assert.
      sectionKey.Should().Be(expectedSectionKey);
    }
  }

  public class ApiKeyTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SendTextMessagesOptions, RequiredAttribute>("ApiKey")
        .Should().BeTrue();
    }
  }

  public class BatchSizeTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_HaveDefaultValue()
    {
      // Arrange.
      int expectedBatchSize = 200;
      SendTextMessagesOptions options = new()
      { 
        ApiKey = string.Empty,
        TextMessageApiUrl = string.Empty
      };

      // Act.
      int batchSize = options.BatchSize;

      // Assert.
      batchSize.Should().Be(expectedBatchSize);
    }

    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SendTextMessagesOptions, RequiredAttribute>("BatchSize")
        .Should().BeTrue();
    }
  }

  public class CheckSendEndpointTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SendTextMessagesOptions, RequiredAttribute>("CheckSendEndpoint")
        .Should().BeTrue();
    }
  }

  public class PrepareEndpointTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SendTextMessagesOptions, RequiredAttribute>("PrepareEndpoint")
        .Should().BeTrue();
    }
  }

  public class SendEndpointTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SendTextMessagesOptions, RequiredAttribute>("SendEndpoint")
        .Should().BeTrue();
    }
  }

  public class SendQueryParameterBatchSizeLimitTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SendTextMessagesOptions, RequiredAttribute>("SendQueryParameterBatchSizeLimit")
        .Should().BeTrue();
    }
  }

  public class TextMessageApiUrlTests : SendTextMessagesOptionsTests
  {
    [Fact]
    public void Should_HaveRequiredAttribute()
    {
      // Arrange. Act. and Assert.
      HasAttribute<SendTextMessagesOptions, RequiredAttribute>("TextMessageApiUrl")
        .Should().BeTrue();
    }
  }
}
