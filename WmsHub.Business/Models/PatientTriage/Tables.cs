using Newtonsoft.Json;
using System;
using System.IO;

namespace WmsHub.Business.Models.PatientTriage
{
  public class Tables
  {
    public TableData AgeGroupCompletionData { get; set; }
    public TableData SexCompletionData { get; set; }
    public TableData EthnicityCompletionData { get; set; }
    public TableData DeprivationCompletionData { get; set; }

    public TableData AgeGroupWeightData { get; set; }
    public TableData SexWeightData { get; set; }
    public TableData EthnicityWeightData { get; set; }
    public TableData DeprivationWeightData { get; set; }

    public static Tables Create(string path)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        throw new ArgumentException(
          "A path to the table data must be supplied");
      }
      if (!File.Exists(path))
      {
        throw new FileNotFoundException(
          $"The data table file {path} could not be found");
      }

      return JsonConvert.DeserializeObject<Tables>(File.ReadAllText(path));

    }
  }
}
