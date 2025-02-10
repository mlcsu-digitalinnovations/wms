using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using WmsHub.Common.Helpers;
using Xunit;

namespace WmsHub.Common.Tests
{
  public class GeneratorTests
  {
    public class GenerateKeyCodeTests : GeneratorTests
    {
      private string _upperTest = "[A-Z]";
      private string _lowerText = "[a-z]";
      private string _numberTest = "[0-9]";
      private string _specialTest = "[!#$%@<>?^~]";

      [Fact]
      public void GenerateComplextCode_default()
      {
        //arrange
        Random rnd = new Random();
        //act
        string code = Generators.GenerateKeyCode(rnd);
        //assert
        code.Length.Should().Be(6);

        MatchCollection numMatches = Regex.Matches(code, _numberTest);
        numMatches.Count.Should().BeGreaterThan(0);

        MatchCollection lowerMatches = Regex.Matches(code, _lowerText);
        lowerMatches.Count.Should().BeGreaterThan(0);

        MatchCollection upperMatches = Regex.Matches(code, _upperTest);
        upperMatches.Count.Should().BeGreaterThan(0);

        MatchCollection specialMatches = Regex.Matches(code, _specialTest);
        specialMatches.Count.Should().BeGreaterThan(0);

      }

      [Fact]
      public void GenerateComplextCode_9_digit_code()
      {
        //arrange
        Random rnd = new Random();
        //act
        string code = Generators.GenerateKeyCode(rnd, 9);
        //assert
        code.Length.Should().Be(9);

        MatchCollection numMatches = Regex.Matches(code, _numberTest);
        numMatches.Count.Should().BeGreaterThan(0);

        MatchCollection lowerMatches = Regex.Matches(code, _lowerText);
        lowerMatches.Count.Should().BeGreaterThan(0);

        MatchCollection upperMatches = Regex.Matches(code, _upperTest);
        upperMatches.Count.Should().BeGreaterThan(0);

        MatchCollection specialMatches = Regex.Matches(code, _specialTest);
        specialMatches.Count.Should().BeGreaterThan(0);

      }

      [Fact]
      public void GenerateComplextCode_6_digit_no_special()
      {
        //arrange
        Random rnd = new Random();
        //act
        string code = Generators.GenerateKeyCode(rnd, 6, false, false);
        //assert
        code.Length.Should().Be(6);

        MatchCollection numMatches = Regex.Matches(code, _numberTest);
        numMatches.Count.Should().BeGreaterThan(0);

        MatchCollection lowerMatches = Regex.Matches(code, _lowerText);
        lowerMatches.Count.Should().BeGreaterThan(0);

        MatchCollection upperMatches = Regex.Matches(code, _upperTest);
        upperMatches.Count.Should().BeGreaterThan(0);

        MatchCollection specialMatches = Regex.Matches(code, _specialTest);
        specialMatches.Count.Should().Be(0);

      }

      [Fact]
      public void GenerateComplextCode_6_digit_with_special_Anywhere()
      {
        //arrange
        Random rnd = new Random();
        //act
        string code = Generators.GenerateKeyCode(rnd, 6, true, false);
        //assert
        code.Length.Should().Be(6);

        MatchCollection numMatches = Regex.Matches(code, _numberTest);
        numMatches.Count.Should().BeGreaterThan(0);

        MatchCollection lowerMatches = Regex.Matches(code, _lowerText);
        lowerMatches.Count.Should().BeGreaterThan(0);

        MatchCollection upperMatches = Regex.Matches(code, _upperTest);
        upperMatches.Count.Should().BeGreaterThan(0);

        MatchCollection specialMatches = Regex.Matches(code, _specialTest);
        specialMatches.Count.Should().Be(1);

      }
    }
  }
}
