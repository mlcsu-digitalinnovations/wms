using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace WmsHub.Ui.Models
{
	public class EthnicityModel : BaseModel
	{
		public string SelectedEthnicity { get; set; }
		[Required(ErrorMessage = 
      "Select the option that best describes your background")]
		public string ServiceUserEthnicity { get; set; }
		[Required(ErrorMessage = "Select an ethnic group or 'Prefer not to say'")]
		public string ServiceUserEthnicityGroup { get; set; }
		public List<SelectListItem> EthnicityList { get; set; }
		public List<SelectListItem> EthnicityGroupList { get; set; }
		public string EthnicityGroupDescription { get; set; }
	}
}
