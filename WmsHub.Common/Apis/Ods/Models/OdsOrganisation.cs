using System.Collections.Generic;

namespace WmsHub.Common.Apis.Ods.Models
{

  public class OdsOrganisation
  {
    public OdsOrganisation()
    { }

    public OdsOrganisation(string odsCode) : this(odsCode, null)
    { }

    public OdsOrganisation(string odsCode, string name)
    {
      Organisation = new()
      {
        Name = name,
        OrgId = new()
        {
          Extension = odsCode
        }
      };
    }


    public Organisation Organisation { get; set; }

    public bool WasFound => Organisation != null;
  }

  public class Date
  {
    public string Type { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
  }

  public class GeoLoc
  {
    public Location Location { get; set; }
  }

  public class Location
  {
    public string AddrLn1 { get; set; }
    public string AddrLn2 { get; set; }
    public string AddrLn3 { get; set; }
    public string Town { get; set; }
    public string County { get; set; }
    public string PostCode { get; set; }
    public string Country { get; set; }
  }

  public class Organisation
  {
    public string Name { get; set; }
    public List<Date> Date { get; set; }
    public OrgId OrgId { get; set; }
    public string Status { get; set; }
    public string LastChangeDate { get; set; }
    public string OrgRecordClass { get; set; }
    public GeoLoc GeoLoc { get; set; }
    public Roles Roles { get; set; }
    public Rels Rels { get; set; }
  }

  public class OrgId
  {
    public string Root { get; set; }
    public string AssigningAuthorityName { get; set; }
    public string Extension { get; set; }
  }

  public class PrimaryRoleId
  {
    public string Id { get; set; }
    public int UniqueRoleId { get; set; }
  }

  public class Rel
  {
    public List<Date> Date { get; set; }
    public string Status { get; set; }
    public Target Target { get; set; }
    public string Id { get; set; }
    public int UniqueRelId { get; set; }
  }

  public class Rels
  {
    public List<Rel> Rel { get; set; }
  }

  public class Role
  {
    public string Id { get; set; }
    public int UniqueRoleId { get; set; }
    public bool PrimaryRole { get; set; }
    public List<Date> Date { get; set; }
    public string Status { get; set; }
  }

  public class Roles
  {
    public List<Role> Role { get; set; }
  }

  public class Target
  {
    public OrgId OrgId { get; set; }
    public PrimaryRoleId PrimaryRoleId { get; set; }
  }


}
