using FluentAssertions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class ReferralUpdateTests : AModelsBaseTests
  {
    [Fact]
    public void Valid()
    {
      //arrange
      IReferralUpdate referralUpdate = RandomModelCreator
        .CreateRandomReferralUpdate();

      // act
      var result = ValidateModel(referralUpdate);

      //assert
      result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(false, false, false, false)]
    public void Required_Hypertension_Or_Diabetes(
      bool hasDiabetesType1,
      bool hasDiabetesType2,
      bool hasHypertension,
      bool isValid)
    {
      //arrange
      IReferralUpdate referralCreate = RandomModelCreator
        .CreateRandomReferralUpdate(
          hasDiabetesType1: hasDiabetesType1,
          hasDiabetesType2: hasDiabetesType2,
          hasHypertension: hasHypertension);

      // act
      var result = ValidateModel(referralCreate);

      //assert
      result.IsValid.Should().Be(isValid);
    }
  }
}
