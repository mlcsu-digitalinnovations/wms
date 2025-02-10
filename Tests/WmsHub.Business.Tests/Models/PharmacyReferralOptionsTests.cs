using FluentAssertions;
using WmsHub.Business.Models;
using WmsHub.Common.Exceptions;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class PharmacyReferralOptionsTests
  {
    protected const string EMAIL_A = "a@nhs.net";
    protected const string EMAIL_B = "b@nhs.net";
    protected const string EMAIL_NOT_IN_WHITELIST = "c@nhs.net";
    protected readonly string[] _whitelist =
      new string[] { EMAIL_A, EMAIL_B };

    public class HasEmails : PharmacyReferralOptionsTests
    {
      [Fact]
      public void ShouldDefaultToFalse()
      {
        // arrange         
        PharmacyReferralOptions options = new();

        // act
        var result = options.HasEmails;

        // assert
        result.Should().BeFalse("HasEmails should be false by default");
      }

      [Fact]
      public void ShouldBeFalseIfEmailsIsNull()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        { 
          Emails = null
        };

        // act
        var result = options.HasEmails;

        // assert
        result.Should().BeFalse("HasEmails is null");
      }

      [Fact]
      public void ShouldBeFalseIfEmailsHasNoEntries()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        {
          Emails = new string[] { }
        };

        // act
        var result = options.HasEmails;

        // assert
        result.Should().BeFalse("HasEmails has no entries");
      }

      [Fact]
      public void ShouldBeTrueIfEmailsHasEntries()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        {
          Emails = _whitelist
        };

        // act
        var result = options.HasEmails;

        // assert
        result.Should().BeTrue("HasEmails has entries");
      }
    }

    public class IsEmailInWhitelist : PharmacyReferralOptionsTests
    {

      [Fact]
      public void WhitelistShouldDefaultToDisabled()
      {
        // arrange         
        PharmacyReferralOptions options = new();

        // act
        var result = options.IsEmailInWhitelist(EMAIL_NOT_IN_WHITELIST);

        // assert
        result.Should().BeTrue("whitelist should be disabled by default.");
      }

      [Fact]
      public void WhitelistDisabledWithEmails()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        {
          Emails = _whitelist,
          IsWhitelistEnabled = false
        };

        // act
        var result = options.IsEmailInWhitelist(EMAIL_NOT_IN_WHITELIST);

        // assert
        result.Should().BeTrue(
          "with whitelist disabled all emails are in the whitelist");
      }

      [Fact]
      public void WhitelistDisabledWithoutEmails()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        {
          IsWhitelistEnabled = false
        };

        // act
        var result = options.IsEmailInWhitelist(EMAIL_NOT_IN_WHITELIST);

        // assert
        result.Should().BeTrue(
          "with whitelist disabled all emails are in the whitelist");
      }

      [Fact]
      public void WhitelistEnabledWithoutEmails()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        {
          IsWhitelistEnabled = true
        };

        // act & assert
        var ex = Assert.Throws<EmailWhiteListException>(
          () => options.IsEmailInWhitelist(EMAIL_NOT_IN_WHITELIST));

        ex.Message.Should().Be("Pharmacy whitelist is enabled but empty.");
      }

      [Fact]
      public void WhitelistEnabledEmailInWhitelist()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        {
          Emails = _whitelist,
          IsWhitelistEnabled = true
        };

        // act 
        var result = options.IsEmailInWhitelist(EMAIL_A);

        // assert
        result.Should().BeTrue($"{EMAIL_A} is in the pharmacy whitelist");
      }

      [Fact]
      public void WhitelistEnabledEmailNotInWhitelist()
      {
        // arrange         
        PharmacyReferralOptions options = new()
        {
          Emails = _whitelist,
          IsWhitelistEnabled = true
        };

        // act & assert
        var ex = Assert.Throws<EmailWhiteListException>(
          () => options.IsEmailInWhitelist(EMAIL_NOT_IN_WHITELIST));

        ex.Message.Should().Be(
          $"Email {EMAIL_NOT_IN_WHITELIST} is not in the pharmacy whitelist.");
      }
    }
  }
}
