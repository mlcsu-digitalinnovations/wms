using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Helpers;
using WmsHub.Ui.Models;
using ProviderChoiceModel = WmsHub.Ui.Models.ProviderChoiceModel;

namespace WmsHub.Ui.Controllers
{
  [AllowAnonymous]
  public class ServiceUserController : BaseController
  {
    private const string SESSION_KEY_TEXT_MESSAGE_TOKEN = "TextMessageToken";
    private const string SESSION_KEY_PROVIDER_ID = "ProviderId";
    private const string SESSION_KEY_UBRN = "Ubrn";
    private const string SESSION_KEY_USER_SID = "User_Sid";
    private const string WMSHUB_UI_USER_SID =
      "86c815f9-208b-41f5-9e46-2af83253c3d1";
    private static readonly string DONT_CONTACT_BY_EMAIL =
      Constants.DO_NOT_CONTACT_EMAIL;

    private readonly ILogger<ServiceUserController> _logger;
    private readonly IConfiguration _config;

    private readonly IEthnicityService _ethnicityService;
    private readonly IProviderService _providerService;
    private readonly IMapper _mapper;
    private readonly IReferralService _referralService;

    private IReferralService ReferralService
    {
      get
      {
        // Add Sid claim for current user
        string userSid = HttpContext?.Session?.GetString(SESSION_KEY_USER_SID);
        if (string.IsNullOrWhiteSpace(userSid))
        {
          userSid = WMSHUB_UI_USER_SID;
        }
        User.AddIdentity(new ClaimsIdentity(new List<Claim>()
        {
          new Claim(ClaimTypes.Sid, userSid)
        }));
        _referralService.User = User;

        return _referralService;
      }
    }

    public ServiceUserController(
      ILogger<ServiceUserController> logger,
      IConfiguration configuration,
      IReferralService referralService,
      IEthnicityService ethnicityService,
      IProviderService providerService,
      IMapper mapper,
      IOptions<WebUiSettings> options
    ) : base(options.Value)
    {
      _logger = logger;
      _config = configuration;
      _ethnicityService = ethnicityService;
      _providerService = providerService;
      _mapper = mapper;
      _referralService = referralService;
    }

    [Route("{controller}")]
    [Route("{controller}/welcome")]
    public IActionResult Index() => View();

    [Route("{controller}/welcome/{textId}")]
    public async Task<IActionResult> Welcome(string textId)
    {
      WelcomeModel model;
      HttpContext.Session.SetString(SESSION_KEY_TEXT_MESSAGE_TOKEN, textId);
      try
      {
        KeyValuePair<Guid, IReferral>? messageQueueIdReferral = 
          await ReferralService.GetMessageQueue(textId);

        if (messageQueueIdReferral == null)
        {
          IReferral referral = await ReferralService
            .GetServiceUserReferralAsync(textId);

          // Set the user sid to the text message id so it can be used as the user 
          // sid for all database updates.
          string textMessageId = referral
            .TextMessages
            .Where(t => t.ServiceUserLinkId == textId)
            .OrderBy(t => t.Sent)
            .LastOrDefault()
            ?.Id
            .ToString();
          HttpContext.Session.SetString(SESSION_KEY_USER_SID, textMessageId);

          model = _mapper.Map<WelcomeModel>(referral);
        }
        else
        {
          // Set the user sid to the text message id so it can be used as the user 
          // sid for all database updates
          HttpContext.Session.SetString(
            SESSION_KEY_USER_SID,
            messageQueueIdReferral.Value.Key.ToString());

          model = _mapper.Map<WelcomeModel>(messageQueueIdReferral.Value.Value);
        }

        HttpContext.Session.SetString(SESSION_KEY_UBRN, model.Ubrn);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "getting referral details");
      }

      return View(model);
    }

