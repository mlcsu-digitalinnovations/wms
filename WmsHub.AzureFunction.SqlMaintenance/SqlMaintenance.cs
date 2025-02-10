using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Data;

namespace WmsHub.AzureFunction.SqlMaintenance
{
  public static class SqlMaintenance
  {
    [Function("SqlMaintenance")]
    public static void Run(
      [TimerTrigger("0 0 0 * * *")] MyInfo myTimer,
      FunctionContext context)
    {
      ILogger logger = context.GetLogger("SqlMaintenance");

      string connectionString = Environment
        .GetEnvironmentVariable("SQLAZURECONNSTR_WmsHub");

      if (myTimer.IsPastDue)
      {
        logger.LogInformation($"Function executed late.");
      }

      if (string.IsNullOrWhiteSpace(connectionString))
      {
        throw new Exception(
          "Environmental variable SQLAZURECONNSTR_WmsHub is null or empty");
      }
      else
      {
        using SqlConnection sqlConnection = new(connectionString);
        sqlConnection.Open();

        SqlCommand sqlCommand = new(
          "dbo.usp_AzureSQLMaintenance",
          sqlConnection);
        sqlCommand.CommandType = CommandType.StoredProcedure;
        sqlCommand.CommandTimeout = 10800;
        int result = sqlCommand.ExecuteNonQuery();

        sqlConnection.Close();
        string msg = $"StoredProcedure {sqlCommand.CommandText} " +
          "{0}. Review dbo.AzureSQLMaintenanceLog for details.";

        if (result == -1)
        {
          logger.LogInformation(string.Format(msg, "completed successfully"));
        }
        else
        {
          throw new Exception(
            string.Format(msg, $"returned unexpected result {result}"));
        }
      }
    }
  }

  public class MyInfo
  {
    public MyScheduleStatus ScheduleStatus { get; set; }

    public bool IsPastDue { get; set; }
  }

  public class MyScheduleStatus
  {
    public DateTime Last { get; set; }

    public DateTime Next { get; set; }

    public DateTime LastUpdated { get; set; }
  }
}
