using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace WmsHub.AzureFunctions.Converters;

// TODO: Consider migrating to Mustard.
/// <summary>
/// A CsvHelper converter to convert booleans to "1" or "0".
/// </summary>
public class BooleanToIntConverter : DefaultTypeConverter
{
  /// <summary>
  /// Converts a bool value to a "1" or "0".
  /// </summary>
  /// <param name="value">The value to convert.</param>
  /// <param name="row">An <see cref="IWriter"/> object.</param>
  /// <param name="memberMapData">The configured data for the MemberMap.</param>
  /// <returns>
  /// <para>"1" if value is a boolean and equal to true,</para>
  /// <para>"0" if value is a boolean and equal to false,</para>
  /// <para>or an empty string for all other types or null.</para>
  /// </returns>
  public override string? ConvertToString(
    object? value, 
    IWriterRow row, 
    MemberMapData memberMapData)
  {
    string result;

    if (value == null)
    {
      result = string.Empty;
    }

    if (value is bool booleanValue)
    {
      if (booleanValue == true)
      {
        result = "1";
      }
      else
      {
        result = "0";
      }
    }
    else
    {
      result = string.Empty;
    }

    return result;
  }
}
