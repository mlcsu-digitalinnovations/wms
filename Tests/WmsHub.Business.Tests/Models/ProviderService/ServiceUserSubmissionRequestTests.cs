using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Tests.Services;
using WmsHub.Common.Validation;
using Xunit;

namespace WmsHub.Business.Tests.Models.ProviderService
{
  public class ServiceUserSubmissionRequestTests
  {
    public class Valid : ServiceUserSubmissionRequestTests
    {
      [Fact]
      public void SubmissionStarted1()
      {
        //Arrange
        var json = ValidTestModels.SubmissionStarted1;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }

      [Fact]
      public void SubmissionStartedWithUpdates1()
      {
        //Arrange
        var json = ValidTestModels.SubmissionStartedWithUpdates1;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }
      [Fact]
      public void SubmissionRejected1()
      {
        //Arrange
        var json = ValidTestModels.SubmissionRejected1;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }
      [Fact]
      public void SubmissionRejected2()
      {
        //Arrange
        var json = ValidTestModels.SubmissionRejected2;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }
      [Fact]
      public void SubmissionComplete1()
      {
        //Arrange
        var json = ValidTestModels.SubmissionComplete1;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }
      [Fact]
      public void SubmissionCompletedWithUpdates()
      {
        //Arrange
        var json = ValidTestModels.SubmissionCompletedWithUpdates;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }
      [Fact]
      public void SumbmissionTerminated()
      {
        //Arrange
        var json = ValidTestModels.SumbmissionTerminated;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }
      [Fact]
      public void SubmissionUpdate()
      {
        //Arrange
        var json = ValidTestModels.SubmissionUpdate;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.True(result.IsValid);
        }
      }
    }

    public class InValid : ServiceUserSubmissionRequestTests
    {
      [Fact]
      public void SubmissionStartedMissingDate()
      {
        //Arrange
        var error = "The Date field is required.";
        var json = InvalidTestModels.SubmissionStartedMissinguDate;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionCompletedWithUpdatesMaxCoaching()
      {
        //Arrange
        var error = "The Coaching field '1 occurances of Coaching exceed " +
          "the max of 100' is invalid.";
        var json = InvalidTestModels.SubmissionCompletedWithUpdatesMaxCoaching;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionWithUpdatesMaxWeight()
      {
        //Arrange
        var error = "The field Weight must be between 35 and 500.";
        var json = InvalidTestModels.SubmissionUpdateMaxWeight;
        ServiceUserSubmissionRequest[] requests = JsonConvert
          .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          result.IsValid.Should().BeFalse();
          result.GetErrorMessage().Should().Be(error);
        }
      }

      [Fact]
      public void SubmissionWithUpdatesMinWeight()
      {
        //Arrange
        var error = "The field Weight must be between 35 and 500.";
        var json = InvalidTestModels.SubmissionUpdateMinWeight;
        ServiceUserSubmissionRequest[] requests = JsonConvert
          .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionCompleteMissingUbrm()
      {
        //Arrange
        var error = "The Ubrn field is required.";
        var json = InvalidTestModels.SubmissionCompleteMissingUbrm;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionCompletedWithUpdatesMissingValues()
      {
        //Arrange
        var error = "The Updates field '0 occurances of Weight, " +
          "Measure or Coaching' is invalid.";
        var json =
          InvalidTestModels.SubmissionCompletedWithUpdatesMissingValues;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }


      [Fact]
      public void SubmissionRejectedMissingUbrn()
      {
        //Arrange
        var error = "The Ubrn field is required.";
        var json = InvalidTestModels.SubmissionRejectedMissingUbrm;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionRejectedUbrnIllegalCharacters()
      {
        //Arrange
        var error = "The Ubrn field 'ABCDEF123456' is invalid.";
        var json = InvalidTestModels.SubmissionStartedUbrnNotNumeric;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionRejectedUbrnSelfReferral()
      {
        //Arrange
        var json = InvalidTestModels.SubmissionStartedUbrnSelfReferral;
        ServiceUserSubmissionRequest[] requests = JsonConvert
          .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          result.IsValid.Should().BeTrue();
        }
      }

      [Fact]
      public void SubmissionRejectedUbrnPharmacyReferral()
      {
        //Arrange
        var json = InvalidTestModels.SubmissionStartedUbrnPharmacy;
        ServiceUserSubmissionRequest[] requests = JsonConvert
          .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          result.IsValid.Should().BeTrue();
        }
      }

      [Fact]
      public void SubmissionRejectedMissingDate()
      {
        //Arrange
        var error = "The Date field is required.";
        var json = InvalidTestModels.SubmissionRejectedMissingDate;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionRejectedMissingReason()
      {
        //Arrange
        var error = "The Reason field '' is invalid.";
        var json = InvalidTestModels.SubmissionRejectedMissingReason;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }

      [Fact]
      public void SubmissionStartedMissingType()
      {
        //Arrange
        var error = "The Date field is required.";
        var json = InvalidTestModels.SubmissionStartedMissinguDate;
        ServiceUserSubmissionRequest[] requests = JsonConvert
         .DeserializeObject<ServiceUserSubmissionRequest[]>(json);
        //Act
        Assert.True(requests.Any());
        foreach (var request in requests)
        {
          ValidateModelResult result = ValidateModel(request);
          //Assert
          Assert.False(result.IsValid);
          Assert.Equal(error, result.GetErrorMessage());
        }
      }



    }


    private ValidateModelResult ValidateModel(object model)
    {
      ValidationContext context = new ValidationContext(instance: model);

      ValidateModelResult result = new ValidateModelResult();
      result.IsValid = Validator.TryValidateObject(
        model, context, result.Results, validateAllProperties: true);

      return result;
    }


  }
}
