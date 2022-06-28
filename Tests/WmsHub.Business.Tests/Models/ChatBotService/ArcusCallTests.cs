using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models.ChatBotService
{
  public class ArcusCallTests: AModelsBaseTests
  {
    private ArcusCall _classToTest;

    public class ValidateClassTests : ArcusCallTests
    {
      public ValidateClassTests()
      {
        Random rnd = new Random();
        _classToTest = new ArcusCall
        {
          Callees = new ICallee[]
          {
            new Callee()
            {
              Id = "test123",
              PrimaryPhone = Generators.GenerateMobile(rnd),
              SecondaryPhone = Generators.GenerateMobile(rnd),
              CallAttempt = "test",
              ServiceUserName = "test"
            }
          },ContactFlowName = "testflow",
          Mode = ModeType.Replace.ToString()
        };
      }
      [Fact]
      public void ValidModel()
      {
        //Arrange

        //Act
        ValidateModelResult result = ValidateModel(_classToTest);
        //Assert
        result.IsValid.Should().BeTrue();
        result.Results.Count.Should().Be(0);
        _classToTest.NumberOfCallsToMake.Should().Be(1);
      }

      [Fact]
      public void InValidModel_Missing_ContactFlowName()
      {
        //Arrange
        string expected = "The ContactFlowName field is required.";
        _classToTest.ContactFlowName = null;
        //Act
        ValidateModelResult result = ValidateModel(_classToTest);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Count.Should().Be(1);
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }

      [Fact]
      public void InValidModel_Missing_Mode()
      {
        //Arrange
        string expected = "The Mode field is required.";
        _classToTest.Mode = null;
        //Act
        ValidateModelResult result = ValidateModel(_classToTest);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Count.Should().Be(1);
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }

      [Fact]
      public void InValidModel_Missing_Callees()
      {
        //Arrange
        string expected = "The Callees field is required.";
        _classToTest.Callees = null;
        //Act
        ValidateModelResult result = ValidateModel(_classToTest);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Count.Should().Be(1);
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }
    }
  }
}
