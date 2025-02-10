using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using WmsHub.Ui.Models;
using Provider = WmsHub.Ui.Models.Provider;
using static WmsHub.Common.Helpers.Constants;
using System.Net.Http;
using WmsHub.Business.Services.Interfaces;
using System.Text.Json;

[assembly: InternalsVisibleTo("WmsHub.Ui.Tests"),
  InternalsVisibleTo("WmsHub.Tests.Helper")]

namespace WmsHub.Ui.Controllers;

[Authorize(Policy = "RmcUiDomainUsers")]
public class RmcController : BaseController
{
  private readonly ILogger<RmcController> _logger;
  private IReferralService _referralService;
  private IEthnicityService _ethnicityService;
  private readonly IMapper _mapper;
  private readonly IProviderService _providerService;
  private readonly INotificationService _notificationService;

  public RmcController(
    ILogger<RmcController> logger,
    IEthnicityService ethnicityService,
    IMapper mapper,
    INotificationService notificationService,
    IOptions<WebUiSettings> options,
    IReferralService referralService,
    IProviderService providerService): base(options.Value)
  {
    _logger = logger;
    _referralService = referralService;
    _ethnicityService = ethnicityService;
    _mapper = mapper;
    _providerService = providerService;
    _notificationService = notificationService;
  }

  private IReferralService ReferralService
  {
    get
    {
      // Add User principal to referral service
      _referralService.User = User;
      return _referralService;
    }
  }

  public IActionResult Index()
  {
    return RedirectToAction("referralList", "rmc", null);
  }

  public async Task<IActionResult> ProviderInfo()
  {
    ProviderInfoModel model = new();
    IEnumerable<Business.Models.ProviderInfo> providerInfos =
      await _providerService.GetProvidersInfo();
    IEnumerable<Business.Models.ReferralSourceInfo> referralSourceInfo =
      await _referralService.GetReferralSourceInfo();

    model.Providers = _mapper
      .Map<IEnumerable<Models.ProviderInfo>>(providerInfos);
    model.Sources = _mapper
      .Map<IEnumerable<Models.ReferralSourceInfo>>(referralSourceInfo);

    return View(model);
  }

  public async Task<IActionResult> ReferralList(ReferralListModel model)
  {
    ReferralSearchModel searchModel = model?.Search
      ?? new ReferralSearchModel();

    if (!searchModel.HasUserSearchCriteria)
      searchModel.IsVulnerable = false;

    searchModel.Limit ??= 25;
    searchModel.Statuses = GetRmcStatuses();
    searchModel.DelayedReferralsFilter = SearchFilter.Exclude;

    ReferralListModel referralListModel =
      await SearchCurrentReferrals(searchModel);

    SetPageTitle("Referral List");
    return View(referralListModel);
  }

  public async Task<IActionResult> PreviouslyDelayedList(
    ReferralListModel model)
  {
    ReferralSearchModel searchModel = model?.Search
      ?? new ReferralSearchModel();

    if (!searchModel.HasUserSearchCriteria)
    {
      searchModel.IsVulnerable = false;
    }

    searchModel.Limit ??= 25;
    searchModel.Statuses = new string[] { ReferralStatus.RmcCall.ToString() };
    searchModel.DelayedReferralsFilter = SearchFilter.Only;

    ReferralListModel referralListModel =
      await SearchCurrentReferrals(searchModel);

    SetPageTitle("Referral List");
    return View(referralListModel);
  }

  public async Task<IActionResult> LetterList(ReferralListModel model)
  {
    ReferralSearchModel searchModel = model?.Search
      ?? new ReferralSearchModel();

    searchModel.Limit ??= 25;
    searchModel.Statuses = GetRmcLetterStatuses();

    ReferralListModel referralListModel =
      await SearchCurrentReferrals(searchModel);

    SetPageTitle("Letter List");
    return View(referralListModel);
  }

  public async Task<IActionResult> DischargeList(ReferralListModel model)
  {
    ReferralSearchModel searchModel = model?.Search
      ?? new ReferralSearchModel();

    searchModel.Limit ??= 25;
    searchModel.Statuses = GetReadyForDischargeStatuses();

    ReferralListModel referralListModel =
      await SearchCurrentReferrals(searchModel);

    SetPageTitle("Discharge List");
    return View(referralListModel);
  }

  public async Task<IActionResult> VulnerableList(ReferralListModel model)
  {
    ReferralSearchModel searchModel = model?.Search
      ?? new ReferralSearchModel();
    searchModel.IsVulnerable = true;
    searchModel.Limit ??= 25;
    searchModel.Statuses = new string[] { ReferralStatus.RmcCall.ToString() };

    ReferralListModel referralListModel =
      await SearchCurrentReferrals(searchModel);

    SetPageTitle("Vulnerable List");
    return View(referralListModel);
  }