    [HttpPost]
    [Route("{controller}/get-started/{id}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
      NoStore = true)]
    public async Task<IActionResult> VerifyUserDoB(VerificationModel model)
    {
      try
      {
        model.Token =
          HttpContext.Session.GetString(SESSION_KEY_TEXT_MESSAGE_TOKEN);

        IReferral referral = await GetReferralWithValidation(model.Id);

        TextMessage textMessage = referral.TextMessages
          .Where(t => t.ServiceUserLinkId == model.Token)
          .OrderBy(x => x.Sent)
          .LastOrDefault() ?? throw new TextMessageNotFoundException(
            $"Unable to find textMessage with ServiceUserLinkId {model.Token}.");

        int currentAttempts = textMessage.DobAttempts;
        model.Attempt = currentAttempts;

        if (currentAttempts >= 3)
        {
          await ReferralService.ExpireTextMessageDueToDobCheckAsync(
            HttpContext.Session.GetString(SESSION_KEY_TEXT_MESSAGE_TOKEN));

          HttpContext.Session.Remove(SESSION_KEY_TEXT_MESSAGE_TOKEN);

          throw new TextMessageExpiredByDoBCheckException(
            "The total allowed date of birth attempts has been exceeded.");
        }

        if (!ModelState.IsValid)
        {
          ModelState.Remove("Day");
          ModelState.Remove("Month");
          ModelState.Remove("Year");

          return View("Verification", model);
        }
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "getting started");
      }

