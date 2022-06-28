using System;
namespace WmsHub.Business.Helpers
{
	public static class Base36Converter
	{
		public static string ConvertDateTimeOffsetToBase36(
			DateTimeOffset? dateTimeOffset)
		{
			char[] baseChars = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
			string base36EncodedValue = string.Empty;
			int targetBase = baseChars.Length;
			long value = dateTimeOffset.Value.Ticks;
			do
			{
				base36EncodedValue = baseChars[value % targetBase] + base36EncodedValue;
				value = value / targetBase;
			}
			while (value > 0);

			return base36EncodedValue;
		}
	}
}