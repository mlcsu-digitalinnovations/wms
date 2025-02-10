using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using WmsHub.AzureFunctions.Options;

namespace WmsHub.AzureFunctions.Services;

/// <summary>
/// Executes the SQL maintenance stored procedure, with notifications to process status dashboard.
/// </summary>
internal class SqlMaintenanceService(
  IDatabaseContext databaseContext,
  IOptions<SqlMaintenanceOptions> options)
  : ISqlMaintenanceService
{
  private readonly IDatabaseContext _databaseContext = databaseContext;
  private readonly SqlMaintenanceOptions _options = options.Value;  

  public async Task<string> ProcessAsync()
  {
    await _databaseContext.OpenConnectionAsync(_options.ConnectionString);

    using IDbCommand dbCommand = _databaseContext.CreateCommand(
      _options.StoredProcedureName,
      CommandType.StoredProcedure);

    int result = dbCommand.ExecuteNonQuery();

    string msg = $"StoredProcedure {_options.StoredProcedureName} {{0}}. " +
      "Review dbo.AzureSQLMaintenanceLog for details.";

    if (result == _options.SuccessResult)
    {
      return string.Format(CultureInfo.InvariantCulture, msg, "completed successfully");
    }
    else
    {
      throw new InvalidOperationException(
        string.Format(CultureInfo.InvariantCulture, msg, $"returned unexpected result {result}"));
    }
  }
}