  public async Task<IActionResult> ExceptionList(ReferralListModel model)
  {
    // replace with call to exception service
    ReferralSearchModel searchModel = model?.Search
      ?? new ReferralSearchModel();

    searchModel.Limit ??= 25;
    searchModel.Statuses = GetExceptionStatuses();

    ReferralListModel referralListModel =
      await SearchCurrentReferrals(searchModel);

    foreach (ReferralListItemModel item in referralListModel.ListItems)
    {
      item.IsException = true;
      item.ExceptionStatus = item.Status;
    }

    SetPageTitle("Exception List");
    return View(referralListModel);
  }

  public async Task<IActionResult> RejectionList(ReferralListModel model)
  {
    // replace with call to exception service
    ReferralSearchModel searchModel = model?.Search
      ?? new ReferralSearchModel();

    searchModel.Limit ??= 25;
    searchModel.Statuses = GetRejectionStatuses;
    searchModel.ReferralSource = ReferralSource.GpReferral.ToString();

    ReferralListModel referralListModel =
      await SearchCurrentReferrals(searchModel);

    foreach (ReferralListItemModel item in referralListModel.ListItems)
    {
      item.IsException = true;
      item.ExceptionStatus = item.Status;
    }

    SetPageTitle("Rejection List");
    return View(referralListModel);
  }

  public async Task<IActionResult> ReferralView(Guid id)
  {
    _logger.LogDebug("ReferralView (GET) View #" + id.ToString());
    
    ReferralListItemModel model = await LoadReferral(id);

    SetPageTitle("Referral View");

    return View(model);
  }

  private async Task<ReferralListItemModel> LoadReferral(Guid id)
  {
    ReferralListItemModel model = await GetReferralFromService(id);

    model.AuditList = await GetAuditListAsync(model.Id);

    model.ReferralStatusReasons = await ReferralService
      .GetRmcRejectedReferralStatusReasonsAsync();

    if (model.AuditList.Any())
    {
      ReferralAuditListItemModel lastDelayAudit = model.AuditList
        .OrderByDescending(t => t.ModifiedAt)
        .FirstOrDefault(t => t.Status == ReferralStatus.RmcDelayed.ToString());

      if (lastDelayAudit != null)
      {
        model.DelayFrom = lastDelayAudit.ModifiedAt;
        model.DelayUntil = lastDelayAudit.DateToDelayUntil;
      }
    }

    model.IsException = GetExceptionStatuses().Contains(model.Status);

    if (model.IsException
      && model.HasStatusReason
      && model.StatusReason != WarningMessages.NO_ATTACHMENT
      && !model.StatusReason.StartsWith(WarningMessages.NHS_WORKLIST)
      && model.StatusReason != WarningMessages.INVALID_FILE_TYPE)
    {
      model.GroupedAuditList = 
        await GetAuditListGroupByPastReferralsAsync(id, model.NhsNumber);
    }

    DateTimeOffset? dateOfFirstContact = await _referralService.GetDateOfFirstContact(id);

    if (dateOfFirstContact != null)
    {
      model.MaxDateToDelay = dateOfFirstContact.Value
        .AddDays(_settings.MaxDaysAfterFirstContactToDelay).Date;
    }
    else
    {
      model.MaxDateToDelay = model.DateOfReferral
        .AddDays(_settings.MaxDaysAfterFirstContactToDelay).Date;
    }

    model.MaxDaysToDelay = (model.MaxDateToDelay.Value - DateTimeOffset.UtcNow).Days;

    return model;
  }

  [HttpPost]
  public async Task<ActionResult> ExportLetters(List<ReferralListItemModel>
    ListItems)
  {
    if (!ListItems.Any())
    {
      return new StatusCodeResult(204);
    }

    List<Guid> referralIds =
      ListItems.Where(l => l.Export == true)
      .Select(l => l.Id).ToList();

    DateTimeOffset dateLettersExported = DateTimeOffset.Now;
    
    byte[] data = await ReferralService
      .SendReferralLettersAsync(referralIds, dateLettersExported);

    if (data != null && data.Length > 0)
    {
      string mimeType = "text/csv";
      
      FileContentResult result = new FileContentResult(data, mimeType)
      {
        FileDownloadName =
          $"LetterExport_{dateLettersExported.ToString("ddMMMyyyyHHmmss")}.csv"
      };

      HttpContext.Response.Cookies.Append("Wmp.Export.Letters", "");
      return result;
    }
    return new StatusCodeResult(204);
  }

