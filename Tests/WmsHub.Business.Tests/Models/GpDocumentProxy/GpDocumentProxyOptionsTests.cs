using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models.GpDocumentProxy;
public class GpDocumentProxyOptionsTests : ATheoryData
{
  private readonly Dictionary<ProgrammeOutcome, Guid> _gpProgrammeOutcomeTemplateIds;
  private readonly Dictionary<ProgrammeOutcome, Guid> _mskProgrammeOutcomeTemplateIds;

  public GpDocumentProxyOptionsTests()
  {
    _gpProgrammeOutcomeTemplateIds = ProgrammeOutcomeTemplateIds();
    _mskProgrammeOutcomeTemplateIds = ProgrammeOutcomeTemplateIds();
  }

  [Theory]
  [MemberData(nameof(ProgrammeOutcomesTheoryData),
    new ProgrammeOutcome[] { ProgrammeOutcome.NotSet })]
  public void GpGetTemplateIdValid(ProgrammeOutcome programmeOutcome)
  {
    // Arrange.
    GpDocumentProxyOptions options = new()
    {
      Gp = GetTemplateIdOptions(_gpProgrammeOutcomeTemplateIds)
    };

    // Act.
    Guid result = options.Gp.GetTemplateId(programmeOutcome.ToString());

    // Assert.
    result.Should().Be(_gpProgrammeOutcomeTemplateIds[programmeOutcome]);
  }

  [Theory]
  [MemberData(nameof(ProgrammeOutcomesTheoryData),
    new ProgrammeOutcome[] { ProgrammeOutcome.NotSet })]
  public void MskGetTemplateIdValid(ProgrammeOutcome programmeOutcome)
  {
    // Arrange.
    GpDocumentProxyOptions options = new()
    {
      Msk = GetTemplateIdOptions(_mskProgrammeOutcomeTemplateIds)
    };

    // Act.
    Guid result = options.Msk.GetTemplateId(programmeOutcome.ToString());

    // Assert.
    result.Should().Be(_mskProgrammeOutcomeTemplateIds[programmeOutcome]);
  }

  [Fact]
  public void GetProgrammeOutcomeInvalidException()
  {
    // Arrange.
    GpDocumentProxyOptions options = new()
    {
      Gp = GetTemplateIdOptions(_gpProgrammeOutcomeTemplateIds)
    };
    string invalidOutcome = "INVALIDOUTCOME";

    // Act.
    Exception resultException = Record.Exception(() => options.Gp.GetTemplateId(invalidOutcome));

    // Assert.
    resultException.Should().NotBeNull();
    resultException.Should().BeOfType<ArgumentException>();
    resultException.Message.Should()
      .Be($"Programme Outcome {invalidOutcome} does not match any valid programme outcomes.");
  }

  [Fact]
  public void GetTemplateIdInvalidException()
  {
    // Arrange.
    GpDocumentProxyOptions options = new()
    {
      Gp = GetTemplateIdOptions(_gpProgrammeOutcomeTemplateIds)
    };
    string programmeOutcome = ProgrammeOutcome.NotSet.ToString();

    // Act.
    Exception resultException = Record.Exception(() => options.Gp.GetTemplateId(programmeOutcome));

    // Assert.
    resultException.Should().NotBeNull();
    resultException.Should().BeOfType<ArgumentException>();
    resultException.Message.Should()
      .Be($"Programme Outcome {programmeOutcome} does not match any discharge template Ids.");
  }

  public static GpDocumentProxyOptions.ProgrammeOutcomeTemplateIdOptions GetTemplateIdOptions(
    Dictionary<ProgrammeOutcome, Guid> dictionary)
  {
    return new()
    {
      ProgrammeOutcomeCompleteTemplateId = dictionary[ProgrammeOutcome.Complete],
      ProgrammeOutcomeDidNotCommenceTemplateId = dictionary[ProgrammeOutcome.DidNotCommence],
      ProgrammeOutcomeDidNotCompleteTemplateId = dictionary[ProgrammeOutcome.DidNotComplete],
      ProgrammeOutcomeFailedToContactTemplateId = dictionary[ProgrammeOutcome.FailedToContact],
      ProgrammeOutcomeInvalidContactDetailsTemplateId = 
        dictionary[ProgrammeOutcome.InvalidContactDetails],
      ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId =
        dictionary[ProgrammeOutcome.RejectedAfterProviderSelection],
      ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId =
        dictionary[ProgrammeOutcome.RejectedBeforeProviderSelection],
    };
  }

  public static Dictionary<ProgrammeOutcome, Guid> ProgrammeOutcomeTemplateIds()
  {
    return new()
    {
      { ProgrammeOutcome.Complete, Guid.NewGuid() },
      { ProgrammeOutcome.DidNotCommence, Guid.NewGuid() },
      { ProgrammeOutcome.DidNotComplete, Guid.NewGuid() },
      { ProgrammeOutcome.FailedToContact, Guid.NewGuid() },
      { ProgrammeOutcome.InvalidContactDetails, Guid.NewGuid() },
      { ProgrammeOutcome.RejectedAfterProviderSelection, Guid.NewGuid() },
      { ProgrammeOutcome.RejectedBeforeProviderSelection, Guid.NewGuid() }
    };
  }
}
