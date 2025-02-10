using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace WmsHub.ReferralsService.Pdf
{
  /// <summary>
  /// This is a mapping class used to normalise string values
  /// </summary>
  [Serializable]
  public class ReferralAttachmentAnswerMap
  {
    public bool loaded = false;
    public bool DuplicateErrors { get; set; }
    public string Duplicates { get; set; }

    //These are mapped first and will apply to all questions
    public Dictionary<string, string> GlobalMap { get; set; }
    
    public bool Load(string path = "./")
    {
      //the file 'globalmappings.json' contains mapping which relate to
      //all templates.
      string mappingsToLoad = Path.Combine(path, "globalmappings.json");
      Duplicates = "No issues";
      List<KeyValuePair<string,string>> map = 
        JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(
        File.ReadAllText(mappingsToLoad));
      GlobalMap = new Dictionary<string, string>();
      foreach(KeyValuePair<string,string> mapping in map)
      {
        string mapValue;
        if (GlobalMap.TryGetValue(mapping.Key, out mapValue))
        {
          //If mappings are different then a fatal error should be thrown.
          //If the mappings are identical then no error needed but a log entry
          //should be made.
          if (mapValue == mapping.Value)
          {
            Duplicates = $"{Duplicates} key='{mapping.Key}' with identical " +
              "map; ";
          }
          else
          {
            Duplicates = $"{Duplicates} key = '{mapping.Key}' with conflicting" +
              $" mappings of '{mapping.Value}' and '{mapValue}'";
            DuplicateErrors = true;
          }
        }
        else
        {
          GlobalMap.Add(mapping.Key, mapping.Value);
        }

      }
      loaded = true;

      return loaded;
    }

    public string MappedItem(string itemToCheck)
    {
      string result;
      if (!GlobalMap.TryGetValue(itemToCheck, out result)) result = itemToCheck;
      return result;
    }

  }
}
