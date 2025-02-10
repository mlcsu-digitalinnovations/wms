using System;

namespace WmsHub.Common.Extensions
{
  public static class DateTimeOffsetExtensions
  {
    public static int GetAge(
      this DateTimeOffset dateTimeOffset,
      DateTimeOffset dateToCheck = default)
    {
      // Use today's date if no date provided.
      dateToCheck = dateToCheck == default ? DateTime.Today : dateToCheck;

      // Calculate the age.
      int age = dateToCheck.Year - dateTimeOffset.Year;

      // Go back to the year in which the person was born in case of a
      // leap year
      if (dateTimeOffset.Date > dateToCheck.AddYears(-age))
      {
        age--;
      }

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

    public static bool CanUpdateDateOldToNew(
      this DateTimeOffset? oldDate,
      DateTimeOffset? newDate)
    {
      return (newDate != null && oldDate == null)
        || (newDate != null && oldDate < newDate);
    }
  }
}