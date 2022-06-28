using System.Reflection;

namespace WmsHub.Business.Models
{
	public class ExportProperty
	{
		public PropertyInfo PropertyInfo { get; set; }
		public ExportAttribute ExportAttribute { get; set; }
	}
}