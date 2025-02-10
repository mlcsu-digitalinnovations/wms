using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using WmsHub.Business.Helpers;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public class StaticHelperTests
  {
    public class CompressionTests : StaticHelperTests
    {
      private readonly byte[] _bytesToTest;
      private readonly string _byteAsString;

      public CompressionTests()
      {
       
        if (_bytesToTest == null)
        {
          _bytesToTest = File.ReadAllBytes($".//ConvertTest.pdf");
          _byteAsString = BitConverter.ToString(_bytesToTest);
          var bytesAsString = 
            Encoding.UTF8.GetString(_bytesToTest, 0, _bytesToTest.Length);
          var base64encoded = Convert.ToBase64String(_bytesToTest);
        }
      }
      [Fact]
      public void Valid()
      {
        //Arrange
        //Act
        byte[] result = _bytesToTest.Compress();
        //Assert
        result.Should().NotBeEquivalentTo(_bytesToTest);
        result.Length.Should().BeLessThan(_bytesToTest.Length);
      }

      [Fact]
      public void Invalid_Throw_ArgumentNullException()
      {
        //Arrange
        string expected = 
          "Value cannot be null. (Parameter 'Byte Array was null')";
        //act
        try
        {
          byte[] results = CompressionHelper.Compress(null);
          Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ae)
        {
          Assert.True(true, $"Expected Exception : {ae.Message}");
          ae.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          Assert.Fail($"Expected ArgumentNullException.  But got {ex.Message}");
        }
      }

      [Fact]
      public void Invalid_Throw_ArgumentException()
      {
        //Arrange
        string expected = "Byte Array was empty";
        byte[] test = Encoding.UTF8.GetBytes("");
        //act
        try
        {
          byte[] results = test.Compress();
          Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException ae)
        {
          Assert.True(true, $"Expected Exception : {ae.Message}");
          ae.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          Assert.Fail($"Expected ArgumentException.  But got {ex.Message}");
        }
      }
    }
  }
}