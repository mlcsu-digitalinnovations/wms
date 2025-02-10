using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Moq;
using WmsHub.AzureFunctions.Converters;

namespace WmsHub.AzureFunctions.Tests.Converters;

public class BooleanToIntConverterTests
{
  private readonly BooleanToIntConverter _converter;

  public BooleanToIntConverterTests() => _converter = new();

  public class ConvertToStringTests : BooleanToIntConverterTests
  {

    [Fact]
    public void When_ValueIsNull_ReturnsEmptyString()
    {
      // Arrange.
      object? value = null;

      // Act.
      string? result = _converter.ConvertToString(
        value, 
        Mock.Of<IWriterRow>(), 
        new MemberMapData(null));

      // Assert.
      result.Should().BeEmpty();
    }

    [Fact]
    public void When_ValueIsFalse_ReturnsZero()
    {
      // Arrange.
      object? value = false;

      // Act.
      string? result = _converter.ConvertToString(
        value,
        Mock.Of<IWriterRow>(),
        new MemberMapData(null));

      // Assert.
      result.Should().Be("0");
    }

    [Fact]
    public void When_ValueIsTrue_ReturnsOne()
    {
      // Arrange.
      object? value = true;

      // Act.
      string? result = _converter.ConvertToString(
        value,
        Mock.Of<IWriterRow>(),
        new MemberMapData(null));

      // Assert.
      result.Should().Be("1");
    }

    [Fact]
    public void When_ValueIsNonBoolean_ReturnsEmptyString()
    {
      // Arrange.
      object? value = "some string";

      // Act.
      string? result = _converter.ConvertToString(
        value,
        Mock.Of<IWriterRow>(),
        new MemberMapData(null));

      // Assert.
      result.Should().Be(string.Empty);
    }
  }
}