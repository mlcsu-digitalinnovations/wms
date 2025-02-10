using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.PatientTriage;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services;

public class PatientTriageService :
  ServiceBase<Entities.PatientTriage>, IPatientTriageService
{
  private PatientTriageItems _triageItems;
  private readonly IMapper _mapper;

  public PatientTriageService(DatabaseContext context, IMapper mapper)
    : base(context)
  {
    _mapper = mapper;

  }

  protected PatientTriageItems TriageItems
  {
    get
    {
      if (_triageItems != null)
      {
        return _triageItems;
      }

      IQueryable<Entities.PatientTriage> entities =
        _context.PatientTriages.Where(t => t.IsActive);

      List<PatientTriage> models =
        _mapper.Map<List<PatientTriage>>(entities);

      _triageItems = new PatientTriageItems() { All = models };

      return _triageItems;
    }
  }


  public PatientTriageItemsResponse GetAllTriage()
  {
    PatientTriageItemsResponse response = new()
    {
      Status = StatusType.Valid
    };
    try
    {
      response.AgeGroupCompletionData = AgeGroupCompletionData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    try
    {
      response.AgeGroupWeightData = AgeGroupWeightData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    try
    {
      response.SexCompletionData = SexCompletionData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    try
    {
      response.SexWeightData = SexWeightData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    try
    {
      response.EthnicityCompletionData = EthnicityCompletionData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    try
    {
      response.EthnicityWeightData = EthnicityWeightData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    try
    {
      response.DeprivationCompletionData = DeprivationCompletionData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    try
    {
      response.DeprivationWeightData = DeprivationWeightData;
    }
    catch (Exception ex)
    {
      response.Status = StatusType.Invalid;
      response.Errors.Add(ex.Message);
    }

    return response;
  }

  public CourseCompletionResponse GetAllCourseCompletion()
  {
    CourseCompletionResponse response = new();

    try
    {
      foreach (KeyValuePair<string, PatientTriage> kvp in TriageItems
        .CourseConstants)
      {
        PropertyInfo propertyInfo = response.GetType().GetProperty(kvp.Key);
        propertyInfo.SetValue(response, kvp.Value.Value);
      }
    }
    catch (Exception ex)
    {
      response.Errors.Add(ex.Message);
      response.Status = StatusType.Invalid;
    }

    return response;
  }

  public async Task<CourseCompletionResponse> UpdateCourseCompletionAsync(
    CourseCompletion model)
  {
    CourseCompletionResponse response =
      _mapper.Map<CourseCompletionResponse>(model);

    bool updated = false;
    foreach (var key in Constants.CourseCompletion.CourseCompletionList)
    {
      Entities.PatientTriage entity =
        await _context.PatientTriages.SingleOrDefaultAsync(t =>
          t.TriageSection == "CompletionScores" && t.Key == key);

      if (entity == null)
      {
        response.Errors.Add($"{key} not found in PatientTriages");
        response.Status = StatusType.NoRowsUpdated;
        return response;
      }

      string propVale = Helpers.ReflectionHelper.GetPropertyValue(model, key)
        .ToString();

      if (!int.TryParse(propVale, out int val))
      {
        response.Errors.Add(
          $"{key} not found in model or propValue is not int");
        response.Status = StatusType.NoRowsUpdated;
        return response;
      }

      if (entity.Value != val)
      {
        entity.Value = val;
        UpdateModified(entity);
        updated = true;
      }
    }

    if (!updated)
    {
      response.Errors.Add("No changes to update");
      response.Status = StatusType.NoRowsUpdated;
      return response;
    }

    return response;
  }

  public async Task<PatientTriageUpdateResponse> UpdatePatientTriage(
    PatientTriageUpdateRequest request)
  {
    PatientTriageUpdateResponse response = new PatientTriageUpdateResponse();
    response.Key = request.Key;
    response.TriageArea = request.TriageArea;
    response.Value = request.Value;

    ValidateModelResult validationResult = ValidateModel(request);
    if (!validationResult.IsValid)
      throw new TriageUpdateException(validationResult.GetErrorMessage());

    Entities.PatientTriage entity =
      await _context.PatientTriages.FirstOrDefaultAsync(
      t => t.TriageSection == request.TriageArea && t.Key == request.Key);

    if (entity == null)
    {
      response.Errors.Add(
        $"Triage not found with for Area {request.TriageArea} and" +
        $" key {request.Key}.");
      response.Status = StatusType.Invalid;
      return response;
    }

    if (entity.Value == request.Value)
    {
      response.Errors.Add("No changes to update");
      response.Status = StatusType.NoRowsUpdated;
      return response;
    }

    entity.Value = request.Value;
    UpdateModified(entity);
    await _context.SaveChangesAsync();

    await ResetChecksumAsync(request.TriageArea);

    return response;
  }

  public async Task<bool> ResetChecksumAsync(string triageArea)
  {
    List<Entities.PatientTriage> entities =
      await _context.PatientTriages.Where(t =>
      t.TriageSection == triageArea && t.IsActive).ToListAsync();

    if (entities == null || !entities.Any())
      throw new TriageNotFoundException(
        $"PatientTriages not found with Triage name of {triageArea}");

    int checksum = entities.Sum(t => t.Value);

    entities.ForEach(t => t.CheckSum = checksum);

    return await _context.SaveChangesAsync() > 0;

  }


  public Dictionary<string, PatientTriage>
    AgeGroupCompletionData => TriageItems.AgeGroupCompletionData;

  public Dictionary<string, PatientTriage> AgeGroupWeightData =>
    TriageItems.AgeGroupWeightData;

  public Dictionary<string, PatientTriage> SexCompletionData =>
    TriageItems.SexCompletionData;

  public Dictionary<string, PatientTriage> SexWeightData =>
    TriageItems.SexWeightData;

  public Dictionary<string, PatientTriage>
    EthnicityCompletionData => TriageItems.EthnicityCompletionData;

  public Dictionary<string, PatientTriage> EthnicityWeightData =>
    TriageItems.EthnicityWeightData;

  public Dictionary<string, PatientTriage>
    DeprivationCompletionData => TriageItems.DeprivationCompletionData;

  public Dictionary<string, PatientTriage> DeprivationWeightData =>
    TriageItems.DeprivationWeightData;

  public CourseCompletionResult GetScores(
    CourseCompletionParameters parameters)
  {
    if (parameters == null)
    {
      throw new ArgumentNullException(nameof(parameters));
    }

    //Generate results
    CourseCompletionResult result = new CourseCompletionResult
    {
      CompletionScoreAge = TriageItems
        .AgeGroupCompletionData[parameters.AgeGroup.ToString()].Value,
      WeightScoreAge = TriageItems
        .AgeGroupWeightData[parameters.AgeGroup.ToString()].Value,

      CompletionScoreSex = TriageItems
        .SexCompletionData[parameters.Sex.ToString()].Value,
      WeightScoreSex =
        TriageItems.SexWeightData[parameters.Sex.ToString()].Value,

      CompletionScoreEthnicity = TriageItems
        .EthnicityCompletionData[parameters.Ethnicity.ToString()].Value,
      WeightScoreEthnicity = TriageItems
        .EthnicityWeightData[parameters.Ethnicity.ToString()].Value,

      CompletionScoreDeprivation = TriageItems
        .DeprivationCompletionData[parameters.Deprivation.ToString()].Value,
      WeightScoreDeprivation = TriageItems
        .DeprivationWeightData[parameters.Deprivation.ToString()].Value,

      MinimumPossibleScoreCompletion = GetConstants(
        Constants.CourseCompletion.MINIMUMPOSSIBLESCORECOMPLETION),
      MinimumPossibleScoreWeight = GetConstants(
        Constants.CourseCompletion.MINIMUMPOSSIBLESCOREWEIGHT),

      MaximumPossibleScoreCompletion = GetConstants(
        Constants.CourseCompletion.MAXIMUMPOSSIBLESCORECOMPLETION),
      MaximumPossibleScoreWeight = GetConstants(
        Constants.CourseCompletion.MAXIMUMPOSSIBLESCOREWEIGHT),

      LowCategoryLowScoreCompletion = GetConstants(
        Constants.CourseCompletion.LOWCATEGORYLOWSCORECOMPLETION),
      MediumCategoryLowScoreCompletion = GetConstants(
        Constants.CourseCompletion.MEDIUMCATEGORYLOWSCORECOMPLETION),
      HighCategoryLowScoreCompletion = GetConstants(
        Constants.CourseCompletion.HIGHCATEGORYLOWSCORECOMPLETION),

      LowCategoryLowScoreWeight = GetConstants(
        Constants.CourseCompletion.LOWCATEGORYLOWSCOREWEIGHT),
      MediumCategoryLowScoreWeight = GetConstants(
        Constants.CourseCompletion.MEDIUMCATEGORYLOWSCOREWEIGHT),
      HighCategoryLowScoreWeight = GetConstants(
        Constants.CourseCompletion.HIGHCATEGORYLOWSCOREWEIGHT),


      LowCategoryHighScoreCompletion = GetConstants(
        Constants.CourseCompletion.LOWCATEGORYHIGHSCORECOMPLETION),
      MediumCategoryHighScoreCompletion = GetConstants(
        Constants.CourseCompletion.MEDIUMCATEGORYHIGHSCORECOMPLETION),
      HighCategoryHighScoreCompletion = GetConstants(
        Constants.CourseCompletion.HIGHCATEGORYHIGHSCORECOMPLETION),

      LowCategoryHighScoreWeight = GetConstants(
        Constants.CourseCompletion.LOWCATEGORYHIGHSCOREWEIGHT),
      MediumCategoryHighScoreWeight = GetConstants
      (Constants.CourseCompletion.MEDIUMCATEGORYHIGHSCOREWEIGHT),
      HighCategoryHighScoreWeight = GetConstants(
        Constants.CourseCompletion.HIGHCATEGORYHIGHSCOREWEIGHT)
    };

    return result;
  }

  protected virtual int GetConstants(string key)
  {
    if (!TriageItems.CourseConstants.ContainsKey(key))
    {
      throw new ArgumentException(
        $"Key {key} not found in TriageItems.CourseConstants.");
    }
    return TriageItems.CourseConstants[key].Value;
  }
}
