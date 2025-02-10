using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Models.MSGraph;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Apis.Ods;
using WmsHub.Common.Apis.Ods.Fhir;
using WmsHub.Common.Apis.Ods.Models;
using WmsHub.Common.Apis.Ods.PostcodesIo;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Services;

public class ElectiveCareReferralService
  : ServiceBase<Referral>, IElectiveCareReferralService
{
  private readonly IEthnicityService _ethnicityService;
  private readonly ILogger _log;
  private readonly IMapper _mapper;
  private readonly IOdsFhirService _odsFhirService;
  private readonly IOdsOrganisationService _odsOrgService;
  private readonly ElectiveCareReferralOptions _options;
  private readonly IMessageService _messageService;
  private readonly IMSGraphService _graphService;
  private readonly IPostcodesIoService _postcodeIoService;
  private readonly IReferralService _referralService;

  public ElectiveCareReferralService(
    DatabaseContext context,
    IEthnicityService ethnicityService,
    IMSGraphService graphService,
    ILogger logger,
    IMapper mapper,
    IMessageService messageService,
    IOdsFhirService odsFhirService,
    IOdsOrganisationService odsOrgService,
    IOptions<ElectiveCareReferralOptions> options,
    IPostcodesIoService postcodeService,
    IReferralService referralService)
    : base(context)
  {
    _ethnicityService = ethnicityService;
    _graphService = graphService;
    _log = logger;
    _mapper = mapper;
    _messageService = messageService;
    _odsFhirService = odsFhirService;
    _odsOrgService = odsOrgService;
    _options = options.Value;
    _postcodeIoService = postcodeService;
    _referralService = referralService;
  }

  public async Task<GetQuotaDetailsResult> GetQuotaDetailsAsync(string odsCode)
  {
    if (string.IsNullOrWhiteSpace(odsCode))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(odsCode));
    }

    GetQuotaDetailsResult result = await _context
      .Organisations
      .Where(x => x.OdsCode == odsCode)
      .Where(x => x.IsActive)
      .Select(x => new GetQuotaDetailsResult()
      {
        OdsCode = x.OdsCode,
        QuotaRemaining = x.QuotaRemaining,
        QuotaTotal = x.QuotaTotal,
      })
      .FirstOrDefaultAsync();

    result ??= new GetQuotaDetailsResult()
    {
      Error = $"{odsCode} is not an known organisation.",
      OdsCode = odsCode
    };

    return result;
  }

  public async Task<ElectiveCareUserManagementResponse>
    ManageUsersUsingUploadAsync(IEnumerable<ElectiveCareUserData> data)
  {
    ElectiveCareUserManagementResponse response = new();
    if (data == null)
    {
      return response;
    }

    Dictionary<string, string> orgs = await GetOrganisations(data);

    List<ElectiveCareUserData> usersToCreate = data
      .Where(c => c.Action == Constants.Actions.CREATE)
      .ToList();

    await CreateNewElectiveCareUsers(orgs, response, usersToCreate);

    List<ElectiveCareUserData> usersToDelete = data
      .Where(c => c.Action == Constants.Actions.DELETE)
      .ToList();

    await DeleteElectiveCareUsers(orgs, response, usersToDelete);

    return response;
  }

  public async Task<ProcessTrustDataResult> ProcessTrustDataAsync(
    IEnumerable<ElectiveCareReferralTrustData> trustData,
    string trustOdsCode,
    Guid trustUserId)
  {
    if (trustData is null)
    {
      throw new ArgumentNullException(nameof(trustData));
    }

    ProcessTrustDataResult result = new();

    if (!trustData.Any())
    {
      result.IsValid = false;
      result.Errors.Add(0, new() { "There are no data rows to process." });
      return result;
    }

    foreach (ElectiveCareReferralTrustData row in trustData)
    {
      await UpdateEthnicityAsync(row);

      await ValidateRowAsync(row);

      if (!row.IsValid)
      {
        result.IsValid = false;
        result.Errors.Add(row.RowNumber, row.ValidationErrors);
        await SaveElectiveCarePostErrorsAsync(row, trustOdsCode, trustUserId);
      }
    }

    if (result.IsValid)
    {
      foreach (ElectiveCareReferralTrustData row in trustData)
      {
        Referral referral = await CreateReferralAsync(row, trustUserId);
        await UpdateUbrnAsync(referral.Id);

        result.NoOfReferralsCreated++;
      }

      GetQuotaDetailsResult quotaDetails = await ReduceQuotaAsync(
        trustData.First().TrustOdsCode,
        result.NoOfReferralsCreated);

      result.QuotaRemaining = quotaDetails.QuotaRemaining;
      result.QuotaTotal = quotaDetails.QuotaTotal;
    }

    return result;
  }

  public async Task<bool> UserHasAccessToOdsCodeAsync(
  Guid trustUserId,
  string trustOdsCode)
  {
    bool result;

    if (await _odsFhirService.OrganisationCodeExistsAsync(trustOdsCode))
    {
      result = true;
    }
    else
    {
      _log.Debug(
        "Invalid {propertyName} of '{trustOdsCode}'.",
        nameof(trustOdsCode),
        trustOdsCode);

      result = false;
    }

    return result;
  }

  private async Task<GetQuotaDetailsResult> ReduceQuotaAsync(
    string trustOdsCode,
    int reduceBy)
  {
    if (string.IsNullOrWhiteSpace(trustOdsCode))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(trustOdsCode));
    }
    if (reduceBy < 0)
    {
      throw new ArgumentException(
        $"{nameof(reduceBy)} must be greater than zero.",
        nameof(reduceBy));
    }

    Entities.Organisation organisation = await _context.Organisations
      .Where(x => x.IsActive)
      .Where(x => x.OdsCode == trustOdsCode.ToUpper())
      .SingleOrDefaultAsync()
      ?? throw new OrganisationNotFoundException(trustOdsCode);

    organisation.QuotaRemaining -= reduceBy;
    UpdateModified(organisation);

    await _context.SaveChangesAsync();

    return new()
    {
      OdsCode = trustOdsCode,
      QuotaRemaining = organisation.QuotaRemaining,
      QuotaTotal = organisation.QuotaTotal,
    };
  }

  private async Task<Referral> CreateReferralAsync(
    ElectiveCareReferralTrustData row,
    Guid trustUserId)
  {
    if (row is null)
    {
      throw new ArgumentNullException(nameof(row));
    }
    if (trustUserId == Guid.Empty)
    {
      throw new ArgumentOutOfRangeException(nameof(trustUserId));
    }

    Referral referral = new()
    {
      CalculatedBmiAtRegistration = row.TrustReportedBmi,
      CreatedByUserId = trustUserId.ToString(),
      CreatedDate = DateTimeOffset.Now,
      DateOfBirth = row.DateOfBirth,
      DateOfBmiAtRegistration = row.DateOfTrustReportedBmi,
      DatePlacedOnWaitingList = row.DatePlacedOnWaitingList,
      DateOfReferral = row.DateOfReferral,
      Ethnicity = row.Ethnicity,
      FamilyName = row.FamilyName,
      GivenName = row.GivenName,
      IsActive = true,
      IsMobileValid = true,
      IsTelephoneValid = false,
      Mobile = row.Mobile,
      ModifiedAt = DateTimeOffset.Now,
      ModifiedByUserId = trustUserId,
      NhsNumber = row.NhsNumber,
      OpcsCodes = row.OpcsCodes,
      Postcode = row.Postcode,
      ReferralSource = row.ReferralSource.ToString(),
      ReferringGpPracticeNumber = Constants.UNKNOWN_GP_PRACTICE_NUMBER,
      ReferringGpPracticeName = Constants.UNKNOWN_GP_PRACTICE_NAME,
      ReferringOrganisationOdsCode = row.TrustOdsCode,
      ServiceUserEthnicity = row.ServiceUserEthnicity,
      ServiceUserEthnicityGroup = row.ServiceUserEthnicityGroup,
      Sex = row.Sex.GetDescriptionAttributeValue(),
      SourceEthnicity = row.SourceEthnicity,
      SpellIdentifier = row.SpellIdentifier,
      Status = ReferralStatus.New.ToString(),
      SurgeryInLessThanEighteenWeeks = row.SurgeryInLessThanEighteenWeeks,
      WeeksOnWaitingList = row.WeeksOnWaitingList
    };

    await _referralService.UpdateDeprivation(referral);

    _context.Referrals.Add(referral);
    await _context.SaveChangesAsync();

    return referral;
  }

  private async Task UpdateEthnicityAsync(ElectiveCareReferralTrustData row)
  {
    StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;

    if (row is null)
    {
      throw new ArgumentNullException(nameof(row));
    }

    row.SourceEthnicity = row.Ethnicity;

    Models.Ethnicity ethnicity = await _ethnicityService
      .GetByMultiple(row.Ethnicity);

    if (ethnicity == null)
    {
      row.Ethnicity = null;
      row.ServiceUserEthnicity = null;
      row.ServiceUserEthnicityGroup = null;
    }
    else
    {
      if (ethnicity?.GroupName.Equals(row.Ethnicity, ignoreCase) == true)
      {
        // Matched on GroupName. ServiceUserEthnicity is indeterminate
        row.Ethnicity = ethnicity.TriageName;
        row.ServiceUserEthnicity = null;
        row.ServiceUserEthnicityGroup = ethnicity.GroupName;
      }
      else if (ethnicity?.TriageName.Equals(row.Ethnicity, ignoreCase) == true)
      {
        // Matched on TriageName. ServiceUserEthnicity and
        // ServiceUserEthnicityGroup are indeterminate
        row.Ethnicity = ethnicity.TriageName;
        row.ServiceUserEthnicity = null;
        row.ServiceUserEthnicityGroup = null;
      }
      else
      {
        // All other matches are full matches.
        row.Ethnicity = ethnicity.TriageName;
        row.ServiceUserEthnicity = ethnicity.DisplayName;
        row.ServiceUserEthnicityGroup = ethnicity.GroupName;
      }
    }
  }

  private async Task UpdateUbrnAsync(Guid referralIdToUpdate)
  {
    if (await _context.ElectiveCareReferrals
      .AnyAsync(x => x.ReferralId == referralIdToUpdate))
    {
      throw new ReferralInvalidCreationException(
        $"There is already an Elective Care referral with the " +
        $"Id {referralIdToUpdate}.");
    }

    Referral referral = await _context.Referrals.FindAsync(referralIdToUpdate)
      ?? throw new ReferralNotFoundException(referralIdToUpdate);

    Entities.ElectiveCareReferral electiveCareReferral = new()
    {
      ReferralId = referralIdToUpdate
    };

    _context.ElectiveCareReferrals.Add(electiveCareReferral);
    await _context.SaveChangesAsync();

    if (string.IsNullOrWhiteSpace(electiveCareReferral.Ubrn))
    {
      throw new ReferralUpdateException(
        $"Elective Care Referral for Referral Id {referralIdToUpdate} was " +
        "not saved.");
    }
    else
    {
      referral.Ubrn = electiveCareReferral.Ubrn;
      referral.ProviderUbrn = electiveCareReferral.Ubrn;
      await _context.SaveChangesAsync();
    }
  }

  private async Task ValidateRowAsync(ElectiveCareReferralTrustData row)
  {
    row.Validate();

    // OPCS codes.
    if (!_options.ValidateOpcsCodeList(row.OpcsCodesList))
    {
      row.ValidationErrors.Add("The field 'OPCS surgery code(s)' does not " +
        "contain a valid OPCS code.");
    }

    // Ethnicity and BMI
    if (row.Ethnicity != null
      && !await _ethnicityService.IsBmiValidByTriageNameAsync(
        row.Ethnicity,
        row.TrustReportedBmi))
    {
      row.ValidationErrors.Add("The field 'Trust Reported BMI' does not " +
        "contain an eligible BMI for the provided ethnic group.");
    }

    // Existing Referrals
    try
    {
      await _referralService.CheckReferralCanBeCreatedWithNhsNumberAsync(
        row.NhsNumber);
    }
    catch (ReferralNotUniqueException)
    {
      row.ValidationErrors.Add("The field 'NHS Number' matches an existing " +
        "referral and is not yet eligible for another referral.");
    }

    // Postcode
    if (!_options.IgnorePostcodeValidation)
    {
      if (_options.EnableEnglishOnlyPostcodes)
      {
        if (!await _postcodeIoService.IsEnglishPostcodeAsync(row.Postcode))
        {
          row.ValidationErrors.Add("The field 'Postcode' does not contain an " +
            "existing English postcode.");
        }
      }
      else
      {
        if (!await _postcodeIoService.IsUkPostcodeAsync(row.Postcode))
        {
          row.ValidationErrors.Add("The field 'Postcode' does not contain a " +
             "valid UK postcode.");
        }
      }
    }

    // Surgery less than 18 weeks
    if (row.SurgeryInLessThanEighteenWeeks == null)
    {
      row.ValidationErrors.Add("The field 'Surgery in less than 18 weeks?' " +
        "must have a value and not be blank.");
    }
  }

  protected virtual async Task<Dictionary<string, string>> GetOrganisations(
    IEnumerable<ElectiveCareUserData> data)
  {
    Dictionary<string, string> organisations = new();
    foreach (string odsCode in data.Select(s => s.ODSCode).Distinct().ToList())
    {
      try
      {
        OdsOrganisation odsOrganisation = 
          await _odsOrgService.GetOdsOrganisationAsync(odsCode);

        if (odsOrganisation.WasFound)
        {
          organisations.Add(odsCode, odsOrganisation.Organisation.Name);
        }
      }
      catch(Exception ex)
      {
        _log.Error(ex, 
          $"When looking for ODS Code {odsCode}, got: {ex.Message}");
      }
    }
    return organisations;
  }

  protected virtual async Task CreateNewElectiveCareUsers(
    Dictionary<string, string> organisations,
    ElectiveCareUserManagementResponse response,
    List<ElectiveCareUserData> usersToCreate
   )
  {
    // Get Access Token
    string accessToken = await _graphService.GetBearerTokenAsync();
    foreach (ElectiveCareUserData userData in usersToCreate)
    {
      response.Processed++;

      if (await DoesUserExist(accessToken, userData) != null)
      {
        response.Add($"User already exists using email address " +
          $"{userData.EmailAddress}.");
        continue;
      }

      Guid principalId = Guid.NewGuid();

      Entities.MessageQueue message = _messageService
        .CreateElectiveCareUserCreateMessage(
        principalId,
        userData.ODSCode,
        userData.EmailAddress);

      if (!organisations.ContainsKey(userData.ODSCode))
      {
        string notFoundMessage = $"ODS Code {userData.ODSCode} not found for" +
          $" user {userData.EmailAddress}.";
        response.Add(notFoundMessage);
        response.Errors.Add(notFoundMessage);
        continue;
      }

      CreateUser user = new()
      {
        AccountEnabled = true,
        Action = Constants.Actions.CREATE,
        DisplayName = $"{userData.GivenName} {userData.Surname}",
        GivenName = userData.GivenName,
        MailNickname = $"{userData.GivenName}",
        Mail = userData.EmailAddress,
        UserPrincipalName =
          $"{userData.GivenName}.{userData.Surname}@{_options.PrincipalName}",
        OdsCode = userData.ODSCode,
        OrgName = organisations[userData.ODSCode],
        PasswordProfile = new CreateUserPassword()
        {
          ForceChangePasswordNextSignIn = true,
          Password = message.ServiceUserLinkId
        },
        Identities = new[] {new CreateIdentities
          {
            Issuer = _options.Issuer,
            SignInType = Constants.SIGNINTYPE,
            IssuerAssignedId = userData.EmailAddress
          }
        },
        Surname = userData.Surname,
        OdsCodes = organisations.Select(t => t.Key).ToArray()
      };

      ValidateModelResult result = Validators.ValidateModel(user);
      if (!result.IsValid)
      {
        response.Add($"Error processing row {userData.RowNumber}.");
        response.Errors.AddRange(
          result.Results.Select(r => r.ErrorMessage).ToList());
        continue;
      }

      ElectiveCareUser electiveCareUser =
        await _graphService.CreateElectiveCareUserAsync(accessToken, user);

      if (electiveCareUser == null)
      {
        response.Add($"{nameof(electiveCareUser)} for row" +
          $" {userData.RowNumber} is null.");
      }
      else if (electiveCareUser.IsValid)
      {
        message.ReferralId = electiveCareUser.Id;
        await _messageService.SaveElectiveCareMessage(message);
        response.UsersAdded++;
      }
      else
      {
        response.Add($"Error processing row {userData.RowNumber}.");
        response.Errors.AddRange(electiveCareUser.Errors);
      }
    }
  }

  protected virtual async Task DeleteElectiveCareUsers(
     Dictionary<string, string> organisations,
     ElectiveCareUserManagementResponse response,
     List<ElectiveCareUserData> usersToDelete)
  {
    // Get Access Token
    string accessToken = await _graphService.GetBearerTokenAsync();
    foreach (ElectiveCareUserData userData in usersToDelete)
    {
      response.Processed++;

      FilteredUser foundUser = await DoesUserExist(accessToken, userData);

      if (foundUser == null)
      {
        response.Add($"No user was found using email address " +
          $"{userData.EmailAddress}.");
        continue;
      }

      DeleteUser user = _mapper.Map<DeleteUser>(foundUser);
      user.Issuer = _options.Issuer;
      user.EmailAddress = userData.EmailAddress;
      user.OdsCodes = organisations.Select(t => t.Key).ToArray();
      user.OdsCodeCompare = userData.ODSCode;

      ValidateModelResult result = Validators.ValidateModel(user);
      if (!result.IsValid)
      {
        response.Add($"Error processing row {userData.RowNumber}.");
        response.Errors.AddRange(
          result.Results.Select(r => r.ErrorMessage).ToList());
        continue;
      }

      Entities.MessageQueue message = _messageService
       .CreateElectiveCareUserDeleteMessage(
         foundUser.Id, // Update with true id before saving
         userData.ODSCode,
         userData.EmailAddress);

      bool deleted = await _graphService.DeleteUserByIdAsync(accessToken, user);

      if (deleted)
      {
        message.ReferralId = foundUser.Id;
        response.UsersRemoved++;
        await _messageService.SaveElectiveCareMessage(message);
      }
      else
      {
        response.Add($"There was a unknown problem trying to delete user " +
          $"{userData.EmailAddress}.");
        response.Errors.AddRange(
          result.Results.Select(r => r.ErrorMessage).ToList());
      }
    }
  }

  protected async Task<FilteredUser> DoesUserExist(
    string accessToken,
    ElectiveCareUserData userData)
  {
    List<FilteredUser> users = await _graphService.GetUsersByEmailAsync(
      accessToken,
      userData.EmailAddress,
      _options.Issuer);
    if (users == null || !users.Any() || users[0].Id == Guid.Empty)
    {
      return null;
    }

    return users[0];
  }

  protected virtual async Task SaveElectiveCarePostErrorsAsync(
    ElectiveCareReferralTrustData row,
    string trustOdsCode,
    Guid trustUserId)
  {
    List<Entities.ElectiveCarePostError> electiveCarePostErrors = new();
    foreach (string error in row.ValidationErrors)
    {
      electiveCarePostErrors.Add(new()
      {
        PostError = error,
        ProcessDate = DateTime.Now,
        RowNumber = row.RowNumber,
        TrustOdsCode = trustOdsCode,
        TrustUserId = trustUserId
      });
    }

    _context.AddRange(electiveCarePostErrors);
    await _context.SaveChangesAsync();
  }
}
