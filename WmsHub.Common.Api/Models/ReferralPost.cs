using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Api.Interfaces;
using WmsHub.Common.Enums;

namespace WmsHub.Common.Api.Models
{
  public class ReferralPost : ReferralPostBase, IReferralTransformable
  {
    private string _email;
    public string NhsNumber { get; set; }
    public DateTimeOffset? DateOfReferral { get; set; }
    public string ReferringGpPracticeNumber { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Postcode { get; set; }
    public string Telephone { get; set; }
    public string Mobile { get; set; }
    [StringLength(200)]
    public string Email
    {
      get => _email;
      set => _email = value?.Trim().ToLower();
    }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string Sex { get; set; }
    public bool? IsVulnerable { get; set; }
    public string VulnerableDescription { get; set; }
    public string Ethnicity { get; set; }
    public bool? HasAPhysicalDisability { get; set; }
    public bool? HasALearningDisability { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    public bool? HasHypertension { get; set; }
    public bool? HasDiabetesType1 { get; set; }
    public bool? HasDiabetesType2 { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? CalculatedBmiAtRegistration { get; set; }
    public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    public string ReferringGpPracticeName { get; set; }
    public string ReferralAttachmentId { get; set; }
    public DateTimeOffset? ReferralLetterDate { get; set; }
    public decimal? DocumentVersion { get; set; }
    public SourceSystem SourceSystem { get; set; }

    /// <summary>
    /// Can be null when record is created from a CSV Batch
    /// </summary>
    public string CriDocument { get; set; }
    public DateTimeOffset? CriLastUpdated { get; set; }
    public string PdfParseLog { get; set; }

    public DateTimeOffset? MostRecentAttachmentDate { get; set; }

    public int NumberOfMissingEntries
    {
      get
      {
        if (string.IsNullOrWhiteSpace(PdfParseLog))
        {
          return 0;
        }
        else
        {
          return PdfParseLog.Split("missing from the form").Length - 1;
        }
      }
    }

  }
}
