using System;

namespace WmsHub.Common.Extensions
{
  public static class DateTimeOffsetExtensions
  {
    public static int GetAge(this DateTimeOffset dateTimeOffset)
    {
      // Save today's date.
      DateTime today = DateTime.Today;
      // Calculate the age.
      int age = today.Year - dateTimeOffset.Year;
      // Go back to the year in which the person was born in case of a 
      // leap year
      if (dateTimeOffset.Date > today.AddYears(-age)) age--;

      return age;
    }

    public static int? GetAge(this DateTimeOffset? dateTimeOffset)
    {
      if (dateTimeOffset.HasValue)
      {
        return GetAge(dateTimeOffset.Value);
      }
      else
      {
        return null;
      } 
    }
  }
}