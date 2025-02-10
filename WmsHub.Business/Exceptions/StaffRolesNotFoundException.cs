using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class StaffRolesNotFoundException : Exception
{
  public StaffRolesNotFoundException() : base() { }
  public StaffRolesNotFoundException(string message) : base(message) { }
  public StaffRolesNotFoundException(Guid staffRoleId)
    : base($"Unable to find a Staff Role with an id of {staffRoleId}.") { }
  public StaffRolesNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}