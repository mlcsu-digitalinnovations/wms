using System;
using System.ComponentModel;
using System.Linq;

namespace WmsHub.Business.Helpers
{
	public static class EnumDescriptionHelper
	{
		public static string GetDescriptionFromEnum(Enum value)
		{
			DescriptionAttribute attribute = value.GetType()
				.GetField(value.ToString())
				.GetCustomAttributes(typeof(DescriptionAttribute), false)
				.SingleOrDefault() as DescriptionAttribute;

			return attribute == null ? value.ToString() : attribute.Description;
		}
	}
}