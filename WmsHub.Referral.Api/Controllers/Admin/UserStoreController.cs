using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class UserStoreController : BaseController
  {
    public UserStoreController(IUsersStoreService service)
      : base(service)
    {
    }

    /// <summary>
    /// Loads the users store from a converted CSV file extract
    /// </summary>
    /// <returns>A status message.</returns>
    /// <response code="200">Users uploaded</response>
    /// <response code="400">Invalid json payload</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Service unavailable, please try again</response>
    [HttpPost("LoadFromAdCsv")]
    public async Task<IActionResult> LoadFromAdCsv(
      List<UserStore> userStoreUsers)
    {
      try
      {
        return Ok(await Service.LoadAsync(userStoreUsers));
      }
      catch (Exception ex)
      {
        if (ex is ArgumentException)
        {
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status403Forbidden);
        }
        else
        {
          LogException(ex);
          return Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    protected IUsersStoreService Service
    {
      get
      {
        var service = _service as IUsersStoreService;
        service.User = User;
        return service;
      }
    }
  }
}
