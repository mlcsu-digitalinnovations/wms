using System;
using System.Data;
using System.Globalization;

namespace WmsHub.Common.Extensions;

public static class DataRowExtensions
{
  public static bool IsMatch(
    this DataRow dataRow,
    int columnIndex,
    string value)
  {
    if (columnIndex < 0 || columnIndex > dataRow.ItemArray.Length - 1)
    {
      throw new ArgumentOutOfRangeException(nameof(columnIndex));
    }

    if (dataRow == null)
    {
      throw new ArgumentNullException(nameof(dataRow));
    }

    if (value == null) 
    {
      throw new ArgumentNullException(nameof(value));
    }

    string rowValue = dataRow[columnIndex].ToString().Replace(" ", "");

    return rowValue.Equals(value, StringComparison.InvariantCultureIgnoreCase);
  }

  public static bool? GetColumnAsBool(
    this DataRow dataRow,
    int columnIndex)
  {
    Validate(dataRow, columnIndex);

    string value = GetColumnAsString(dataRow, columnIndex).ToUpper();

    if (value == "Y" || value == "YES" || value == "TRUE" || value == "1")
    {
      return true;
    }

    if (value == "N" || value == "NO" || value == "FALSE" || value == "0")
    {
      return false;
    }

    return null;
  }

  public static DateTimeOffset? GetColumnAsDateTimeOffset(
    this DataRow dataRow,
    int columnIndex)
  {
    Validate(dataRow, columnIndex);

    if (DateTime.TryParseExact(
      GetColumnAsString(dataRow, columnIndex),
      "yyyy-MM-dd",
      CultureInfo.InvariantCulture,
      DateTimeStyles.None,
      out DateTime dateTime))
    {
      return new DateTimeOffset(dateTime);
    }
    else
    {
      if (DateTime.TryParseExact(
        GetColumnAsString(dataRow, columnIndex),
        "dd/MM/yyyy HH:mm:ss",
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out dateTime))
      {
        return dateTime;
      }
      else
      {
        if (DateTime.TryParse(
          GetColumnAsString(dataRow, columnIndex),
          CultureInfo.InvariantCulture,
          DateTimeStyles.None,
          out dateTime))
        {
          return dateTime;
        }
      }
    }
    
    return null;
  }

  public static decimal? GetColumnAsDecimal(
    this DataRow dataRow,
    int columnIndex)
  {
    Validate(dataRow, columnIndex);

    if (decimal.TryParse(
      GetColumnAsString(dataRow, columnIndex), 
      out decimal value))
    {
      return value;
    }

    return null;
  }

  public static string GetColumnAsString(
    this DataRow dataRow,
    int columnIndex)
  {
    Validate(dataRow, columnIndex);

    return dataRow[columnIndex].ToString().Trim();
  }

  private static void Validate(DataRow dataRow, int columnIndex)
  {
    if (columnIndex < 0 || columnIndex > dataRow.ItemArray.Length - 1)
    {
      throw new ArgumentOutOfRangeException(nameof(columnIndex));
    }

    if (dataRow == null)
    {
      throw new ArgumentNullException(nameof(dataRow));
    }
  }
}
