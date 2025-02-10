using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.GpDocumentProxy;

public class GpDocumentProxyOptions
{
  private string _endpoint;

  [Required]
  public string[] AwaitingDischargeRejectionReasons { get; set; }

  [Required]
  public string[] CompleteRejectionReasons { get; set; }

  [Required]
  public string DelayEndpoint { get; set; }

  [Required]
  public string Endpoint
  {
    get => _endpoint;
    set => _endpoint = value.TrimEnd('/');
  }

  [Required]
  public ProgrammeOutcomeTemplateIdOptions Gp { get; set; } = new();

  [Required]
  public string[] GpdpCompleteRejectionReasons { get; set; }

  [Required]
  public string[] GpdpTracePatientRejectionReasons { get; set; }

  [Required]
  public string[] GpdpUnableToDischargeRejectionReasons { get; set; }

  [Required]
  public ProgrammeOutcomeTemplateIdOptions Msk { get; set; } = new();

  [Required]
  public int PostDischargesLimit { get; set; } = 70;

  [Required]
  public string PostEndpoint { get; set; }

  [Required]
  public string ResolveEndpoint { get; set; }

  public const string SectionKey = "GpDocumentProxyOptions";

  [Required]
  public string Token { get; set; }

  [Required]
  public string[] TracePatientRejectionReasons { get; set; }

  [Required]
  public string[] UnableToDischargeRejectionReasons { get; set; }

  [Required]
  public string UpdateEndpoint { get; set; }

  public class ProgrammeOutcomeTemplateIdOptions
  {
    [Required]
    public Guid ProgrammeOutcomeCompleteTemplateId { get; set; }

    [Required]
    public Guid ProgrammeOutcomeDidNotCommenceTemplateId { get; set; }

    [Required]
    public Guid ProgrammeOutcomeDidNotCompleteTemplateId { get; set; }

    [Required]
    public Guid ProgrammeOutcomeFailedToContactTemplateId { get; set; }

    [Required]
    public Guid ProgrammeOutcomeInvalidContactDetailsTemplateId { get; set; }

    [Required]
    public Guid ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId { get; set; }

    [Required]
    public Guid ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId { get; set; }

    public const string GpSectionKey = "Gp";
    public const string MskSectionKey = "Msk";

    public Guid GetTemplateId(string programmeOutcome)
    {
      if (Enum.TryParse(programmeOutcome, out ProgrammeOutcome programmeOutcomeValue))
      {
        return programmeOutcomeValue switch
        {
          ProgrammeOutcome.Complete => ProgrammeOutcomeCompleteTemplateId,
          ProgrammeOutcome.DidNotCommence => ProgrammeOutcomeDidNotCommenceTemplateId,
          ProgrammeOutcome.DidNotComplete => ProgrammeOutcomeDidNotCompleteTemplateId,
          ProgrammeOutcome.FailedToContact => ProgrammeOutcomeFailedToContactTemplateId,
          ProgrammeOutcome.InvalidContactDetails => ProgrammeOutcomeInvalidContactDetailsTemplateId,
          ProgrammeOutcome.RejectedAfterProviderSelection =>
            ProgrammeOutcomeRejectedAfterProviderSelectionTemplateId,
          ProgrammeOutcome.RejectedBeforeProviderSelection =>
            ProgrammeOutcomeRejectedBeforeProviderSelectionTemplateId,
          _ => throw new ArgumentException(
            $"Programme Outcome {programmeOutcome} does not match any discharge template Ids.")
        };
      }

      throw new ArgumentException(
        $"Programme Outcome {programmeOutcome} does not match any valid programme outcomes.");
    }
  }
}
