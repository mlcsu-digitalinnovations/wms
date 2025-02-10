using Microsoft.Data.SqlClient;

namespace WmsHub.AzureFunctions.Factories;
public interface ISqlConnectionFactory
{
  SqlConnection Create(string connectionString);
  Task OpenAsync(SqlConnection sqlConnection);
}