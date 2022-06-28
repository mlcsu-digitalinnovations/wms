using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Services;
using WmsHub.Common.Extensions;
using WmsHub.Ui.Models;
using Ethnicity = WmsHub.Business.Enums.Ethnicity;
using Provider = WmsHub.Ui.Models.Provider;

namespace WmsHub.Ui.Controllers
{
  [Authorize(Policy = "RmcUiDomainUsers")]
  public class RmcController : Controller
  {
    private readonly ILogger<RmcController> _logger;
    private IReferralService _referralService;
    private IEthnicityService _ethnicityService;
    private readonly IMapper _mapper;
    private readonly IProviderService _providerService;
    private WebUiSettings _settings;

    public RmcController(
      ILogger<RmcController> logger,
      IReferralService referralService,
      IEthnicityService ethnicityService,
      IMapper mapper,
      IOptions<WebUiSettings> options,
      IProviderService providerService
    )
    {
      _logger = logger;
      _referralService = referralService;
      _ethnicityService = ethnicityService;
      _mapper = mapper;
      _providerService = providerService;
      _settings = options.Value;
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

      model.Providers = _mapper
        .Map<IEnumerable<Models.ProviderInfo>>(providerInfos);


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
      searchModel.Statuses = GetRejectionStatuses();

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
      _logger.LogInformation("ReferralView (GET) View #" + id.ToString());

      ReferralListItemModel model = await GetReferralFromService(id);

      model.AuditList = await GetAuditListAsync(model.Id);

      if (model.AuditList.Any())
      {
        var lastDelayAudit = model.AuditList
          .OrderByDescending(t => t.ModifiedAt).FirstOrDefault(t =>
            t.Status == ReferralStatus.RmcDelayed.ToString());
        if (lastDelayAudit != null)
        {
          model.DelayFrom = lastDelayAudit.ModifiedAt;
          model.DelayUntil = lastDelayAudit.DateToDelayUntil;
        }
      }

      // check for exception status
      if (GetExceptionStatuses().Contains(model.Status))
        model.IsException = true;

      SetPageTitle("Referral View");

      return View(model);
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

      var data = await ReferralService
        .SendReferralLettersAsync(referralIds, dateLettersExported);

      if (data != null && data.Length > 0)
      {
        string mimeType = "text/csv";

        var result = new FileContentResult(data, mimeType)
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
      if (ModelState.IsValid)
      {
        IReferral businessReferral = await ReferralService
          .UpdateStatusFromRmcCallToFailedToContactAsync(
            model.Id, 
            model.StatusReason);
                
        model = _mapper.Map<IReferral, ReferralListItemModel>(businessReferral);
      }

      return GetRedirectDestination(model);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(ReferralListItemModel model)
    {
      if (ModelState.IsValid)
      {
        IReferral referral = await ReferralService
          .UpdateEmail(model.Id, model.Email);

        model.EthnicityList = await GetEthnicityList();

        model.AuditList = await GetAuditListAsync(model.Id);

        if (referral.HasProviders)
        {
          model.Providers = GetProvidersFromReferral(referral);
        }
      }
      else
      {
        _logger.LogInformation("Model is NOT valid");
        var message = string.Join(" | ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        _logger.LogError(message);
      }

      return View("ReferralView", model);
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
      if (ModelState.IsValid)
      {

        Business.Models.IReferral businessReferral = await ReferralService
          .DelayReferralUntilAsync(model.Id, model.DelayReason,
            model.DelayUntil ??
            DateTimeOffset.Now.AddDays(_settings
              .DefaultReferralCallbackDelayDays));

        model = _mapper.Map<Business.Models.IReferral, ReferralListItemModel>
          (businessReferral);
      }

      return GetRedirectDestination(model);
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
    public async Task<IActionResult> AddToRmcCallList(
      ReferralListItemModel model)
    {
      if (ModelState.IsValid)
      {
        IReferral businessReferral = await ReferralService
          .UpdateStatusToRmcCallAsync(model.Id);

        model = _mapper.Map<IReferral, ReferralListItemModel>(businessReferral);
      }

      return GetRedirectDestination(model);
    }



    [HttpPost]
    public async Task<IActionResult> ConfirmEthnicity(
      ReferralListItemModel model)
    {
      if (ModelState.IsValid)
      {
        if (!model.SelectedEthnicity.TryParseToEnumName(
          out Ethnicity ethnicityFromEnum))
        {
          ethnicityFromEnum = Ethnicity.Other;
        }

        IReferral referral = await ReferralService
          .UpdateEthnicity(model.Id, ethnicityFromEnum);

        if (referral != null && !referral.IsBmiTooLow)
        {
          model.Providers = GetProvidersFromReferral(referral);
        }
        model.EthnicityList = await GetEthnicityList();
        model.AuditList = await GetAuditListAsync(model.Id);
        model.Bmi = referral.CalculatedBmiAtRegistration.Value;
        model.IsBmiTooLow = referral.IsBmiTooLow;
        model.SelectedEthnicGroupMinimumBmi = referral
          .SelectedEthnicGroupMinimumBmi;

        _logger.LogTrace("Ethnicity Updated");
      }
      else
      {
        _logger.LogInformation("Model is NOT valid");
        var message = string.Join(" | ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        _logger.LogError(message);
      }

      return View("ReferralView", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDateOfBirth(
      ReferralListItemModel model)
    {
      ReferralListItemModel updatedModel = model;

      if (ModelState.IsValid)
      {
        IReferral referral = await ReferralService
          .UpdateDateOfBirth(model.Id, model.DateOfBirth);       

        if (referral != null)
        {
          updatedModel = _mapper
            .Map<IReferral, ReferralListItemModel>(referral);

          updatedModel.Providers = GetProvidersFromReferral(referral);

          if (string.IsNullOrWhiteSpace(updatedModel.ProviderName) 
            && updatedModel.ProviderId != Guid.Empty)
          {
            updatedModel.IsProviderPreviouslySelected = true;
            updatedModel.ProviderName = await ReferralService
              .GetProviderNameAsync(updatedModel.ProviderId);
          }

          updatedModel.EthnicityList = await GetEthnicityList();
          updatedModel.AuditList = await GetAuditListAsync(model.Id);
        }

        _logger.LogTrace("Date Of Birth Updated");
      }
      else
      {
        var message = string.Join(" | ", ModelState.Values
          .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage));
        _logger.LogError($"Model is NOT valid: {message}");
      }

      return View("ReferralView", updatedModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMobileNumber(
      ReferralListItemModel model)
    {
      ReferralListItemModel updatedModel = model;

      if (ModelState.IsValid)
      {
        IReferral referral = await ReferralService
          .UpdateMobile(model.Id, model.Mobile);

        if (referral != null)
        {
          updatedModel = _mapper
            .Map<IReferral, ReferralListItemModel>(referral);

          updatedModel.Providers = GetProvidersFromReferral(referral);

          if (string.IsNullOrWhiteSpace(updatedModel.ProviderName)
            && updatedModel.ProviderId != Guid.Empty)
          {
            updatedModel.IsProviderPreviouslySelected = true;
            updatedModel.ProviderName =
              await ReferralService.GetProviderNameAsync(updatedModel.ProviderId);
          }

          updatedModel.EthnicityList = await GetEthnicityList();
          updatedModel.AuditList = await GetAuditListAsync(model.Id);
        }

        _logger.LogTrace("Mobile Number Updated");
      }
      else
      {
        _logger.LogInformation("Model is NOT valid");
        var message = string.Join(" | ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        _logger.LogError(message);
      }

      return View("ReferralView", updatedModel);
    }

    private List<Provider> GetProvidersFromReferral(IReferral referral)
    {
      var providers = new List<Provider>();

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

    private string[] GetExceptionStatuses()
    {
      return new string[] {
        ReferralStatus.Exception.ToString(),
      };
    }

    private string[] GetRejectionStatuses()
    {
      return new string[] {
        ReferralStatus.FailedToContact.ToString(),
        ReferralStatus.ProviderRejected.ToString(),
        ReferralStatus.ProviderDeclinedByServiceUser.ToString(),
        ReferralStatus.ProviderTerminated.ToString()
      };
    }

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

      if (string.IsNullOrWhiteSpace(model.ProviderName) &&
          model.ProviderId != Guid.Empty)
      {
        model.IsProviderPreviouslySelected = true;
        model.ProviderName =
          await ReferralService.GetProviderNameAsync(model.ProviderId);
      }

      model.ActiveUser = JsonConvert.SerializeObject(GetUserDetails());
      model.EthnicityList = await GetEthnicityList();

      if (businessReferral.Ethnicity != null)
      {
        model.SelectedEthnicity = model.EthnicityList
          .Where(e => e.Text.StartsWith(businessReferral.Ethnicity))
          .FirstOrDefault()
          ?.Value ?? null;
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

    private async Task<List<SelectListItem>> GetEthnicityList()
    {
      // use a service to get a list of ethnicity codes
      IEnumerable<Business.Models.Ethnicity> businessModel =
        await _ethnicityService.Get();
      List<SelectListItem> ethnicityList = new List<SelectListItem>();

      IEnumerable<IGrouping<string, Business.Models.Ethnicity>> groups =
        businessModel
          .OrderBy(e => e.GroupOrder)
          .GroupBy(e => e.GroupName);

      foreach (IGrouping<string, Business.Models.Ethnicity> group in groups)
      {
        ethnicityList.Add(new SelectListItem()
        {
          Value = group.ElementAt(0).TriageName,
          Text = group.Key
        });
      }
      return ethnicityList;
    }
  }
}
