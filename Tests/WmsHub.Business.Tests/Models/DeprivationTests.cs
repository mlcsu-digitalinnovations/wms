using FluentAssertions;
using WmsHub.Business.Models;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class DeprivationTests : AModelsBaseTests
  {
    [Fact]
    public void Valid()
    {
      // arrange
      var model = Create();

      // act
      var result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void ImdDecile_Range(int imdDecile)
    {
      // arrange
      var model = Create();
      model.ImdDecile = imdDecile;

      var expectedErrorMessage = new RangeValidationResult(
        nameof(model.ImdDecile), 1, 10)
          .ErrorMessage;

      // act
      var result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    [Theory]
    [InlineData(1, Enums.Deprivation.IMD1)]
    [InlineData(2, Enums.Deprivation.IMD1)]
    [InlineData(3, Enums.Deprivation.IMD2)]
    [InlineData(4, Enums.Deprivation.IMD2)]
    [InlineData(5, Enums.Deprivation.IMD3)]
    [InlineData(6, Enums.Deprivation.IMD3)]
    [InlineData(7, Enums.Deprivation.IMD4)]
    [InlineData(8, Enums.Deprivation.IMD4)]
    [InlineData(9, Enums.Deprivation.IMD5)]
    [InlineData(10, Enums.Deprivation.IMD5)]
    public void ImdQuintile(
      int imdDecile, 
      Enums.Deprivation expectedImdQuintile)
    {
      // arrange
      var model = Create();
      model.ImdDecile = imdDecile;

      // act
      var validationResult = ValidateModel(model);
      var imdQuintile = model.ImdQuintile();

      // assert
      validationResult.IsValid.Should().BeTrue();
      imdQuintile.Should().Be(expectedImdQuintile);      
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 2)]
    [InlineData(5, 3)]
    [InlineData(6, 3)]
    [InlineData(7, 4)]
    [InlineData(8, 4)]
    [InlineData(9, 5)]
    [InlineData(10, 5)]
    public void ImdQuintileValue(
      int imdDecile,
      int expectedImdQuintileValue)
    {
      // arrange
      var model = Create();
      model.ImdDecile = imdDecile;

      // act
      var validationResult = ValidateModel(model);
      var imdQuintileValue = model.ImdQuintileValue();

      // assert
      validationResult.IsValid.Should().BeTrue();
      imdQuintileValue.Should().Be(expectedImdQuintileValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Lsoa_Required(string lsoa)
    {
      // arrange
      var model = Create();
      model.Lsoa = lsoa;

      var expectedErrorMessage = new RequiredValidationResult(
        nameof(model.Lsoa))
          .ErrorMessage;

      // act
      var result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }


    private static Deprivation Create(
      int imdDecile = 1,
      string lsoa = "E01000001")
    {
      return new Deprivation()
      {
        ImdDecile = imdDecile,
        Lsoa = lsoa
      };
    }
  }
}
