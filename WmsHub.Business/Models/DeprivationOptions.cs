using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models
{
  public class DeprivationOptions
  {
    public const string SectionKey = "Deprivation";

    [Required]
    public string ImdResourceUrl { get; set; } = 
      @"https://assets.publishing.service.gov.uk/government/uploads/" +
      @"system/uploads/attachment_data/file/833970/" + 
      @"File_1_-_IMD2019_Index_of_Multiple_Deprivation.xlsx";

    [Required]
    public string Col1 { get; set; } = "LSOA code (2011)";
    [Required]
    public string Col2 { get; set; } =
      "Index of Multiple Deprivation(IMD) Decile";
    public DomainAccess Access => DomainAccess.DeprivationServiceApi;
  }
}
