using System;
using System.Text;
using FluentAssertions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Helpers;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Helpers
{
  public class BmiHelperTests
  {
    public class CalculateBmi : BmiHelperTests
    {
      [Fact]
      public void WeightTooLow_0()
      {
        // arrange
        decimal weightKg = Constants.MIN_WEIGHT_KG - 1;
        decimal heightKg = Constants.MIN_HEIGHT_CM + 1;

        // act
        var result = BmiHelper.CalculateBmi(weightKg, heightKg);

        // assert
        result.Should().Be(0);
      }

      [Fact]
      public void WeightTooHigh_0()
      {
        // arrange
        decimal weightKg = Constants.MAX_WEIGHT_KG + 1;
        decimal heightKg = Constants.MIN_HEIGHT_CM + 1;

        // act
        var result = BmiHelper.CalculateBmi(weightKg, heightKg);

        // assert
        result.Should().Be(0);
      }

      [Fact]
      public void HeightTooLow_0()
      {
        // arrange
        decimal weightKg = Constants.MIN_WEIGHT_KG + 1;
        decimal heightKg = Constants.MIN_HEIGHT_CM - 1;

        // act
        var result = BmiHelper.CalculateBmi(weightKg, heightKg);

        // assert
        result.Should().Be(0);
      }

      [Fact]
      public void HeightTooHigh_0()
      {
        // arrange
        decimal weightKg = Constants.MIN_WEIGHT_KG + 1;
        decimal heightKg = Constants.MAX_HEIGHT_CM + 1;

        // act
        var result = BmiHelper.CalculateBmi(weightKg, heightKg);

        // assert
        result.Should().Be(0);
      }

      [Fact]
      public void Height190Weight120_332()
      {
        // arrange
        decimal weightKg = 120;
        decimal heightKg = 190;

        // act
        var result = BmiHelper.CalculateBmi(weightKg, heightKg);

        // assert
        result.Should().Be(33.2m);
      }
    }
  }

  public class MessageTemplateEnumTests
  {
    [Fact]
    public void MessageTemplateEnum_GetMessageTemplateLookupAttribute() 
    {
      StringBuilder sb = new StringBuilder();
      foreach (Enums.MessageTemplates template in Enum.GetValues(
        typeof(Enums.MessageTemplates)))
      {
        // Arrange.

        // Act.
        MessageTemplateLookupAttribute attribute = 
          template.GetAttributeOfType<MessageTemplateLookupAttribute>();

        sb.AppendLine($" [MessageTemplateLookup(" +
          $"    {attribute.Source.ToString().Replace(","," | ")}, " +
          $"    {attribute.Status.ToString().Replace(",", " | ")}]");
        sb.AppendLine($"{template},");
      }
      var result = sb.ToString().Replace(" ","");
    }
  }
  
}
