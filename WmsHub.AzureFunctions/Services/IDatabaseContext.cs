using System.Data;

namespace WmsHub.AzureFunctions.Services;
public interface IDatabaseContext : IDisposable
{
  IDbCommand CreateCommand(
    string commandText,
    CommandType commandType,
    int commandTimeout = 10800);

  Task OpenConnectionAsync(string connectionString);
}