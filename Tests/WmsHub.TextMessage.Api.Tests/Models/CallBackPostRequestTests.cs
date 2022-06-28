using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using WmsHub.TextMessage.Api.Models.Notify;
using WmsHub.TextMessage.Api.Tests.TestSetup;
using Xunit;

namespace WmsHub.TextMessage.Api.Tests.Models
{
  public class CallBackPostRequestTests: AModelsBaseTests
  {
    private CallbackPostRequest _modelToTest;


    [Fact]
    public void Valid()
    {
      //Arrange
      _modelToTest = TestGenerator.CallbackPostRequestGenerator();

      //Act
      ValidateModelResult result = ValidateModel(_modelToTest);
      //Assert
      result.IsValid.Should().BeTrue();
      //Cleanup
      _modelToTest = null;
    }

    [Fact]
    public void Invalid_Id_NotSupplied()
    {
      //Arrange
      _modelToTest = TestGenerator.CallbackPostRequestGenerator();
      _modelToTest.Id = null;
      string expected = "The Id field is required.";
      //Act
      ValidateModelResult result = ValidateModel(_modelToTest);
      //Assert
      result.IsValid.Should().BeFalse();
      result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      //Cleanup
      _modelToTest = null;
    }

    [Fact]
    public void Invalid_Create_At_NotSupplied()
    {
      //Arrange
      Mock<IDateTimeWrapper> _mockDateTime = new Mock<IDateTimeWrapper>();
      _mockDateTime.Setup(t => t.Now)
       .Throws(new ArgumentNullException("Not Implemented Exception"));
      _modelToTest = TestGenerator.CallbackPostRequestGenerator();
      try
      {
        _modelToTest.Created_at = _mockDateTime.Object.Now;
        //Act
        ValidateModelResult result = ValidateModel(_modelToTest);
        //Assert
        Assert.True(false, "Expected ArgumentNullException");
      }
      catch (ArgumentNullException ex)
      {
        Assert.True(true, ex.Message);
      }
      catch (Exception ex)
      {
        Assert.True(false, ex.Message);
      }

      //Cleanup
      _modelToTest = null;
    }

  }

}
