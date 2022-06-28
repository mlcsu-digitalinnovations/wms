using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Tests.Services;
using WmsHub.Common.Validation;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class ServiceUserSubmissionRequestTests
  {
    public class Valid : ServiceUserSubmissionRequestTests
    {
      [Fact]
      public async Task SubmissionStarted1()
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
      public async Task SubmissionStartedWithUpdates1()
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
      public async Task SubmissionRejected1()
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
      public async Task SubmissionRejected2()
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
      public async Task SubmissionComplete1()
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
      public async Task SubmissionCompletedWithUpdates()
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
      public async Task SumbmissionTerminated()
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
      public async Task SubmissionUpdate()
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
      public async Task SubmissionStartedMissinguDate()
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
      public async Task SubmissionCompletedWithUpdatesMaxCoaching()
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
      public async Task SubmissionWithUpdatesMaxWeight()
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
      public async Task SubmissionWithUpdatesMinWeight()
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
      public async Task SubmissionCompleteMissingUbrm()
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
      public async Task SubmissionCompletedWithUpdatesMissingValues()
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
      public async Task SubmissionRejectedMissingUbrn()
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
      public async Task SubmissionRejectedUbrnIllegalCharacters()
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
      public async Task SubmissionRejectedUbrnSelfReferral()
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
      public async Task SubmissionRejectedUbrnPharmacyReferral()
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
      public async Task SubmissionRejectedMissingDate()
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
      public async Task SubmissionRejectedMissingReason()
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
      public async Task SubmissionStartedMissingType()
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