  [HttpPost]
  public async Task<ActionResult>
    CreateDischargeLetters(List<ReferralListItemModel>
    ListItems)
  {
    if (ListItems == null || !ListItems.Any())
    {
      return new StatusCodeResult(204);
    }

    List<Guid> referralIds =
      ListItems.Where(l => l.Export == true)
      .Select(l => l.Id).ToList();

    DateTimeOffset dateLettersExported = DateTimeOffset.Now;

    try
    {
      FileContentResult dischargeFile = await ReferralService
        .CreateDischargeLettersAsync(referralIds);

      if (dischargeFile == null)
      {
        return new StatusCodeResult(204);
      }
      else
      {
        HttpContext.Response.Cookies.Append("Wmp.Discharge.Letters", "");
        return dischargeFile;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex.Message);
      return new StatusCodeResult(500);
    }
  }


  [HttpPost]
  public async Task<IActionResult> UnableToContact(ReferralListItemModel model)
  {
    try
    {
      if (ModelState.IsValid)
      {
        IReferral referral = await ReferralService
          .UpdateStatusFromRmcCallToFailedToContactAsync(
            model.Id,
            model.StatusReason);

        model = _mapper.Map<IReferral, ReferralListItemModel>(referral);

        return GetRedirectDestination(model);
      }      
    }
    catch (ReferralInvalidStatusException ex)
    {
      ModelState.AddModelError(
        "There was a problem when updating status to UnableToContact.",
        $"{ex.Message} The unable to contact update has been cancelled and " +
        $"the referral refreshed to show the changes.");

      model = await LoadReferral(model.Id);
    }

    _logger.LogWarning(
      "ReferralListItemModel is invalid, {errors}",
      ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

    return View("ReferralView", model);
  }

  [HttpPost]
  public async Task<IActionResult> ConfirmEmail(ReferralListItemModel model)
  {
    ReferralListItemModel returnModel = model;

    if (ModelState.IsValid)
    {
      IReferral referral = await ReferralService
        .UpdateEmail(model.Id, model.Email);

      returnModel = await LoadReferral(model.Id);

      if (referral.HasProviders)
      {
        returnModel.Providers = GetProvidersFromReferral(referral);
      }
    }
    else
    {
      _logger.LogInformation("Model is NOT valid");
      string message = string.Join(" | ", ModelState.Values
          .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage));
      _logger.LogError(message);
    }

    SetPageTitle("Referral View");
    return View("ReferralView", returnModel);
  }

  [HttpPost]
  public async Task<IActionResult> ConfirmProvider(
    ReferralListItemModel model)
  {

    // update the referral with the selected service
    // if service fails then refresh this page and add an error

    if (ModelState.IsValid)
    {
      Business.Models.Referral businessModel =
        _mapper.Map<ReferralListItemModel, Business.Models.Referral>(model);

      Business.Models.IReferral businessReferral = await ReferralService
        .ConfirmProviderAsync(businessModel);

      model = _mapper.Map<Business.Models.IReferral, ReferralListItemModel>
        (businessReferral);
    }

    return GetRedirectDestination(model);
  }

