using System.ComponentModel.DataAnnotations;

namespace WmsHub.AzureFunctions.Options;
internal class SqlMaintenanceOptions
{
  public static string SectionKey => nameof(SqlMaintenanceOptions);

  [Required]
  public required string ConnectionString { get; set; }

  [Required]
  public string StoredProcedureName { get; set; } = "dbo.usp_AzureSQLMaintenance";

  [Required]
  public int SuccessResult { get; internal set; } = -1;
}
