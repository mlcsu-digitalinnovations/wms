using FluentAssertions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models;
public class MskReferralCreateTests : AModelsBaseTests
{
  public class ValidateTests : MskReferralCreateTests
  {
    [Fact]
    public void InvalidModelReturnsCorrectValidationResults()
    {
      // Arrange.
      MskReferralCreate mskReferralCreateModel = RandomModelCreator.CreateRandomMskReferralCreate(
        ethnicity: "InvalidEthnicity",
        hasArthritisOfHip: false,
        hasArthritisOfKnee: false,
        sex: "InvalidSex");

      ValidationContext validationContext = new(mskReferralCreateModel);

      IEnumerable<string>[] expectedMemberNames =
      [
        [nameof(mskReferralCreateModel.Ethnicity)],
        [nameof(mskReferralCreateModel.Sex)],
        [
          nameof(mskReferralCreateModel.HasArthritisOfHip),
          nameof(mskReferralCreateModel.HasArthritisOfKnee)
        ]
      ];

      // Act.
      IEnumerable<ValidationResult> validationResult =
        mskReferralCreateModel.Validate(validationContext);

      // Assert.
      validationResult.Should().HaveCount(3);
      foreach (IEnumerable<string> memberNames in expectedMemberNames)
      {
        validationResult.Where(x =>
        memberNames.Intersect(x.MemberNames).Count() == memberNames.Count()).Should().HaveCount(1);
      }
    }

    [Fact]
    public void ValidModelReturnsNoValidationResults()
    {
      // Arrange.
      MskReferralCreate mskReferralCreateModel = RandomModelCreator.CreateRandomMskReferralCreate();
      ValidationContext validationContext = new(mskReferralCreateModel);

      // Act.
      IEnumerable<ValidationResult> validationResult =
        mskReferralCreateModel.Validate(validationContext);

      // Assert.
      validationResult.Should().BeEmpty();
    }
  }
}