  [HttpPost]
  public async Task<IActionResult> ConfirmDelay(ReferralListItemModel model)
  {
    try
    {
      if (ModelState.IsValid)
      {
        IReferral referral = await ReferralService.DelayReferralUntilAsync(
          model.Id,
          model.DelayReason,
          model.DelayUntil ?? DateTimeOffset.Now
            .AddDays(_settings.DefaultReferralCallbackDelayDays));

        model = _mapper.Map<IReferral, ReferralListItemModel>(referral);

        return GetRedirectDestination(model);
      }
    }
    catch (DelayReferralException ex)
    {
      ModelState.AddModelError(
        "There was a problem when delaying the referral.",
        $"{ex.Message} The delay has been cancelled and the referral " +
        "refreshed to show the changes.");

      model = await LoadReferral(model.Id);
    }

    _logger.LogWarning(
      "ReferralListItemModel is invalid, {errors}",
      ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

    return View("ReferralView", model);
  }

  [HttpPost]
  public async Task<IActionResult> ExceptionOverride(ReferralListItemModel model)
  {
    try
    {
      if (ModelState.IsValid)
      {
        if (string.IsNullOrEmpty(model.SelectedStatus))
        {
          throw new StatusOverrideException(
            $"{nameof(model.SelectedStatus)} cannot be null or empty.");
        }

        ReferralStatus selectedStatus = 
          (ReferralStatus)long.Parse(model.SelectedStatus);

        ReferralStatus acceptableStatusesFlag = 
          ReferralStatus.New 
          | ReferralStatus.RmcCall
          | ReferralStatus.Cancelled;

        if (!acceptableStatusesFlag.HasFlag(selectedStatus))
        {
          throw new StatusOverrideException(
            $"{nameof(selectedStatus)} must be in {acceptableStatusesFlag}.");
        }

        if (string.IsNullOrWhiteSpace(model.SelectedStatusReason))
        {
          throw new StatusOverrideException(
            $"{nameof(model.SelectedStatusReason)} must contain a " +
            $"valid reason.");
        }

        IReferral referral = await ReferralService.ExceptionOverride(
          model.Id, 
          selectedStatus,
          model.StatusReason);

        if (referral == null)
        {
          return RedirectToAction("exceptionList", "rmc", null);
        }

        model = _mapper.Map<IReferral, ReferralListItemModel>(referral);

        return GetRedirectDestination(model);
      }
    }
    catch(StatusOverrideException ex)
    {
      ModelState.AddModelError(
       "There was a problem when overriding the referral status.",
       $"{ex.Message} The override has been cancelled and the referral " +
       "refreshed to show the changes.");

      model = await LoadReferral(model.Id);
    }

    _logger.LogWarning(
      "ReferralListItemModel is invalid, {errors}",
      ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

    return View("ReferralView", model);
  }

  [HttpPost]
  public async Task<IActionResult> RejectToEreferrals(
    ReferralListItemModel model)
  {
    // Exception must be captured here because after update the model is no
    // longer an exception
    bool isException = model.IsException;

    if (string.IsNullOrWhiteSpace(model.StatusReason))
    {
      ModelState
        .AddModelError("StatusReason", "The status reason must be provided");
    }

    if (ModelState.IsValid)
    {
      Business.Models.IReferral businessReferral = await ReferralService
        .UpdateStatusToRejectedToEreferralsAsync(
          model.Id, model.StatusReason);

      model = _mapper.Map<IReferral, ReferralListItemModel>
        (businessReferral);
    }

    if (isException)
    {
      return new RedirectToActionResult("exceptionList", "rmc", null);
    }
    else
    {
      return GetRedirectDestination(model);
    }
  }

  [HttpPost]
  public async Task<IActionResult> RejectAfterProviderSelection(
    ReferralListItemModel model)
  {
    return await RejectReferral(
      model,
      ReferralService.RejectAfterProviderSelectionAsync);
  }

  [HttpPost]
  public async Task<IActionResult> RejectBeforeProviderSelection(
    ReferralListItemModel model)
  {
    return await RejectReferral(
      model,
      ReferralService.RejectBeforeProviderSelectionAsync);
  }

  private async Task<IActionResult> RejectReferral(
    ReferralListItemModel model,
    Func<Guid, string, Task<IReferral>> rejectionFunction)
  {
    try
    {
      if (ModelState.IsValid)
      {
        IReferral businessReferral = await rejectionFunction(
          model.Id,
          model.StatusReason);

        model = _mapper.Map<IReferral, ReferralListItemModel>(businessReferral);
      }

      return GetRedirectDestination(model);
    }
    catch (Exception ex)
    {
      ModelState.AddModelError(
        "Rejection Cancelled",
        $"There was a problem when rejecting the referral: {ex.Message}. The " +
        $"rejection has been cancelled.");

      model = await LoadReferral(model.Id);

      return View("ReferralView", model);
    }
  }

  [HttpPost]
  public async Task<IActionResult> AddToRmcCallList(
    ReferralListItemModel model)
  {
    if (ModelState.IsValid)
    {
      IReferral businessReferral = await ReferralService
        .UpdateStatusToRmcCallAsync(model.Id);

      model = _mapper
        .Map<IReferral, ReferralListItemModel>(businessReferral);
    }

    return GetRedirectDestination(model);
  }

  [HttpPost]
  public async Task<IActionResult> ConfirmEthnicity(
    ReferralListItemModel model)
  {
    try
    {
      if (ModelState.IsValid)
      {
        Business.Models.Ethnicity ethnicity =
          await _ethnicityService.GetByMultiple(model.SelectedServiceUserEthnicity)
          ?? throw new EthnicityNotFoundException("An ethnicity with a DisplayName of " +
          $"{model.SelectedServiceUserEthnicity} could not be found.");

        IReferral referral = await ReferralService.UpdateEthnicity(model.Id, ethnicity);

        model = _mapper.Map<IReferral, ReferralListItemModel>(referral);

        if (referral != null)
        {
          model.Bmi = referral.CalculatedBmiAtRegistration.Value;
          if (referral.IsBmiTooLow)
          {
            model.StatusReason = $"The service user's BMI of {model.Bmi} is " +
              $"below the minimum of {model.SelectedEthnicGroupMinimumBmi} " +
              $"for the selected ethnic group of {ethnicity.TriageName}.";
          }
          else
          {
            model.Providers = GetProvidersFromReferral(referral);
          }
        }

        model.SelectedEthnicity = referral.Ethnicity.ToString();
        model.SelectedServiceUserEthnicity = referral.ServiceUserEthnicity;
        model.SelectedServiceUserEthnicityGroup = referral.ServiceUserEthnicityGroup;
        model.ServiceUserEthnicityGroupList = await GetServiceUserEthnicityGroupList();
        model.ServiceUserEthnicityList = await GetServiceUserEthnicityList(
          referral.ServiceUserEthnicityGroup);
        model.AuditList = await GetAuditListAsync(model.Id);

        _logger.LogTrace("Ethnicity Updated");
      }
      else
      {
        _logger.LogInformation("Model is NOT valid");
        string message = string.Join(" | ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        _logger.LogError(message);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex.Message);
    }

    model.ReferralStatusReasons = await ReferralService.GetRmcRejectedReferralStatusReasonsAsync();

    return View("ReferralView", model);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateDateOfBirth(
    ReferralListItemModel model)
  {
    ReferralListItemModel updatedModel = model;

    if (ModelState.IsValid)
    {
      await ReferralService.UpdateDateOfBirth(model.Id, model.DateOfBirth);

      updatedModel = await LoadReferral(model.Id);

      _logger.LogTrace("Date Of Birth Updated");
    }
    else
    {
      string message = string.Join(" | ", ModelState.Values
        .SelectMany(v => v.Errors)
        .Select(e => e.ErrorMessage));

      _logger.LogError($"Model is NOT valid: {message}");
    }

    updatedModel.ReferralStatusReasons = await ReferralService
      .GetRmcRejectedReferralStatusReasonsAsync();

    return View("ReferralView", updatedModel);
  }

  [HttpPost]
  public async Task<IActionResult> UpdateMobileNumber(
    ReferralListItemModel model)
  {
    ReferralListItemModel updatedModel = model;

    if (ModelState.IsValid)
    {
      await ReferralService.UpdateMobile(model.Id, model.Mobile);

      updatedModel = await LoadReferral(model.Id);

      _logger.LogTrace("Mobile Number Updated");
    }
    else
    {
      _logger.LogInformation("Model is NOT valid");
      string message = string.Join(
        " | ", 
        ModelState.Values
          .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage));
      _logger.LogError(message);
    }

    updatedModel.ReferralStatusReasons = await ReferralService
      .GetRmcRejectedReferralStatusReasonsAsync();

    return View("ReferralView", updatedModel);
  }

  [AllowAnonymous]
  [HttpPost]
  [Consumes("application/json")]
  public async Task<IActionResult> EmailElectiveCareLinkToServiceUser(Guid referralId)
  {
    if (referralId == Guid.Empty)
    {
      throw new ArgumentException("Must provide a valid referral Id.", nameof(referralId));
    }

    try
    {
      Referral referral =
        _mapper.Map<Referral>(await _referralService.GetReferralEntity(referralId));

      string serviceUserLinkId = await ReferralService.GetServiceUserLinkIdAsync(referral);
      string link =
        WebUtility.HtmlEncode($"{_settings.ElectiveCareServiceUserHubLink}?u={serviceUserLinkId}");

      MessageQueue messageQueue = new(
        apiKeyType: ApiKeyType.ElectiveCareNewUser,
        clientReference: referral.Id.ToString(),
        emailTo: referral.Email,
        emailReplyToId: _settings.ReplyToId,
        personalisationList: EmailElectiveCareLinkPersonalisation.ExpectedPersonalisation,
        personalisations: new Dictionary<string, dynamic>
        {
          {
            NotificationPersonalisations.GIVEN_NAME,
            referral.GivenName
          },
          {
            NotificationPersonalisations.LINK,
            link
          }
        },
        templateId: Guid.Parse(_settings.ElectiveCareServiceUserHubLinkTemplateId),
        type: MessageType.Email,
        linkId: serviceUserLinkId);

      HttpResponseMessage response = await _notificationService.SendMessageAsync(messageQueue);

      string result = await response.Content.ReadAsStringAsync();

      EmailResponse emailElectiveCareLinkResponse =
        System.Text.Json.JsonSerializer.Deserialize<EmailElectiveCareLinkResponse>(result);

      ObjectResult emailVerification =
        (ObjectResult)await EmailVerification(emailElectiveCareLinkResponse.Id);

      emailVerification.Value = link;
      return emailVerification;
    }
    catch (Exception ex)
    {
      if (ex is ProcessAlreadyRunningException)
      {
        return Conflict("Request coincided with another user's request. Please retry shortly.");
      }

      return BadRequest(ex.Message);
    }
  }

  [AllowAnonymous]
  [HttpPost]
  [Consumes("application/json")]
  public async Task<IActionResult> EmailProviderListToServiceUser(string ubrn, Guid? referralId)
  {
    ArgumentNullException.ThrowIfNull(ubrn);
    ArgumentNullException.ThrowIfNull(referralId);

    try
    {
      IReferral referral = await ReferralService
        .GetReferralWithTriagedProvidersById(referralId.Value);

      if (referral == null)
      {
        return BadRequest($"Referral not found.");
      }

      if (referral.Providers == null || referral.Providers.Count == 0)
      {
        return BadRequest($"Referral has no providers.");
      }

      StringBuilder sb = new();
      string pattern = @"[^a-zA-Z0-9_]";
      string substitution = @"";
      RegexOptions options = RegexOptions.Multiline;
      Regex regex = new(pattern, options);

      for (int i = 0; i < referral.Providers.Count; i++)
      {
        string filename = regex
          .Replace(referral.Providers[i].Name, substitution);

        string link = WebUtility.HtmlEncode(
          $"https://{Request.Host.Value}/" +
          $"{_settings.ProviderLinkEndpoint}/" +
          $"{filename}_" +
          $"{referral.OfferedCompletionLevel}" +
          ".pdf");

        sb.AppendLine($"Provider {i + 1}.  {referral.Providers[i].Name}.  {link}");
      }

      string serviceUserLinkId = await ReferralService.GetServiceUserLinkIdAsync(referral);

      MessageQueue messageQueue = new(
        apiKeyType: ApiKeyType.ProviderList,
        clientReference: referral.Id.ToString(),
        emailTo: referral.Email,
        emailReplyToId: _settings.ReplyToId,
        personalisationList: EmailProvidersListPersonalisation.ExpectedPersonalisation,
        personalisations: new Dictionary<string, dynamic>
        {
          {
            NotificationPersonalisations.GIVEN_NAME,
            referral.GivenName
          },
          {
            NotificationPersonalisations.PROVIDER_COUNT,
            referral.Providers.Count.ToString()
          },
          {
            NotificationPersonalisations.PROVIDER_LIST,
            sb.ToString()
          },
          {
            NotificationPersonalisations.LINK,
            $"{_settings.ServiceUserHubLink}?u={serviceUserLinkId}"
          }
        },
        templateId: Guid.Parse(_settings.ProviderByEmailTemplateId),
        type: MessageType.Email,
        linkId: serviceUserLinkId);

      HttpResponseMessage response = await _notificationService.SendMessageAsync(messageQueue);

      string result = await response.Content.ReadAsStringAsync();

      EmailResponse emailProvidersResponse =
        System.Text.Json.JsonSerializer.Deserialize<EmailProvidersListResponse>(result);

      return await EmailVerification(emailProvidersResponse.Id);
    }
    catch (Exception ex)
    {
      if (ex is ProcessAlreadyRunningException)
      {
        return Conflict("Request coincided with another user's request. Please retry shortly.");
      }

      return BadRequest(ex.Message);
    }
  }

  protected async Task<IActionResult> EmailVerification(string messageId)
  {
    try
    {
      EmailResponse emailResponse = new()
      {
        Id = messageId,
        Status = Actions.CREATED
      };

      int loopCount = 0;
      while(emailResponse.Status == Actions.SENDING || emailResponse.Status == Actions.CREATED)
      {
        if (loopCount >= 5)
        {
          break;
        }

        emailResponse = await MessageVerification(messageId);

        await Task.Delay(1000);
        loopCount++;
      }

      if (loopCount >=5 && 
        (emailResponse.Status == Actions.SENDING || emailResponse.Status == Actions.CREATED))
      {
        throw new ReferralContactEmailException($"Email message verification has been delayed. " +
          $"Current status: {emailResponse.Status}.");
      }

      if (emailResponse.Status != Actions.DELIVERED)
      {
        throw new ReferralContactEmailException($"Email message of id {messageId} not delivered. " +
          $"Status: {emailResponse.Status}.");
      }

      return Ok(emailResponse);
    }
    catch (Exception ex)
    {
      return BadRequest(ex.Message);
    }
  }

  [HttpGet]
  public async Task<IActionResult> ProviderDetailsEmailHistory([FromQuery] Guid referralId)
  {
    if (referralId == Guid.Empty)
    {
      return Problem(
        detail: "Must provide a valid referral Id.",
        statusCode: StatusCodes.Status400BadRequest);
    }

    try
    {
      HttpResponseMessage response =
        await _notificationService.GetEmailHistory(referralId.ToString());

      if (response.StatusCode == HttpStatusCode.NotFound)
      {
        return NoContent();
      }

      string content = await response.Content.ReadAsStringAsync();

      JsonSerializerOptions jsonSerializerOptions = new()
      {
        PropertyNameCaseInsensitive = true
      };

      ProviderDetailsEmailHistoryItem[] emailHistory =
        System.Text.Json.JsonSerializer.Deserialize<ProviderDetailsEmailHistoryItem[]>(
          content,
          jsonSerializerOptions);

      return Ok(System.Text.Json.JsonSerializer.Serialize(
        emailHistory.OrderByDescending(x => x.Created)));
    }
    catch (Exception ex)
    {
      if (ex is not NotificationProxyException)
      {
        _logger.LogError(ex.Message);
      } 

      return Problem(
        detail: "An error occurred when retrieving email history.",
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private async Task<EmailResponse> MessageVerification(
    string messageId)
  {
    HttpResponseMessage response =
       await _notificationService.GetMessageVerification(messageId);
    string result = await response.Content.ReadAsStringAsync();
    return System.Text.Json.JsonSerializer.Deserialize<EmailResponse>(result);
  }

  private List<Provider> GetProvidersFromReferral(IReferral referral)
  {
    List<Provider> providers = new List<Provider>();

    if (referral.HasProviders)
    {
      foreach (Business.Models.Provider provider in referral.Providers)
      {
        Provider providerModel =
          _mapper.Map<Business.Models.Provider, Provider>(provider);

        if (referral.TriagedCompletionLevel == "2")
          providerModel.Summary = provider.Summary2;
        if (referral.TriagedCompletionLevel == "3")
          providerModel.Summary = provider.Summary3;

        providers.Add(providerModel);
      }
    }
    return providers;
  }

  public IActionResult Privacy()
  {
    return View();
  }

  [AllowAnonymous]
  [ResponseCache(
    Duration = 0,
    Location = ResponseCacheLocation.None,
    NoStore = true
  )]
  public IActionResult Error()
  {
    return View(
      new ErrorViewModel
      {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
      }
    );
  }

  [HttpGet]
  public async Task<IActionResult> ServiceUserEthnicityGroupMembers(string ethnicityGroup)
  {
    IEnumerable<Business.Models.Ethnicity> ethnicities = 
      await _ethnicityService.GetEthnicityGroupMembersAsync(ethnicityGroup);

    if (ethnicities.Any())
    {
      return Ok(ethnicities.Select(e => e.DisplayName).ToArray());
    }

    return Problem(
      detail: "Invalid ethnicityGroup value.",
      statusCode: StatusCodes.Status400BadRequest);
  }

  private string[] GetExceptionStatuses()
  {
    return new string[] {
      ReferralStatus.Exception.ToString(),
    };
  }

  private string[] GetRejectionStatuses =>
    RejectionListAttributeExtension
      .RejectionStatusItems<ReferralStatus>()
      .Select(t => (ReferralStatus)Enum.Parse(typeof(ReferralStatus),
        t.Name)).Select(t => t.ToString()).ToArray();

  private string[] GetRmcStatuses()
  {
    return new string[] {
      ReferralStatus.RmcCall.ToString(),
      ReferralStatus.ChatBotTransfer.ToString()
    };
  }

  private string[] GetRmcLetterStatuses()
  {
    return new string[] {
      ReferralStatus.Letter.ToString()
    };
  }

  private string[] GetReadyForDischargeStatuses()
  {
    return new string[] {
      ReferralStatus.ProviderCompleted.ToString()
    };
  }

  private async Task<ReferralListModel> SearchCurrentReferrals(
    ReferralSearchModel model)
  {
    _logger.LogInformation("ReferralList (GET) View");

    model = model ?? new ReferralSearchModel();

    ReferralSearch referralSearch = _mapper
      .Map<ReferralSearchModel, ReferralSearch>(model);

    IReferralSearchResponse response =
      await ReferralService.Search(referralSearch);

    IEnumerable<Referral> businessReferrals = 
      (IEnumerable<Referral>)response.Referrals;

    List<ReferralListItemModel> referrals =_mapper.Map<
      List<Referral>, 
      List<ReferralListItemModel>>(businessReferrals.ToList());

    ReferralListModel referralListModel = new();
    referralListModel.ActiveUser = JsonConvert
      .SerializeObject(GetUserDetails());
    referralListModel.ListItems = referrals;
    referralListModel.Count = response.Count;

    return referralListModel;
  }

  private void SetPageTitle(string title)
  {
    ViewData["Title"] = title;
  }

  private async Task<ReferralListItemModel> GetReferralFromService(Guid id)
  {
    IReferral businessReferral = await ReferralService
      .GetReferralWithTriagedProvidersById(id);

    ReferralListItemModel model = _mapper
      .Map<IReferral, ReferralListItemModel>(businessReferral);

    model.Providers = GetProvidersFromReferral(businessReferral);

    if (string.IsNullOrWhiteSpace(model.ProviderName) &&
        model.ProviderId != Guid.Empty)
    {
      model.IsProviderPreviouslySelected = true;
      model.ProviderName =
        await ReferralService.GetProviderNameAsync(model.ProviderId);
    }

    model.ActiveUser = JsonConvert.SerializeObject(GetUserDetails());
    model.ServiceUserEthnicityGroupList = await GetServiceUserEthnicityGroupList();

    if (businessReferral.Ethnicity != null)
    {
      model.SelectedEthnicity = businessReferral.Ethnicity;
      model.SelectedServiceUserEthnicity = businessReferral.ServiceUserEthnicity ?? null;
      model.SelectedServiceUserEthnicityGroup = businessReferral.ServiceUserEthnicityGroup ?? null;
    }

    if (model.SelectedServiceUserEthnicityGroup != null)
    {
      model.ServiceUserEthnicityList = 
        await GetServiceUserEthnicityList(model.SelectedServiceUserEthnicityGroup);
    }

    return model;
  }

  private async Task<IEnumerable<ReferralAuditListItemModel>>
    GetAuditListAsync(Guid id)
  {
    IEnumerable<ReferralAuditListItemModel> auditItems = null;
    List<ReferralAudit> auditList = null;
    try
    {
      auditList = await _referralService
        .GetReferralAuditForServiceUserAsync(id);

      if (!auditList.Any())
      {
        return new List<ReferralAuditListItemModel>();
      }

      auditItems = _mapper
        .Map<IEnumerable<ReferralAuditListItemModel>>(auditList);

      IEnumerable<ReferralAuditListItemModel> noDuplicates = auditItems
        .Distinct().ToList();

      return noDuplicates;
    }
    catch (Exception ex)
    {
      string auditListString = string
        .Join(',', auditList.Select(a => a.AuditId).ToList());

      string auditItemsString = string
        .Join(',', auditItems.Select(a => a.AuditId).ToList());

      _logger.LogError(
        $"Referral Id: '{id}', Audit List: '{auditListString}'" +
          $"Audit Items: '{auditItemsString}'",
        ex);
      return auditItems;
    }
  }

  private async Task<ReferralAuditListGroupModel>
    GetAuditListGroupByPastReferralsAsync(Guid id, string nhsNumber)
  {
    ReferralAuditListGroupModel referralAuditListGroupModel 
      = new(id, nhsNumber);

    List<Guid> referralIds = 
      await _referralService.GetReferralIdsByNhsNumber(nhsNumber);

    foreach(Guid referralId in referralIds)
    {
      if (referralId == id)
      {
        continue;
      }

      referralAuditListGroupModel.PastItems.Add(
        referralId,  
        await GetAuditListAsync(referralId));
    }

    return referralAuditListGroupModel;
  }

  private IActionResult GetRedirectDestination(ReferralListItemModel model)
  {
    RedirectToActionResult redirect =
      new RedirectToActionResult("referralList", "rmc", null);

    if (model.IsException)
    {
      redirect = new RedirectToActionResult("exceptionList", "rmc", null);
    }
    else if (model.IsVulnerable)
    {
      redirect = new RedirectToActionResult("vulnerableList", "rmc", null);
    }

    return redirect;
  }

  private UserDetailsModel GetUserDetails()
  {
    UserDetailsModel userDetails = new UserDetailsModel();

    ClaimsPrincipal principal = HttpContext.User as ClaimsPrincipal;
    if (principal != null)
    {
      Claim objectIdentifierClaim = principal.Claims
        .Where(c => c.Type == ClaimConstants.ObjectId)
        .FirstOrDefault();

      if (objectIdentifierClaim != null)
      {
        userDetails.UserIdentifier = objectIdentifierClaim.Value;
      }

      Claim givenNameClaim = principal.Claims
        .Where(c => c.Type == ClaimTypes.GivenName)
        .FirstOrDefault();

      Claim familyNameClaim = principal.Claims
        .Where(c => c.Type == ClaimTypes.Surname)
        .FirstOrDefault();

      if (givenNameClaim != null && familyNameClaim != null)
      {
        userDetails.UserInitials =
          givenNameClaim.Value.Substring(0, 1).ToUpper() +
          familyNameClaim.Value.Substring(0, 1).ToUpper();
      }
    }

    return userDetails;
  }

  private async Task<List<SelectListItem>> GetServiceUserEthnicityList(
    string serviceUserEthnicityGroup)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(serviceUserEthnicityGroup);

    IEnumerable<Business.Models.Ethnicity> ethnicities = 
      await _ethnicityService.GetEthnicityGroupMembersAsync(serviceUserEthnicityGroup);

    List<SelectListItem> serviceUserEthnicityList = [];

    foreach (Business.Models.Ethnicity ethnicity in ethnicities)
    {
      serviceUserEthnicityList.Add(new()
      {
        Value = ethnicity.DisplayName,
        Text = ethnicity.DisplayName,
      });
    }

    return serviceUserEthnicityList;
  }

  private async Task<List<SelectListItem>> GetServiceUserEthnicityGroupList()
  {
    IEnumerable<string> groupNames = await _ethnicityService.GetEthnicityGroupNamesAsync();

    List<SelectListItem> ethnicityList = [];

    foreach (string groupName in groupNames)
    {
      ethnicityList.Add(new()
      {
        Value = groupName,
        Text = groupName
      });
    }

    return ethnicityList;
  }
}