      return await GetVerificationResult(model);
    }

    private async Task<IActionResult> GetVerificationResult(
      VerificationModel model)
    {
      ModelState.Remove("Attempt");
      IActionResult redirect = View("Verification", model);

      try
      {
        IReferral referral = await GetReferralWithValidation(model.Id);

        ServiceUserModel serviceUser = _mapper.Map<ServiceUserModel>(referral);

        serviceUser.Source = string.IsNullOrWhiteSpace(referral.ReferralSource)
          ? ReferralSource.GpReferral
          : Enum.Parse<ReferralSource>(referral.ReferralSource);

        DateTime dateOfBirth = serviceUser.DateOfBirth.Value.Date;

        // compare the date of birth
        if (dateOfBirth.Day == model.Day &&
          dateOfBirth.Month == model.Month &&
          dateOfBirth.Year == model.Year)
        {

          HttpContext.Session.SetString("ReferralId", model.Id.ToString());

          if (serviceUser.Source == ReferralSource.SelfReferral)
          {
            await ReferralService.TriageReferralUpdateAsync(model.Id);
            redirect = NavigateToAction("contact-preference", model.Id);
          }
          else
          {
            redirect = NavigateToAction("email-confirmation", model.Id);
          }
        }
        else
        {
          model.IncorrectDOBEntered = true;

          TextMessage textMessage = referral.TextMessages
            .Where(t => t.IsActive)
            .Where(t => t.ServiceUserLinkId == model.Token)
            .OrderBy(t => t.Sent)
            .LastOrDefault() ?? throw new TextMessageNotFoundException(
              $"Unable to find textMessage with ServiceUserLinkId {model.Token}.");

          textMessage.DobAttempts += 1;
          model.Attempt = textMessage.DobAttempts;

          await ReferralService.UpdateDobAttemptsAsync(textMessage.Id, textMessage.DobAttempts);

          if (model.Attempt >= 3)
          {
            await ReferralService.ExpireTextMessageDueToDobCheckAsync(
              HttpContext.Session.GetString(SESSION_KEY_TEXT_MESSAGE_TOKEN));
            HttpContext.Session.Remove(SESSION_KEY_TEXT_MESSAGE_TOKEN);
            redirect = VerificationFailure();
          }
        }
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "verifying date of birth");
      }

      return redirect;
    }

    [Route("{controller}/email-confirmation/{id}")]
    public async Task<IActionResult> EmailConfirmation(Guid id)
    {
      ContactModel model;
      try
      {
        IReferral referral = await GetReferralWithValidation(id);

        string serviceUserLinkId = HttpContext.Session.GetString(SESSION_KEY_TEXT_MESSAGE_TOKEN);
        TextMessage textMessage = referral.TextMessages
          .Where(t => t.IsActive)
          .Where(t => t.ServiceUserLinkId == serviceUserLinkId)
          .OrderBy(t => t.Sent)
          .LastOrDefault();

        if (textMessage != null)
        {
          await ReferralService.UpdateDobAttemptsAsync(textMessage.Id, 0);
        }

        model = _mapper.Map<IReferral, ContactModel>(referral);

        if (model != null && model.DontContactByEmail)
        {
          model.Email = null;
        }
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "getting contact details");
      }

      return View("EmailConfirmation", model);
    }

    [HttpPost]
    [Route("{controller}/email-confirmation/{id}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
      NoStore = true)]
    public async Task<IActionResult> EmailConfirmation(ContactModel model)
    {
      if (!ModelState.IsValid)
      {
        return View("EmailConfirmation", model);
      }

      try
      {
        if (model.DontContactByEmail)
        {
          return NavigateToAction("EmailFinalWarning", model.Id);
        }

        IReferral updatedReferral = await ReferralService
          .UpdateConsentForFutureContactForEvaluation(
            model.Id,
            model.DontContactByEmail,
            consented: false,
            model.Email);

        return NavigateToAction("select-ethnicity-group", model.Id);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "confirming email");
      }
    }

    [Route("{controller}/select-ethnicity-group/{id}")]
    public async Task<IActionResult> SelectEthnicityGroup(Guid id)
    {
      EthnicityModel model;

      try
      {
        IReferral referral = await GetReferralWithValidation(id);

        model = _mapper.Map<IReferral, EthnicityModel>(referral);

        model.EthnicityGroupList = await GetEthnicityGroupList();
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "selecting ethnicity group");
      }

      return View("EthnicityGroup", model);
    }

    [HttpPost]
    [Route("{controller}/select-ethnicity-group/{id}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
      NoStore = true)]
    public async Task<IActionResult> SelectEthnicityGroup(EthnicityModel model)
    {
      ModelState.Remove("ServiceUserEthnicity");

      if (!ModelState.IsValid)
      {
        model.EthnicityGroupList = await GetEthnicityGroupList();
        return View("EthnicityGroup", model);
      }

      try
      {
        IReferral updatedReferral = await ReferralService
          .UpdateServiceUserEthnicityGroupAsync(
            model.Id, 
            model.ServiceUserEthnicityGroup);

        if (model.ServiceUserEthnicityGroup == EnumDescriptionHelper
          .GetDescriptionFromEnum(EthnicityGroup.DoNotWishToDisclose))
        {
          updatedReferral = await ReferralService
            .UpdateServiceUserEthnicityAsync(
              model.Id,
              model.ServiceUserEthnicityGroup);

          return NavigateToAction("contact-preference", model.Id);
        }

        return NavigateToAction("select-ethnicity", model.Id);
      }
      catch (BmiTooLowException)
      {
        return RedirectToAction("BmiWarning", "ServiceUser", new { model.Id });
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "selecting ethnicity group");
      }
    }

    [Route("{controller}/select-ethnicity/{id}")]
    public async Task<IActionResult> SelectEthnicity(Guid id)
    {
      EthnicityModel model;

      try
      {
        IReferral referral = await GetReferralWithValidation(id);

        model = _mapper.Map<IReferral, EthnicityModel>(referral);

        if (!model.ServiceUserEthnicityGroup.ToLower().Contains("other"))
          model.EthnicityGroupDescription = model.ServiceUserEthnicityGroup;

        if (model.ServiceUserEthnicityGroup ==
           "I do not wish to Disclose my Ethnicity")
        {
          model.EthnicityGroupList = await GetEthnicityGroupList();
          return View("EthnicityGroup", model);
        }

        model.EthnicityList =
          await GetEthnicityList(model.ServiceUserEthnicityGroup);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "getting ethnicity details");
      }

      return View("Ethnicity", model);
    }

    [HttpPost]
    [Route("{controller}/select-ethnicity/{id}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
      NoStore = true)]
    public async Task<IActionResult> SelectEthnicity(EthnicityModel model)
    {
      ModelState.Remove("ServiceUserEthnicityGroup");

      if (!ModelState.IsValid)
      {
        model.EthnicityList = await GetEthnicityList(
          model.ServiceUserEthnicityGroup);
        return View("Ethnicity", model);
      }

      try
      {
        IReferral updatedReferral = await ReferralService
          .UpdateServiceUserEthnicityAsync(
            model.Id,
            model.ServiceUserEthnicity);
        return NavigateToAction("contact-preference", model.Id);
      }
      catch (BmiTooLowException)
      {
        return RedirectToAction("BmiWarning", "ServiceUser", new { model.Id });
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "selecting ethnicity");
      }
    }

    [Route("{controller}/bmi-warning/{id}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
      NoStore = true)]
    public async Task<IActionResult> BmiWarning(Guid id)
    {
      BmiWarningModel model;
      try
      {
        IReferral referral = await GetReferralWithValidation(id, true);

        model = _mapper.Map<IReferral, BmiWarningModel>(referral);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "with bmi warning");
      }
      return View("BmiWarning", model);
    }

    [Route("{controller}/bmi-too-low/{id}")]
    public async Task<IActionResult> BmiTooLow(Guid id)
    {
      try
      {
        IReferral updatedReferral = await ReferralService
          .SetBmiTooLowAsync(id);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "with bmi too low");
      }

      return View("BmiTooLow");
    }

    public IActionResult VerificationFailure()
    {
      return View("VerificationFailure");
    }

    [Route("{controller}/email-final-warning/{id}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
      NoStore = true)]
    public async Task<IActionResult> EmailFinalWarning(Guid id)
    {
      ContactModel model;
      try
      {
        IReferral referral = await GetReferralWithValidation(id);

        model = _mapper.Map<IReferral, ContactModel>(referral);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "with email final warning");
      }
      return View("EmailFinalWarning", model);
    }

    [Route("{controller}/email-not-provided/{id}")]
    public async Task<IActionResult> EmailNotProvided(Guid id)
    {
      ContactModel model;
      try
      {
        IReferral referral = await GetReferralWithValidation(id);

        model = _mapper.Map<IReferral, ContactModel>(referral);

        if (model != null)
        {
          model.DontContactByEmail = true;
          model.Email = DONT_CONTACT_BY_EMAIL;
        }

        IReferral updatedReferral = await ReferralService
          .EmailAddressNotProvidedAsync(model.Id);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "with email not provided");
      }

      return NavigateToAction("process-complete", model.Id);
    }

    [Route("{controller}/contact-preference/{id}")]
    public async Task<IActionResult> ContactPreference(Guid id)
    {
      ContactModel model;
      try
      {
        IReferral referral = await GetReferralWithValidation(id);

        model = _mapper.Map<IReferral, ContactModel>(referral);
        model.Source = string.IsNullOrWhiteSpace(referral.ReferralSource)
          ? ReferralSource.GpReferral
          : Enum.Parse<ReferralSource>(referral.ReferralSource);

        if (model?.DontContactByEmail == true)
        {
          if (model != null)
          {
            model.Email = null;
          }
          return View("EmailConfirmation", model);
        }
        else
        {
          return View("Contact", model);
        }
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "getting contact details");
      }
    }

    [HttpPost]
    [Route("{controller}/contact-preference/{id}")]
    public async Task<IActionResult> ContactPreference(ContactModel model)
    {
      if (!ModelState.IsValid)
      {
        return View("Contact", model);
      }

      try
      {
        IReferral updatedReferral = await ReferralService
          .UpdateConsentForFutureContactForEvaluation(
            model.Id,
            true,
            model.CanContact,
            model.Email);

        return NavigateToAction("choose-provider", model.Id);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "getting contact preference");
      }
    }

    [Route("{controller}/provider-summary/{id}")]
    public async Task<IActionResult> ProviderSummary(Guid id, Guid providerId)
    {
      if (providerId == Guid.Empty)
      {
        return View("GoneWrong", GetErrorModel("viewing provider summary"));
      }

      try
      {
        TriageLevel triagedCompletionLevel = await _referralService
          .GetTriagedCompletionLevelAsync(id);

        ProviderResponse providerResponse = await _providerService.GetProviderAsync(providerId);

        ProviderSummaryModel model = new()
        {
          Id = id,
          Logo = providerResponse.Logo,
          ProviderId = providerResponse.Id,
          ProviderName = providerResponse.Name,
          TriagedCompletionLevel = triagedCompletionLevel
        };

        return View(model);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "viewing provider summary");
      }
    }

    [Route("{controller}/choose-provider/{id}")]
    public async Task<IActionResult> SelectProvider(Guid id)
    {
      try
      {
        ProviderChoiceModel model = await GetProviderChoiceModelPopulatedWithProviders(id);

        if (Guid.TryParse(
          HttpContext.Session.GetString(SESSION_KEY_PROVIDER_ID), 
          out Guid providerId))
        {
          model.ProviderId = providerId;
        }

        return View("WmsSelection", model);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "getting provider details");
      }
    }

    [HttpPost]
    [Route("{controller}/choose-provider/{id}")]
    public async Task<IActionResult> WmsSelection(ProviderChoiceModel model)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          ProviderChoiceModel invalidModel = 
            await GetProviderChoiceModelPopulatedWithProviders(model.Id);
          invalidModel.DisplayError = true;

          return View("WmsSelection", invalidModel);
        }

        IReferral referral = await GetReferralWithValidation(model.Id);

        ProviderResponse providerResponse = 
          await _providerService.GetProviderAsync(model.ProviderId);
        model.Provider = model.Providers.Where(p => p.Id == model.ProviderId).FirstOrDefault();
        model.Provider.Logo = providerResponse.Logo;

        HttpContext.Session.SetString(SESSION_KEY_PROVIDER_ID, model.ProviderId.ToString());

        return View("WmsConfirmation", model);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "choosing provider");
      }
    }

    [HttpPost]
    [Route("{controller}/confirm-provider/{id}")]
    public async Task<IActionResult> ConfirmProvider(ProviderChoiceModel model)
    {
      if (!ModelState.IsValid)
      {
        return View("WmsConfirmation", model);
      }

      try
      {
        IReferral referral = await ReferralService
          .ConfirmProviderAsync(model.Id, model.Provider.Id);

        // remove cookies
        HttpContext.Session.Remove(SESSION_KEY_PROVIDER_ID);
        HttpContext.Session.Remove(SESSION_KEY_TEXT_MESSAGE_TOKEN);

        return NavigateToAction("Process-Complete", model.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);

        if (ex is ReferralProviderSelectedException)
        {
          return View("WmsCompletion", model);
        }
        else
        {
          return View("GoneWrong", GetErrorModel("confirming provider"));
        }
      }
    }

    [Route("{controller}/process-complete/{id}")]
    public async Task<IActionResult> WmsCompletion(Guid id)
    {
      ServiceUserModel model;
      try
      {
        IReferral referral = await ReferralService
          .GetReferralWithTriagedProvidersById(id);

        model = _mapper.Map<IReferral, ServiceUserModel>(referral);
      }
      catch (Exception ex)
      {
        return ErrorHandler(ex, "completing process");
      }
      return View("WmsCompletion", model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
      NoStore = true)]
    public IActionResult Error(string message)
    {
      return View(new ErrorViewModel
      {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        Message = message
      });
    }

    [Route("{controller}/session-ping")]
    public IActionResult SessionPing()
    {
      return Ok();
    }

    [Route("{controller}/session-ended")]
    public IActionResult EndSession()
    {
      // remove cookies
      HttpContext.Session.Remove(SESSION_KEY_PROVIDER_ID);
      HttpContext.Session.Remove(SESSION_KEY_TEXT_MESSAGE_TOKEN);

      //return
      return View();
    }

    public IActionResult Help()
    {
      _logger.LogInformation("Help Get View");

      return View();
    }

    public IActionResult GoneWrong()
    {
      var model = new ErrorViewModel()
      {
        RequestId = HttpContext.Session.GetString(SESSION_KEY_UBRN),
        Message = $"Unknown error."
      };
      _logger.LogInformation("Gone Wrong View");

      return View(model);
    }

    private ViewResult ErrorHandler(Exception ex, string goneWrongMessage)
    {

      ExpiredLinkModel expiredLinkModel = new ExpiredLinkModel();

      // exception thrown if the user incorrectly enters their
      // date of birth 3 times
      if (ex is TextMessageExpiredByDoBCheckException)
      {
        expiredLinkModel.ExpirationReason = "This link no longer " +
         "works because the date of birth you entered did not match our " +
         "records.";
        _logger.LogInformation(ex.Message);
        return View("Expired", expiredLinkModel);
      }

      // exception thrown if user previous declined to provide their email 
      // address
      if (ex is TextMessageExpiredByEmailException)
      {
        expiredLinkModel.ExpirationReason = "This link no longer " +
         "works because your referral has been rejected back to your GP " +
         "following your decision to not provide an e-mail address.";
        _logger.LogInformation(ex.Message);
        return View("Expired", expiredLinkModel);
      }

      // exception thrown if the referral has a provider selected date
      if (ex is TextMessageExpiredByProviderSelectionException ||
        ex is ReferralProviderSelectedException)
      {
        expiredLinkModel.ExpirationReason = "This link no longer " +
         "works because you previously selected a service provider.";

        _logger.LogInformation(ex.Message);
        return View("Expired", expiredLinkModel);
      }

      // exception thrown if the referral has an invalid status
      if (ex is ReferralInvalidStatusException)
      {
        expiredLinkModel.ExpirationReason = "This link has expired due to " +
          "the current status of your referral.";

        _logger.LogInformation(ex.Message);
        return View("Expired", expiredLinkModel);
      }

      // exception thrown if the bmi is too low
      if (ex is BmiTooLowException)
      {
        _logger.LogInformation(ex.Message);
        return View("BmiTooLow");
      }

      _logger.LogError(ex, goneWrongMessage);
      return View("GoneWrong", GetErrorModel(goneWrongMessage));
    }

    protected async Task<IReferral> GetReferralWithValidation(
      Guid id,
      bool showException = false)
    {
      IReferral referral = await ReferralService.GetReferralWithTriagedProvidersById(id);

      if (referral.IsProviderSelected)
      {
        throw new ReferralProviderSelectedException(referral.Id, referral.ProviderId);
      }

      if (referral.IsExceptionDueToEmailNotProvided)
      {
        throw new TextMessageExpiredByEmailException();
      }

      string[] validStatuses = new string[] 
      {
        ReferralStatus.TextMessage1.ToString(),
        ReferralStatus.TextMessage2.ToString(),
        ReferralStatus.ChatBotCall1.ToString(),
        ReferralStatus.ChatBotTransfer.ToString(),
        ReferralStatus.RmcCall.ToString(),
        ReferralStatus.RmcDelayed.ToString(),
        ReferralStatus.TextMessage3.ToString()
      };

      if (!validStatuses.Contains(referral.Status) && !showException)
      {
        throw new ReferralInvalidStatusException($"Invalid Status of {referral.Status}");
      }

      return referral;
    }

    private ErrorViewModel GetErrorModel(string message)
    {
      return new ErrorViewModel()
      {
        RequestId = HttpContext.Session.GetString(SESSION_KEY_UBRN),
        Message = $"Error {message}."
      };
    }

    private async Task<List<SelectListItem>> GetEthnicityList(string group)
    {
      List<SelectListItem> ethnicityList = new List<SelectListItem>();

      // use a service to get a list of ethnicity codes
      try
      {
        IEnumerable<Business.Models.Ethnicity> businessModel =
          await _ethnicityService.GetEthnicityGroupMembersAsync(group);

        foreach (Business.Models.Ethnicity ethnicity in businessModel)
        {
          ethnicityList.Add(new SelectListItem()
          {
            Value = ethnicity.DisplayName,
            Text = ethnicity.DisplayName
          });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
      }

      return ethnicityList;
    }

    private async Task<List<SelectListItem>> GetEthnicityGroupList()
    {
      List<SelectListItem> ethnicityGroupList = new List<SelectListItem>();

      // use a service to get a list of ethnicity codes
      try
      {
        IEnumerable<string> businessModel =
          await _ethnicityService.GetEthnicityGroupNamesAsync();

        foreach (string groupName in businessModel)
        {
          ethnicityGroupList.Add(new SelectListItem()
          {
            Value = groupName,
            Text = groupName
          });
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
      }

      return ethnicityGroupList;
    }

    private async Task<ProviderChoiceModel> GetProviderChoiceModelPopulatedWithProviders(
      Guid referralId)
    {
      IReferral referral = await GetReferralWithValidation(referralId);

      // The first time the service user is shown a list of the providers it should be randomised
      List<string> providerNamesOrder = await ReferralService
        .GetProviderNamesOrderAsync(referralId);

      if (providerNamesOrder == null)
      {
        referral.Providers = referral.Providers.Randomize().ToList();

        await ReferralService.UpdateAnalyticsForProviderList(
          referral.Id,
          string.Join(",", referral.Providers.Select(t => t.Name)));
      }
      else
      {
        List<Business.Models.Provider> previouslyOrderedProvidersList = new();
        foreach (string providerName in providerNamesOrder)
        {
          Business.Models.Provider provider = referral
            .Providers
            .FirstOrDefault(x => x.Name == providerName);

          if (provider != null)
          {
            previouslyOrderedProvidersList.Add(provider);
          }
        }

        if (previouslyOrderedProvidersList.Count == referral.Providers.Count)
        {
          referral.Providers = previouslyOrderedProvidersList;
        }
      }

      ProviderChoiceModel model = _mapper.Map<IReferral, ProviderChoiceModel>(referral);

      return model;
    }

    private IActionResult NavigateToAction(string actionName, Guid id)
    {
      return new RedirectToActionResult(actionName, "ServiceUser", new { id });
    }
  }
}
