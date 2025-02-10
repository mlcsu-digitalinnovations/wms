using System;

namespace WmsHub.Common.Extensions
{
  public static class DateTimeExtensions
  {
    public static int GetAge(this DateTime dateTime)
    {
      // Save today's date.
      DateTime today = DateTime.Today;
      // Calculate the age.
      int age = today.Year - dateTime.Year;
      // Go back to the year in which the person was born in case of a 
      // leap year
      if (dateTime.Date > today.AddYears(-age)) age--;

      return age;
    }

    public static int? GetAge(this DateTime? dateTime)
    {
      if (dateTime.HasValue)
      {
        return GetAge(dateTime.Value);
      }
      else
      {
        return null;
      } 
    }
  }
}