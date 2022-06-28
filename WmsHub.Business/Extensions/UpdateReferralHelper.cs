using WmsHub.Business.Entities;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Extensions
{
  public static class UpdateReferralHelper
  {
    public static int GeneralReferralUpdate(
      this IGeneralReferralUpdate update, Referral entity)
    {
      int fieldsUpdated = 0;
      if (entity.FamilyName != update.FamilyName)
      {
        fieldsUpdated++;
        entity.FamilyName = update.FamilyName;
      }

      if (entity.GivenName != update.GivenName)
      {
        fieldsUpdated++;
        entity.GivenName = update.GivenName;
      }

      if (entity.Address1 != update.Address1)
      {
        fieldsUpdated++;
        entity.Address1 = update.Address1;
      }

      if (entity.Address2 != update.Address2)
      {
        fieldsUpdated++;
        entity.Address2 = update.Address2;
      }

      if (entity.Address3 != update.Address3)
      {
        fieldsUpdated++;
        entity.Address3 = update.Address3;
      }

      if (entity.Postcode != update.Postcode)
      {
        fieldsUpdated++;
        entity.Postcode = update.Postcode;
      }

      if (entity.Email != update.Email)
      {
        fieldsUpdated++;
        entity.Email = update.Email;
      }

      if (entity.Telephone != update.Telephone)
      {
        fieldsUpdated++;
        entity.Telephone = update.Telephone;
      }

      if (entity.Mobile != update.Mobile)
      {
        fieldsUpdated++;
        entity.Mobile = update.Mobile;
      }

      if (entity.Sex != update.Sex)
      {
        fieldsUpdated++;
        entity.Sex = update.Sex;
      }

      if (entity.Ethnicity != update.Ethnicity)
      {
        fieldsUpdated++;
        entity.Ethnicity = update.Ethnicity;
      }

      if (entity.ServiceUserEthnicity != update.ServiceUserEthnicity)
      {
        fieldsUpdated++;
        entity.ServiceUserEthnicity = update.ServiceUserEthnicity;
      }

      if (entity.ServiceUserEthnicityGroup != update.ServiceUserEthnicityGroup)
      {
        fieldsUpdated++;
        entity.ServiceUserEthnicityGroup = update.ServiceUserEthnicityGroup;
      }

      if (entity.IsPregnant != update.IsPregnant)
      {
        fieldsUpdated++;
        entity.IsPregnant = update.IsPregnant;
      }

      if (entity.HasALearningDisability != update.HasALearningDisability)
      {
        fieldsUpdated++;
        entity.HasALearningDisability = update.HasALearningDisability;
      }

      if (entity.HasAPhysicalDisability != update.HasAPhysicalDisability)
      {
        fieldsUpdated++;
        entity.HasAPhysicalDisability = update.HasAPhysicalDisability;
      }

      if (entity.HasActiveEatingDisorder != update.HasActiveEatingDisorder)
      {
        fieldsUpdated++;
        entity.HasActiveEatingDisorder = update.HasActiveEatingDisorder;
      }

      if (entity.HasALearningDisability != update.HasALearningDisability)
      {
        fieldsUpdated++;
        entity.HasALearningDisability = update.HasALearningDisability;
      }
      if (entity.HasArthritisOfHip != update.HasArthritisOfHip)
      {
        fieldsUpdated++;
        entity.HasArthritisOfHip = update.HasArthritisOfHip;
      }

      if (entity.HasArthritisOfKnee != update.HasArthritisOfKnee)
      {
        fieldsUpdated++;
        entity.HasArthritisOfKnee = update.HasArthritisOfKnee;
      }

      if (entity.HasDiabetesType1 != update.HasDiabetesType1)
      {
        fieldsUpdated++;
        entity.HasDiabetesType1 = update.HasDiabetesType1;
      }

      if (entity.HasDiabetesType2 != update.HasDiabetesType2)
      {
        fieldsUpdated++;
        entity.HasDiabetesType2 = update.HasDiabetesType2;
      }

      if (entity.HasHypertension != update.HasHypertension)
      {
        fieldsUpdated++;
        entity.HasHypertension = update.HasHypertension;
      }

      if (entity.HasHadBariatricSurgery != update.HasHadBariatricSurgery)
      {
        fieldsUpdated++;
        entity.HasHadBariatricSurgery = update.HasHadBariatricSurgery;
      }

      if (entity.HasRegisteredSeriousMentalIllness != 
          update.HasRegisteredSeriousMentalIllness)
      {
        fieldsUpdated++;
        entity.HasRegisteredSeriousMentalIllness = 
          update.HasRegisteredSeriousMentalIllness;
      }

      if (entity.ReferringGpPracticeName != update.ReferringGpPracticeName)
      {
        fieldsUpdated++;
        entity.ReferringGpPracticeName = update.ReferringGpPracticeName;
      }

      if (entity.ReferringGpPracticeNumber != update.ReferringGpPracticeNumber)
      {
        fieldsUpdated++;
        entity.ReferringGpPracticeNumber = update.ReferringGpPracticeNumber;
      }

      if (entity.DateOfBirth != update.DateOfBirth)
      {
        fieldsUpdated++;
        entity.DateOfBirth = update.DateOfBirth;
      }

      if (entity.DateOfBmiAtRegistration != update.DateOfBmiAtRegistration)
      {
        fieldsUpdated++;
        entity.DateOfBmiAtRegistration = update.DateOfBmiAtRegistration;
      }

      if (entity.HeightCm != update.HeightCm)
      {
        fieldsUpdated++;
        entity.HeightCm = update.HeightCm;
      }

      if (entity.WeightKg != update.WeightKg)
      {
        fieldsUpdated++;
        entity.WeightKg = update.WeightKg;
      }

      if (entity.ConsentForFutureContactForEvaluation != 
          update.ConsentForFutureContactForEvaluation)
      {
        fieldsUpdated++;
        entity.ConsentForFutureContactForEvaluation = 
          update.ConsentForFutureContactForEvaluation;
      }

      if (entity.ConsentForGpAndNhsNumberLookup != 
          update.ConsentForGpAndNhsNumberLookup)
      {
        fieldsUpdated++;
        entity.ConsentForGpAndNhsNumberLookup = 
          update.ConsentForGpAndNhsNumberLookup;
      }

      if (entity.ConsentForReferrerUpdatedWithOutcome != 
          update.ConsentForReferrerUpdatedWithOutcome)
      {
        fieldsUpdated++;
        entity.ConsentForReferrerUpdatedWithOutcome = 
          update.ConsentForReferrerUpdatedWithOutcome;
      }



      return fieldsUpdated;
    }
  }
}