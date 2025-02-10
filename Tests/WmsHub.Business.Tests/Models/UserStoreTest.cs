using System.Linq;
using FluentAssertions;
using WmsHub.Business.Entities;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class UserStoreTest: AModelsBaseTests
  {
    private UserStore _classToTest;

    
    public class ValidateClass_Tests: UserStoreTest
    {
      [Fact]
      public void ApiKeyMissing_Throw()
      {
        //Arrange
        string expected = "The ApiKey field is required.";
        _classToTest = new UserStore
        {
          Domain = typeof(MyTestClass).Assembly.GetName().Name,
          OwnerName = "TestOwner"
        };
        //Act
        ValidateModelResult result = ValidateModel(_classToTest);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }

      [Fact]
      public void DomainMissing_Throw()
      {
        //Arrange
        string expected = "The Domain field is required.";
        _classToTest = new UserStore
        {
          ApiKey = "abc123",
          //Domain = typeof(MyTestClass).Assembly.GetName().Name,
          OwnerName = "TestOwner"
        };
        //Act
        ValidateModelResult result = ValidateModel(_classToTest);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }


      [Fact]
      public void OwnerMissing_Throw()
      {
        //Arrange
        string expected = "The OwnerName field is required.";
        _classToTest = new UserStore
        {
          ApiKey = "abc123",
          Domain = typeof(MyTestClass).Assembly.GetName().Name,
          //OwnerName = "TestOwner"
        };
        //Act
        ValidateModelResult result = ValidateModel(_classToTest);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }
    }
  }

  public class MyTestClass
  {
    public int DummyId { get; set; }
  }
}