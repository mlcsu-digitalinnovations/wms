using CsvHelper;
using CsvHelper.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Common.Api.Models;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Mappings;
using WmsHub.ReferralsService.Models;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.ReferralsService.Models.Results;

namespace WmsHub.ReferralsService
{
  public class ReferralCSVProcessor
  {
    private readonly Config _config;
    private readonly ILogger _log;
    private readonly IReferralsDataProvider _dataProvider;

    public ReferralCSVProcessor(
      IReferralsDataProvider dataProvider,
      Config configuration,
      ILogger log
      )
    {
      _config = configuration;
      _log = log;
      _dataProvider = dataProvider;

    }

    public async Task<bool> UpdateFromCSV(string filename)
    {
      bool result = true;
      try
      {
        if (File.Exists(filename) == false)
        {
          _log.Verbose($"File {filename} not found.");
          return false;
        }
        List<ReferralPutCSV> referralsToProcess = LoadCSVUpdate(filename);

        List<UpdateReferralResult> results =
          await ProcessUpdateReferrals(referralsToProcess);

        if (results == null)
          throw new InvalidDataException("The CSV File returned a null value");

        int count = 0;
        foreach (UpdateReferralResult updateReferralResult in results)
        {
          count++;
          if (updateReferralResult == null)
          {
            result = false;
            break;
          }
          else if (updateReferralResult.Success == false)
          {
            result = false;
            break;
          }
        }
      }
      catch (Exception ex)
      {
        result = false;
        _log.Verbose(ex, $"Failed to Process {filename}");
      }

      return result;
    }
    public async Task<bool> CreateFromCSV(string filename)
    {
      bool result = true;
      try
      {
        if (File.Exists(filename) == false)
        {
          _log.Verbose($"File {filename} not found.");
          return false;
        }
        List<ReferralPost> referralsToProcess = LoadCSVCreate(filename);

        List<CreateReferralResult> results = 
          await ProcessCreateReferrals(referralsToProcess);

        int count = 0;
        foreach (CreateReferralResult createReferralResult in results)
        {
          count++;
          if (createReferralResult.Success == false)
          {
            result = false;
            break;
          }
        }

      }
      catch (Exception ex)
      {
        result = false;
        _log.Verbose($"Failed to Process {filename}");
        _log.Verbose(ex, $"Message: {ex.Message}");
        if (ex.InnerException != null)
        {
          _log.Verbose($"Inner Exception: {ex.InnerException.Message}");
        }
      }

      return result;
    }

    public async Task<List<CreateReferralResult>> 
      ProcessCreateReferrals(List<ReferralPost> referralPosts)
    {
      List<CreateReferralResult> result = new List<CreateReferralResult>();
      int count = 0;
      foreach (ReferralPost referralPost in referralPosts)
      {
        count++;
        CreateReferralResult referral = 
          await _dataProvider.CreateReferral(referralPost);
        result.Add(referral);
        if (referral.Success == true)
        {
          _log.Debug($"Referral {count} Created.");
        }
        else
        {
          _log.Error(
            $"Referral {count} failed with error: {referral.AggregateErrors}");
        }
      }
      return result;
    }

    public async Task<List<UpdateReferralResult>>
      ProcessUpdateReferrals(List<ReferralPutCSV> referralPuts)
    {
      List<UpdateReferralResult> result = new List<UpdateReferralResult>();
      int count = 0;
      foreach (ReferralPutCSV referralPut in referralPuts)
      {
        count++;
        UpdateReferralResult referral =
          await _dataProvider.UpdateReferral(referralPut, referralPut.Ubrn);
        result.Add(referral);
        if (referral.Success == true)
        {
          _log.Verbose($"Referral {count} updated.");
        }
        else
        {
          _log.Verbose(
            $"Referral {count} failed with error: {referral.AggregateErrors}");
        }
      }
      return result;
    }

    public List<ReferralPost> LoadCSVCreate(string filename)
    {
      List<ReferralPost> result;

      var config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        PrepareHeaderForMatch = args => args.Header.ToLower()
      };
      using (var reader = new StreamReader(filename))
      using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
      {
        csv.Context.RegisterClassMap<ReferralPostMap>();
        result = csv.GetRecords<ReferralPost>().ToList();
      }

      return result;
    }

    public List<ReferralPutCSV> LoadCSVUpdate(string filename)
    {
      List<ReferralPutCSV> result;

      var config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        PrepareHeaderForMatch = args => args.Header.ToLower()
      };
      using (var reader = new StreamReader(filename))
      using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
      {
        csv.Context.RegisterClassMap<ReferralPutCSVMap>();
        result = csv.GetRecords<ReferralPutCSV>().ToList();
      }

      return result;
    }

    public static Batch LoadBatchCSV(string filename)
    {
      if (string.IsNullOrWhiteSpace(filename))
      {
        throw new ArgumentException("Filename should contain a value");
      }

      Batch result = new()
      {
        FileName = filename
      };
      var config = new CsvConfiguration(CultureInfo.InvariantCulture)
      {
        PrepareHeaderForMatch = args => args.Header.ToLower(),
      };
      using (var reader = new StreamReader(filename))
      using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
      {
        result.Items = csv.GetRecords<BatchItem>().ToList();
      }

      return result;
    }
  }
}
