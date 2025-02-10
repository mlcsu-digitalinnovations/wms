using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Ui.Models
{
	public class ServiceUserModel
	{
		public Guid Id { get; set; }
		public string NhsNumber { get; set; }
		public string ReferringGpPracticeNumber { get; set; }
		public string Ubrn { get; set; }
		public string FamilyName { get; set; }
		public string GivenName { get; set; }

		public string EmailAddress { get; set; }
		public DateTimeOffset? DateOfBirth { get; set; }
		public DateTimeOffset? DateOfReferral { get; set; }
		public bool ConsentForFutureContact { get; set; }
		public string Ethnicity { get; set; }
		public Provider SelectedProvider { get; set; }
		public ReferralSource Source { get; set; }
    public List<Provider> Providers { get; set; }
    public bool HasEmailAddress => !string.IsNullOrWhiteSpace(EmailAddress);
    public bool HasGpReferralSource => Source == ReferralSource.GpReferral;
  }
}
