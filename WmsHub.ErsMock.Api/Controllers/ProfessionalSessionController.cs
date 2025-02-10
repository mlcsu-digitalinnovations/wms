using Microsoft.AspNetCore.Mvc;
using WmsHub.Common.Models;
using WmsHub.ErsMock.Api.Models.ProfessionalSession;

namespace WmsHub.ErsMock.Api.Controllers;

[ApiController]
[Route("ers-api/v1/[controller]")]
public class ProfessionalSessionController : ControllerBase
{
  ErsSession.PermissionModel _permission = new()
  {
    BusinessFunction = "SERVICE_PROVIDER_CLINICIAN",
    OrgIdentifier = "0CX",
    OrgName = "MLCSU"
  };

  ErsSession _ersSession = new()
  {
    Id = "MockErsSession",
    Permission = null,
    TypeInfo = "uk.nhs.ers.xapi.dto.v1.session.ProfessionalSession",
    User = new()
    {
      FirstName = "Mock",
      Identifier = "MockUser",
      LastName = "User",
      MiddleName = ""
    }
  };

  [HttpPost]
  public IActionResult A001CreateProfessionalSession(CreateRequest request)
  {
    _ersSession.User.Permissions = new() { _permission };
    return Ok(_ersSession);
  }

  [HttpPut("{activeSessionId}")]
  public IActionResult A002ProfessionalSessionSelectRole(
    [FromRoute] string activeSessionId,
    [FromBody] SelectRolePathRequest request)
  {
    if (activeSessionId == _ersSession.Id)
    {
      _ersSession.Permission = _permission;
      _ersSession.User.Permissions = new() { _permission };
      return Ok(_ersSession);
    }
    else
    {
      return Unauthorized();
    }
  }

  [HttpDelete("{sessionKey}")]
  public IActionResult A003DeleteProfessionalSession(string sessionKey)
  {
    return NoContent();
  }

}
