using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Services
{
  public class DeprivationService
    : ServiceBase<Entities.Deprivation>, IDeprivationService
  {
    private readonly IMapper _mapper;
    private readonly DeprivationOptions _options;

    public DeprivationService(
      IOptions<DeprivationOptions> options,
      DatabaseContext context,
      IMapper mapper,
      ILogger log)
      : base(context)
    {
      _options = options.Value;
      _mapper = mapper;
      Log.Logger = log;
    }

    public async Task EtlImdFile()
    {
      List<Deprivation> deprivationList = new List<Deprivation>();

      // This is required for the Excel Data Reader
      System.Text.Encoding.RegisterProvider(
        System.Text.CodePagesEncodingProvider.Instance);

      using HttpClient httpClient = new();

      httpClient.DefaultRequestHeaders.Add(
        HttpRequestHeader.Accept.ToString(),
        "application/json;odata=verbose");

      using var stream = new MemoryStream(await
        httpClient.GetByteArrayAsync(new Uri(_options.ImdResourceUrl)));

      using var reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
      var headers = new List<string>();
      var excelResult = reader.AsDataSet(new ExcelDataSetConfiguration()
      {
        // get second worksheet from document
        FilterSheet = (tableReader, sheetIndex) => sheetIndex == 1,

        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
        {
          UseHeaderRow = true,

          ReadHeaderRow = rowReader =>
          {
            for (var i = 0; i < rowReader.FieldCount; i++)
              headers.Add(Convert.ToString(rowReader.GetValue(i)));
          },
          FilterColumn = (columnReader, columnIndex) =>
            headers.IndexOf(_options.Col1) == columnIndex ||
            headers.IndexOf(_options.Col2) == columnIndex
        }
      });

      deprivationList = ConvertToDeprivationList(excelResult.Tables[0]);

      await RefreshDeprivations(deprivationList);
    }

    public async Task RefreshDeprivations(
      IEnumerable<Deprivation> deprivations)
    {
      if (deprivations == null)
        throw new ArgumentNullException(nameof(deprivations));

      Stopwatch sw = new Stopwatch();
      sw.Start();

      //await _context.Database.ExecuteSqlRawAsync(
      //  "TRUNCATE TABLE dbo.Deprivations");

      _context.ChangeTracker.AutoDetectChangesEnabled = false;

      _context.Deprivations.RemoveRange(_context.Deprivations);

      // populate table with latest deprivations
      _context.Deprivations.AddRange(deprivations
        .Select(d => new Entities.Deprivation()
        {
          ImdDecile = d.ImdDecile,
          IsActive = true,
          Lsoa = d.Lsoa,
          ModifiedAt = DateTimeOffset.Now,
          ModifiedByUserId = User.GetUserId()
        }).ToArray());

      await _context.SaveChangesAsync();

      //Re-enable
      _context.ChangeTracker.AutoDetectChangesEnabled = true;

      sw.Stop();

      Log.Information("Took {seconds}s to add {deprivations} deprivations.",
        sw.Elapsed.TotalSeconds,
        deprivations.Count());
    }

    public async Task<Deprivation> GetByLsoa(string lsoa)
    {
      if (lsoa == null)
        throw new ArgumentNullException(nameof(lsoa));

      if (!_context.Deprivations.Any())
        throw new DeprivationNotFoundException(
          "Deprivations have not been loaded.");

      Deprivation deprivation = await _context.Deprivations
        .Where(d => d.Lsoa == lsoa)
        .Where(d => d.IsActive)
        .ProjectTo<Deprivation>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();

      if (deprivation == null)
      {
        throw new DeprivationNotFoundException(
          $"Deprivation with a lsoa code of {lsoa} not found.");
      }

      return deprivation;
    }

    private static List<Deprivation> ConvertToDeprivationList(
      DataTable dataTable)
    {
      if (dataTable == null)
        throw new ArgumentNullException(nameof(dataTable));

      List<Deprivation> deprivationList = new List<Deprivation>();

      for (int i = 0; i < dataTable.Rows.Count; ++i)
      {
        Deprivation d = new Deprivation()
        {
          Lsoa = dataTable.Rows[i][0].ToString(),
          ImdDecile = dataTable.Rows[i][1] == null
            ? 0
            : int.Parse(dataTable.Rows[i][1].ToString())
        };
        deprivationList.Add(d);
      }
      return deprivationList;
    }

  }
}