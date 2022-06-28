using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WmsHub.Ui.Models
{
	public class VerificationModel : BaseModel
	{
		[Required(ErrorMessage = "Date of birth must include a day")]
		[Range(1, 31, ErrorMessage = "Day must be between 1 and 31")]
		public int? Day { get; set; }
		[Required(ErrorMessage = "Date of birth must include a month")]
		[Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
		public int? Month { get; set; }
		[Required(ErrorMessage = "Date of birth must include a year")]
 		public int? Year { get; set; }
		[HiddenInput]
		public int Attempt { get; set; }
	}
}