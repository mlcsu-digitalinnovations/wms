using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using WmsHub.Common.Validation;

namespace WmsHub.Tests.Helper
{
  public abstract class AModelsBaseTests : ATheoryData
  {
    protected static string[] FieldNames { get; set; }
    protected static ValidateModelResult ValidateModel(object model)
    {
      ValidationContext context = new ValidationContext(instance: model);

      ValidateModelResult result = new ValidateModelResult();
      result.IsValid = Validator.TryValidateObject(
        model, context, result.Results, validateAllProperties: true);

      return result;
    }

    protected static PropertyInfo[] GetAllProperties<T>(T model)
    {
      PropertyInfo[] propinfo =
        typeof(T).GetProperties();

      return propinfo;
    }

    protected static List<string> RequiredFields<T>(T model)
    {
      List<string> result = new();
      PropertyInfo[] propinfo = typeof(T).GetProperties();
      foreach (PropertyInfo info in propinfo)
      {
        foreach (CustomAttributeData att in info.CustomAttributes)
        {
          if (att.AttributeType.Name == "RequiredAttribute")
          {
            result.Add(info.Name);
          }
        }
      }
      return result;
    }

    protected static Dictionary<string, string> RangeFieldsMax<T>(T model)
    {
      Dictionary<string, string> result = new();
      PropertyInfo[] propinfo = typeof(T).GetProperties();
      foreach (PropertyInfo info in propinfo)
      {
        if (info.Name != "ReferralAttachmentId" &&
          info.Name != "MostRecentAttachmentId")
        {
          foreach (CustomAttributeData att in info.CustomAttributes)
          {
            if (att.AttributeType.Name.Contains("Range"))
              result.Add(info.Name, 
                att.ConstructorArguments[1].Value.ToString());

          }
        }
      }
      return result;
    }

    protected static Dictionary<string, string> RangeFieldsMin<T>(T model)
    {
      Dictionary<string, string> result = new();
      PropertyInfo[] propinfo = typeof(T).GetProperties();
      foreach (PropertyInfo info in propinfo)
      {
        if (info.Name != "ReferralAttachmentId")
        {
          foreach (CustomAttributeData att in info.CustomAttributes)
          {
            if (att.AttributeType.Name.Contains("Range"))
              result.Add(info.Name,
                att.ConstructorArguments[0].Value.ToString());

          }
        }
      }
      return result;
    }

    protected static void SetPropertyValue<T>(T model, 
      string name, 
      string value,
      int add = 0,
      int minus = 0,
      bool isPositive = true)
    {
      PropertyInfo pInfo = typeof(T).GetProperty(name);

      if (pInfo.PropertyType.FullName.Contains("DateTimeOffset"))
      {
        int.TryParse(value, out int i);
        pInfo.SetValue(model, DateTimeOffset.Now.AddYears(-(i + add - minus)));
      }
      else if (pInfo.PropertyType.FullName.Contains("Decimal"))
      {
        decimal.TryParse(value, out decimal d);
        pInfo.SetValue(model, d + add - minus);
      }
      else if (pInfo.PropertyType.FullName.Contains("Int64"))
      {
        long.TryParse(value, out long l);
        pInfo.SetValue(model, l + add - minus);
      }
      else if (pInfo.PropertyType.FullName.Contains("Int32"))
      {
        int.TryParse(value, out int i);
        pInfo.SetValue(model, i + add - minus);
      }
      else
      {
        pInfo.SetValue(model, value);
      }
    }

    public static PropertyInfo[] CorrectNumberOfFields<T>(out string message) 
      where T:class
    {
      //Act
      PropertyInfo[] propinfo = typeof(T).GetProperties();

      List<string> missingCheckFields =
        FieldNames.Where(field => 
          typeof(T).GetProperty(field) == null).ToList();

      List<string> missingProperties =
        (from info in propinfo
          where Array.IndexOf(FieldNames, info.Name) == -1
          select info.Name).ToList();
      message = "";
      if (missingCheckFields.Any())
        message +=
          $"Checkfields missing from model properties:" +
          $"{string.Join(", ", missingCheckFields)}.";

      if (missingProperties.Any())
        message +=
          $"model properties missing from checkfields:" +
          $"{string.Join(", ", missingProperties)}.";

      return propinfo;
    }


    protected string GenerateCsv<T>(List<T> rows) where T : class
    {
      Type rowType = typeof(T);
      IEnumerable<PropertyInfo> properties = typeof(T).GetProperties()
        .Where(n =>
          n.PropertyType == typeof(string)
          || n.PropertyType == typeof(bool)
          || n.PropertyType == typeof(char)
          || n.PropertyType == typeof(byte)
          || n.PropertyType == typeof(decimal)
          || n.PropertyType == typeof(int)
          || n.PropertyType == typeof(DateTime)
          || n.PropertyType == typeof(DateTime?));

      string output = "";
      char delimiter = ',';

      using (StringWriter sw = new StringWriter())
      {
        var header = properties
          .Select(n => n.Name)
          .Aggregate((a, b) => a + delimiter + b);

        sw.WriteLine(header);

        foreach (T item in rows)
        {
          string row = properties
            .Select(n => n.GetValue(item, null))
            .Select(n => n == null ? "null" : n.ToString())
            .Aggregate((a, b) => a + delimiter + b);

          sw.WriteLine(row);
        }

        output = sw.ToString();
      }

      return output;
    }

  }
}
