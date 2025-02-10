using System;
using System.Reflection;
using FluentAssertions;
using WmsHub.ReferralsService.Models.BaseClasses;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ReferralsService.Tests.Models
{
  public class ReferralResultsTests:AModelsBaseTests
  {
    public class MyTestClass : ReferralsResult
    {

    }

    private readonly string[] _fields = new string[]
    {
      "AggregateErrors",
      "Errors",
      "HasErrors",
      "Success",
      "WasRetrievedFromErs",
    };

    [Fact]
    public void CorrectNumberFields()
    {
      //Arrange

      //Act
      PropertyInfo[] propinfo = GetAllProperties(new MyTestClass());
      //Assert
      foreach (PropertyInfo info in propinfo)
      {
        Array.IndexOf(_fields, info.Name).Should()
          .BeGreaterThan(-1, info.Name);
      }
      propinfo.Length.Should().Be(_fields.Length);
    }

    [Fact]
    public void NoErrors_Valid()
    {
      //Arrange
      MyTestClass classToTest = new MyTestClass();
      string expected = "No AggregateErrors";
      //Act

      //Assert
      classToTest.AggregateErrors.Should().Be(expected);
    }

    [Fact]
    public void WithErrors_Valid()
    {
      //Arrange
      MyTestClass classToTest = new MyTestClass();
      string expected = "Test Error";
      classToTest.Errors.Add(expected);
      //Act

      //Assert
      classToTest.AggregateErrors.Should().Be(expected);
    }

  }
}