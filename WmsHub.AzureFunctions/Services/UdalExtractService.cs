using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.MeshMailbox.Interfaces;
using Mlcsu.Diu.Mustard.Apis.MeshMailbox.Models;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.Net;
using System.Text.Json;
using WmsHub.AzureFunctions.Converters;
using WmsHub.AzureFunctions.Factories;
using WmsHub.AzureFunctions.Models;
using WmsHub.AzureFunctions.Options;

namespace WmsHub.AzureFunctions.Services;

/// <summary>
/// Runs the DWMP UDAL extract process, with notifications to process status dashboard.
/// </summary>
internal class UdalExtractService(
  IFileSystem fileSystem,
  HttpClient httpClient,
  IMeshMailboxService meshMailboxService,
  IProcessFactory processFactory,
  IOptions<UdalExtractOptions> udalExtractOptions)
  : IUdalExtractService
{
  private const int FiveMinutes = 300000;
  private readonly IFileSystem _fileSystem = fileSystem;
  private readonly HttpClient _httpClient = httpClient;
  private readonly JsonSerializerOptions _jsonSerializerOptions = new()
  {
    PropertyNameCaseInsensitive = true,
  };
  private readonly IMeshMailboxService _meshMailboxService = meshMailboxService;
  private readonly IProcessFactory _processFactory = processFactory;
  private readonly UdalExtractOptions _options = udalExtractOptions.Value;
  private List<UdalExtract>? _udalExtractList;

  public async Task<string> ProcessAsync()
  {
    CheckAndDeleteExistingFilesInExtractDirectory();
    await DownloadUdalExtractListAsync();
    CreateCsvExtract();
    ProcessWithPipeTool();
    SendMessageResponse sendMessageResponse = await SendToMeshAsync();
    _fileSystem.File.Delete(_options.AgemPipe.EncryptedFilePath);

    string result = $"'{_options.AgemPipe.EncryptedFilename}' containing " +
      $"{_udalExtractList!.Count} referrals sent successfully to MeshMailboxApi " +
      $"with message id: {sendMessageResponse.MessageId}.";

    return result;
  }

  private void CheckAndDeleteExistingFilesInExtractDirectory()
  {
    string[] filesToDelete = _fileSystem.Directory.GetFiles(_options.AgemPipe.ExtractFilePath);
    if (filesToDelete.Length > 0)
    {
      foreach (string fileToDelete in filesToDelete)
      {
        _fileSystem.File.Delete(fileToDelete);
      }

      throw new InvalidOperationException(
        $"Found and deleted {filesToDelete.Length} unexpected files in the extract file path " +
        $"'{_options.AgemPipe.ExtractFilePath}'. All files should have been deleted in the " +
        "previous run. Investigate why these files were not deleted.");
    }
  }

  private void CreateCsvExtract()
  {
    if (_udalExtractList == null)
    {
      throw new InvalidOperationException(
        $"Failed to create CSV extract, {nameof(_udalExtractList)} is null.");
    }
    else
    {
      using FileSystemStream fileStream = _fileSystem.File
        .Create(_options.AgemPipe.FileToEncryptFilePath);
      using StreamWriter streamWriter = new(fileStream);
      using CsvWriter csvWriter = new(streamWriter, CultureInfo.GetCultureInfo("en-GB"));
      TypeConverterOptions dateFormatOptions = new() { Formats = ["dd/MM/yyyy"] };
      csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime>(dateFormatOptions);
      csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime?>(dateFormatOptions);
      csvWriter.Context.TypeConverterCache.AddConverter<bool>(new BooleanToIntConverter());
      csvWriter.Context.RegisterClassMap(typeof(UdalExtractMap));
      csvWriter.WriteRecords(_udalExtractList);
    }
  }

  private async Task DownloadUdalExtractListAsync()
  {
    HttpResponseMessage responseMessage = await _httpClient.GetAsync(_options.BiApi.RequestUri);
    if (!responseMessage.IsSuccessStatusCode)
    {
      throw new HttpRequestException(
        $"GET request to '{_options.BiApi.RequestUri}' returned a status code of " +
          $"{responseMessage.StatusCode:d}.");
    }

    string responseString = await responseMessage.Content.ReadAsStringAsync();
    if (responseMessage.StatusCode == HttpStatusCode.NoContent)
    {
      throw new InvalidOperationException(
        "No data available between the requested dates: " +
          $"'{_options.BiApi.From}' and '{_options.BiApi.To}'.");
    }
    else
    {
      _udalExtractList = JsonSerializer.Deserialize<List<UdalExtract>>(
        responseString,
        _jsonSerializerOptions);
    }
  }

  private void ProcessWithPipeTool()
  {
    try
    {
      using IProcess pipeToolProcess = _processFactory.Create();
      ProcessStartInfo processStartInfo = new()
      {
        Arguments =
          $"-i {_options.AgemPipe.FileToEncryptFilePath} " +
          $"-o {_options.AgemPipe.EncryptedFilePath} " +
          $"-c \"{_options.AgemPipe.ConfigName}\" " +
          $"-C {_options.AgemPipe.ColumnActions}",
        CreateNoWindow = false,
        FileName = _options.AgemPipe.CliPath,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        WindowStyle = ProcessWindowStyle.Hidden,
        WorkingDirectory = _options.AgemPipe.WorkingDirectory
      };
      pipeToolProcess.StartInfo = processStartInfo;

      bool hasStarted = pipeToolProcess.Start();

      if (!hasStarted)
      {
        throw new InvalidOperationException(
          "PIPE tool failed to start with the following start info: " +
            $"{processStartInfo.Arguments}.");
      }

      bool hasExited = pipeToolProcess.WaitForExit(FiveMinutes);

      if (!hasExited)
      {
        throw new TimeoutException(
          "PIPE tool failed to exit after 5 minutes with the following start info: " +
            $"{processStartInfo.Arguments}.");
      }

      string output = pipeToolProcess.StandardOutput.ReadToEnd();
      if (output != _options.AgemPipe.OutputIndicatingSuccess)
      {
        throw new InvalidOperationException(
          $"PIPE tool completed with invalid state: '{output.Replace("\r\n", "")}' when " +
            $"'{_options.AgemPipe.OutputIndicatingSuccess}' was expected.");
      }

      if (!_fileSystem.File.Exists(_options.AgemPipe.EncryptedFilePath))
      {
        throw new InvalidOperationException(
          $"PIPE tool failed to create encrypted file: '{_options.AgemPipe.EncryptedFilePath}'.");
      }
    }
    finally
    {
      if (_options.AgemPipe.FileToEncryptKeepAfterEncryption == false)
      {
        _fileSystem.File.Delete(_options.AgemPipe.FileToEncryptFilePath);
      }
    }

    if (_options.AgemPipe.FileToEncryptKeepAfterEncryption == true)
    {
      throw new InvalidOperationException(
        $"Process halted because unencrypted file '{_options.AgemPipe.FileToEncryptFilePath}' " +
        $"was kept due to '{nameof(_options.AgemPipe.FileToEncryptKeepAfterEncryption)}' " +
        $"being set to true. Review files and then delete from " +
        $"'{_options.AgemPipe.EncryptedFilePath}'.");
    }
  }

  private async Task<SendMessageResponse> SendToMeshAsync()
  {
    byte[] data = await _fileSystem.File.ReadAllBytesAsync(_options.AgemPipe.EncryptedFilePath);

    SendMessageResponse sendMessageResponse = await _meshMailboxService
      .SendMessageWithDefaultsViaMesh(new() { Data = data });

    return sendMessageResponse;
  }
}
