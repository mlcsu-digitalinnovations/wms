using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using WmsHub.AzureFunctions.Factories;

namespace WmsHub.AzureFunctions.Services;

/// <summary>
/// Facilitates the mocking of <see cref="SqlConnection"/> for unit tests.
/// </summary>
/// <param name="sqlConnectionFactory">
/// </param>
public class DatabaseContext(ISqlConnectionFactory sqlConnectionFactory) : IDatabaseContext
{
  private SqlConnection? _sqlConnection;
  private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

  /// <summary>
  /// Asynchronously opens a connection using the provided connection string.
  /// </summary>
  /// <param name="connectionString">The connection string to use for the connnection.</param>
  public async Task OpenConnectionAsync(string connectionString)
  {
    _sqlConnection = _sqlConnectionFactory.Create(connectionString);
    await _sqlConnectionFactory.OpenAsync(_sqlConnection);
  }

  /// <summary>
  /// Creates a <see cref="SqlCommand"/> with the provided attributes.
  /// </summary>
  /// <param name="commandText">The text of the query.</param>
  /// <param name="commandType">Indicates how the commandText is to be interpreted.</param>
  /// <param name="commandTimeout">
  /// Wait time in seconds before the command is terminated and an error generated.
  /// </param>
  /// <returns>
  /// A <see cref="IDbCommand"/> interface to the intilised <see cref="SqlCommand"/>.
  /// </returns>
  /// <exception cref="InvalidOperationException"></exception>
  public IDbCommand CreateCommand(
    string commandText, 
    CommandType commandType, 
    int commandTimeout = 10800)
  {
    if (_sqlConnection == null)
    {
      throw new InvalidOperationException("Connection is not open.");
    }

    return new SqlCommand(commandText, _sqlConnection)
    {
      CommandType = commandType,
      CommandTimeout = commandTimeout
    };
  }

  /// <summary>
  /// Releases all resouses used by the <see cref="DatabaseContext"/>.
  /// </summary>
  [ExcludeFromCodeCoverage(Justification = "Unable to test that _sqlConnection has been disposed")]
  public void Dispose()
  {
    _sqlConnection?.Dispose();
    GC.SuppressFinalize(this);
  }
}