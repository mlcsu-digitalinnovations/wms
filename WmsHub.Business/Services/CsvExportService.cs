using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
	public class CsvExportService : ICsvExportService
	{
    private const string CsvDelimiter = ",";

    public byte[] Export<TAttribute>(IEnumerable<Referral>referrals)
      where TAttribute : ExportAttribute
    {
      using MemoryStream cMs = (MemoryStream)GetStream<TAttribute>(referrals);
      return cMs.ToArray();
    }

    protected virtual Stream GetStream<TAttribute>(
      IEnumerable<Referral> referrals)
      where TAttribute : ExportAttribute
    {

      Stream stream = new MemoryStream();
      StreamWriter streamWriter =
        new StreamWriter(stream, new UTF8Encoding(false));

      var columns = GetColumns<TAttribute>()
        .OrderBy(o => o.ExportAttribute.Order);

      var columnNames = columns.Select(c =>
          c.ExportAttribute.ExportName ?? c.PropertyInfo.Name);

      streamWriter.WriteLine(string.Join(CsvDelimiter, columnNames));

      foreach(Referral referral in referrals) {
        var values = GetReferralValues<TAttribute>(referral, columns);
        streamWriter.WriteLine(string.Join(CsvDelimiter, values));
      }

      streamWriter.Flush();
      stream.Seek(0, SeekOrigin.Begin);

      return stream;
    }

		private IEnumerable<ExportProperty> GetColumns<TAttribute>()
      where TAttribute : ExportAttribute
		{
			return typeof(Referral).GetProperties().Select(
				property =>
				{
					var exportAttribute = ((TAttribute)property
            .GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault());
					return exportAttribute == null
						? null
						: new ExportProperty { PropertyInfo = property,
              ExportAttribute = exportAttribute };
				}
			).Where(p => p != null);
		}

		private List<string> GetReferralValues<TAttribute>(Referral referral,
      IEnumerable<ExportProperty> columns)
			where TAttribute : ExportAttribute
		{
			var propertyValues = new List<string>();
			foreach (var column in columns)
			{
				propertyValues.Add(GetAttributeValue(referral, column.PropertyInfo,
          column.ExportAttribute));
			}

			return propertyValues;
		}

		private string GetAttributeValue<TAttribute>(Referral referral,
      PropertyInfo propertyInfo, TAttribute attribute)
			where TAttribute : ExportAttribute
		{
			object value = propertyInfo.GetValue(referral);

			if (value == null || attribute == null)
			{
				return string.Empty;
			}

			if (!string.IsNullOrWhiteSpace(attribute.Format) && value is IFormattable)
			{
				return (value as IFormattable).ToString(attribute.Format,
          CultureInfo.CurrentCulture);
			}

			if (!string.IsNullOrWhiteSpace(attribute.Format))
			{
				return string.Format(attribute.Format, value);
			}

			return propertyInfo.GetValue(referral).ToString();
		}
	}
}