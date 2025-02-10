using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Common.Extensions;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Console.Logging;

namespace WmsHub.ReferralsService.Console.Services
{
  internal class UploadMissingLogFile
  {
    private readonly IConfiguration _config;
    private readonly string _filename;
    private string[] _fileLines;
    private readonly DateTimeOffset? _from;
    private List<LogEvent> _logEvents;
    private readonly string _requestUri;
    private readonly DateTimeOffset? _to;       

    public int NumberOfLogEvents => _logEvents?.Count ?? 0;

    public UploadMissingLogFile(
      IConfiguration config,
      string filename,
      DateTimeOffset? from,
      DateTimeOffset? to)
    {
      if (string.IsNullOrWhiteSpace(filename))
      {
        throw new ArgumentException(
          $"'{nameof(filename)}' cannot be null or whitespace.",
          nameof(filename));
      }

      if (!File.Exists(filename))
      {
        throw new FileNotFoundException(
          $"{filename} does not exist",
          filename);
      }

      _config = config;
      _filename = filename;
      _from = from;
      _to = to;

      _requestUri = GetRequestUri();
    }

    private string GetRequestUri()
    {
      foreach (var section in _config
        .GetSection("SerilogAudit:WriteTo").GetChildren())
      {
        var subSections = section.GetChildren();
        foreach (var subsection in subSections)
        {
          if (subsection.Key.EqualsIgnoreCase("Args"))
          {
            var subSubSections = subsection.GetChildren();
            foreach (var subSubSection in subSubSections)
            {
              if (subSubSection.Key.EqualsIgnoreCase("requestUri"))
              {
                return subSubSection.Value;
              }
            }
          }
        }
      }

      throw new Exception("Unable to find requestUri key in " +
        "SerilogAudit WriteTo Http Args section");
    }

    internal async Task ProcessAsync()
    {
      LoadDataFromFile();
      CreateLogEvents();
      await UploadData();
    }

    private void LoadDataFromFile()
    {
      _fileLines = File.ReadAllLines(_filename);
    }

    private void CreateLogEvents()
    {
      _logEvents = _fileLines
        .Select(line => new LogEvent
        {
          Timestamp = DateTimeOffset.Parse(line[..30]),
          Level = line[32..35].LevelCodeToName(),
          RenderedMessage = line[37..]
        })
        .Where(le => le.Timestamp >= (_from ?? DateTimeOffset.MinValue))
        .Where(le => le.Timestamp <= (_to ?? DateTimeOffset.MaxValue))
        .ToList();
    }

    private async Task UploadData()
    {
      using ApiKeyHttpClient httpClient = new();
      httpClient.Configure(_config);

      HttpContent content = new StringContent(
        JsonConvert.SerializeObject(_logEvents),
        Encoding.UTF8,
        "application/json");

      HttpResponseMessage response = await httpClient
        .PostAsync(_requestUri, content);

      response.EnsureSuccessStatusCode();
    }
  }
}