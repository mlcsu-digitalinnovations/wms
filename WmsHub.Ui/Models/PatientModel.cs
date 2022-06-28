using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Ui.Models
{
	public class PatientModel
	{
		public string NhsNumber { get; set; }
		public string ReferringGpPracticeNumber { get; set; }
		public string Ubrn { get; set; }
		public string FamilyName { get; set; }
		public string GivenName { get; set; }
		public string Postcode { get; set; }
		public string TelephoneNumber { get; set; }
		public string EmailAddress { get; set; }
		public DateTime DateOfBirth { get; set; }
		public int Sex { get; set; }
		public int Gender { get; set; }
		public DateTime DateOfReferral { get; set; }
		public bool ConsentForFutureContact { get; set; }
		public string Ethnicity { get; set; }

	}
}
