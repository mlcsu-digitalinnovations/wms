using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.AzureFunctions.Factories;

/// <summary>
/// Facilitates the mocking of <see cref="SqlConnection"/> for unit tests.
/// </summary>
public class SqlConnectionFactory : ISqlConnectionFactory
{
  /// <summary>
  /// Creates a <see cref="SqlConnection"/> with the provided connection string.
  /// </summary>
  /// <param name="connectionString">
  /// The connection string to use in the construction of <see cref="SqlConnection"/>.
  /// </param>
  /// <returns>
  /// A <see cref="SqlConnection"/> instantiated with the provided connection string.
  /// </returns>
  public SqlConnection Create(string connectionString) => new(connectionString);

  /// <summary>
  /// Opens the provided <see cref="SqlConnection"/> asynchronously.
  /// </summary>
  /// <param name="sqlConnection">The <see cref="SqlConnection"/> to open.</param>
  [ExcludeFromCodeCoverage(Justification = "Need to create an integration to test.")]
  public async Task OpenAsync(SqlConnection sqlConnection) => await sqlConnection.OpenAsync();
}