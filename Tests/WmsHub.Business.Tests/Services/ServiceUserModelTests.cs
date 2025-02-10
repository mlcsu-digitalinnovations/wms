using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Common.Validation;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public class ServiceUserModelTests
  {
    [Theory]
    [InlineData(ValidTestModels.SubmissionStarted1)]
    [InlineData(ValidTestModels.SubmissionStartedWithUpdates1)]
    [InlineData(ValidTestModels.SubmissionRejected1)]
    [InlineData(ValidTestModels.SubmissionRejected2)]
    [InlineData(ValidTestModels.SubmissionComplete1)]
    [InlineData(ValidTestModels.SubmissionCompletedWithUpdates)]
    [InlineData(ValidTestModels.SumbmissionTerminated)]
    [InlineData(ValidTestModels.SubmissionUpdate)]
    [InlineData(ValidTestModels.SelfReferralSubmissionUpdate)]
    public void TestValidModel(string json)
    {
      //Arrange
      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
      //act
      foreach (ServiceUserSubmissionRequest request in requests)
      {
        var context = new ValidationContext(instance: request);

        var result = new ValidateModelResult();
        result.IsValid = Validator.TryValidateObject(
          request, context, result.Results, validateAllProperties: true);
        //assert
        Assert.True(result.IsValid);
      }
    }



    [Theory]
    [InlineData( "The Ubrn field is required.",
      InvalidTestModels.SubmissionStartedMissinguUbrn )]
    [InlineData("The field Ubrn must be a string with a minimum length of " +
      "12 and a maximum length of 12.", 
      InvalidTestModels.SubmissionStartedUbrnTooSmall)]
    [InlineData("The Date field is required.", 
      InvalidTestModels.SubmissionStartedMissinguDate )]
    [InlineData("The Type field is required.",
      InvalidTestModels.SubmissionStartedMissinguType)]
    [InlineData("The Date field is required.",
      InvalidTestModels.SubmissionStartedWithUpdatesMissingDate)]
    [InlineData("The Updates field '0 occurances of Weight, Measure or " +
      "Coaching' is invalid.",
      InvalidTestModels.SubmissionStartedWithUpdatesMissingWeight)]
    [InlineData("The Coaching field '1 occurances of Coaching exceed the " +
      "max of 100' is invalid.",
      InvalidTestModels.SubmissionCompletedWithUpdatesMaxCoaching)]
    public void TestInValidModel(string expected, string json)
    {
      //Arrange
      ServiceUserSubmissionRequest[] requests = JsonConvert
        .DeserializeObject<ServiceUserSubmissionRequest[]>(json);

      //act
      foreach (ServiceUserSubmissionRequest request in requests)
      {
        var context = new ValidationContext(instance: request);

        var result = new ValidateModelResult();
        result.IsValid = Validator.TryValidateObject(
          request, context, result.Results, validateAllProperties: true);

        //assert
        Assert.False(result.IsValid);
        Assert.Equal(expected, result.Results[0].ErrorMessage);
      }
    }
  }
}
