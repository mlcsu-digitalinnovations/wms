using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Entities.Interfaces;

namespace WmsHub.Business.Entities;
public class MskOrganisationBase : BaseEntity, IMskOrganisation
{
  public string OdsCode { get; set; }
  public bool SendDischargeLetters { get; set; }
  public string SiteName { get; set; }
}
