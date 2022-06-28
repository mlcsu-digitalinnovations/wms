using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WmsHub.Common.Extensions;

namespace WmsHub.Utilities.Testing
{
  public static class Postcodes
  {
    public struct TestPostcode
    {
      public string Original { get; private set; }
      public string Expected { get; private set; }
      public string Converted { get; private set; }

      public bool IsConverted => Converted == Expected;

      public TestPostcode(string original, string expected)
      {
        Original = original;
        Expected = string.Compare(expected, "null", true) == 0 
          ? null
          : expected;
        Converted = original.ConvertToPostcode();
      }
    }

    internal static void RunTestFile(string filePath)
    {
      List<TestPostcode> testPostcodes = new();

      if (File.Exists(filePath))
      {
        string[] lines = File.ReadAllLines(filePath);
        int lineNo = 0;
        foreach (var line in lines)
        {
          lineNo++;
          string[] splits = line.Split("|", StringSplitOptions.TrimEntries);

          if (splits.Length != 2)
          {
            throw new Exception($"Could not find separator | on line {lineNo}");
          }

          testPostcodes.Add(new TestPostcode(splits[0], splits[1]));
        }
        Log.Information($"Found {testPostcodes.Count} postcodes.");

        testPostcodes
          .Where(t => !t.IsConverted).ToList()
          .ForEach(t => Log.Warning(
            $"{t.Original} converted to {t.Converted} not {t.Expected}."));

        Log.Information($"Successfully converted " +
          $"{testPostcodes.Count(t => t.IsConverted)} postcodes.");

      }
      else
      {
        Log.Fatal($"{filePath} does not exist.");
      }
    }
  }
}
