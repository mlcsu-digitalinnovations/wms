﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WmsHub.Provider.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class ServiceUserResponse
  {
    public DateTimeOffset DateOfReferral { get; set; }
    public string ReferringGpPracticeNumber { get; set; }
    public string Ubrn => ProviderUbrn;
    [JsonIgnore]
    public string ProviderUbrn { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }    
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Postcode { get; set; }
    public string Telephone { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public string SexAtBirth { get; set; }
    public bool? IsVulnerable { get; set; }
    public string Ethnicity { get; set; }
    public bool? HasPhysicalDisability { get; set; }
    public bool? HasLearningDisability { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    public bool? HasHypertension { get; set; }
    public bool? HasDiabetesType1 { get; set; }
    public bool? HasDiabetesType2 { get; set; }
    public decimal Height { get; set; }
    public decimal Bmi { get; set; }
    public DateTimeOffset BmiDate { get; set; }
    public DateTimeOffset ProviderSelectedDate { get; set; }
    public int TriagedLevel { get; set; }
    public string ReferralSource { get; set; }
  }
}
