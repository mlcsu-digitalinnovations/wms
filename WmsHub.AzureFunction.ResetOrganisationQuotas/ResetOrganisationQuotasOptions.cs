using System.ComponentModel.DataAnnotations;

namespace WmsHub.AzureFunction.ResetOrganisationQuotas;

public class ResetOrganisationQuotasOptions
{
  [Required]
  public bool OverrideDate { get; set; }
  [Required]
  public string ReferralApiAdminKey { get; set; }
  [Required]
  public string ResetOrganisationQuotasUrl { get; set; }
  public const string SectionKey =
    "ResetOrganisationQuotasOptions";
}