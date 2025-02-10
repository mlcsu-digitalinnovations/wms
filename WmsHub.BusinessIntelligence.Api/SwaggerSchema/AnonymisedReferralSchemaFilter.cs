using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;
using WmsHub.Common;

namespace WmsHub.BusinessIntelligence.Api.SwaggerSchema;

public class AnonymisedReferralSchemaFilter: ISchemaFilter
{
  public void Apply(OpenApiSchema schema, SchemaFilterContext context)
  {
    foreach (System.Reflection.PropertyInfo propInfo in 
      context.Type.GetProperties())
    {
      if (propInfo.Name.ToUpper() == "STATUS")
      {
        OpenApiSchema status = schema.Properties["status"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[{string.Join("<br />", Enum.GetNames<ReferralStatus>())}]",
          Example = new OpenApiString(ReferralStatus.Exception.ToString())
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("status", obj);

        schema.Properties.Remove("status");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "DEPRIVATION")
      {
        OpenApiSchema status = schema.Properties["deprivation"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[{string.Join(", ", Enum.GetNames<Deprivation>())}]",
          Example = new OpenApiString(Deprivation.IMD1.ToString())
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("deprivation", obj);

        schema.Properties.Remove("deprivation");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "METHODOFCONTACT")
      {
        OpenApiSchema status = schema.Properties["methodOfContact"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[{string.Join(", ", Enum.GetNames<MethodOfContact>())}]",
          Example = new OpenApiString(MethodOfContact.NoContact.ToString())
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("methodOfContact", obj);

        schema.Properties.Remove("methodOfContact");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "ETHNICITY")
      {
        OpenApiSchema status = schema.Properties["ethnicity"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[{string.Join(", ", Enum.GetNames<Ethnicity>())}]",
          Example = new OpenApiString(Ethnicity.White.ToString())
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("ethnicity", obj);

        schema.Properties.Remove("ethnicity");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "SERVICEUSERETHNICITY")
      {
        OpenApiSchema status = schema.Properties["serviceUserEthnicity"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[" +
            "African<br />" +
            "Any other Asian background<br />" +
            "Any other Black, African or Caribbean background<br />" +
            "Any other ethnic group<br />" +
            "Any other Mixed or Multiple ethnic background<br />" +
            "Any other White background<br />" +
            "Arab<br />" +
            "Bangladeshi<br />" +
            "Caribbean<br />" +
            "Chinese<br />" +
            "English, Welsh, Scottish, Northern Irish or British<br />" +
            "Gypsy or Irish Traveller<br />" +
            "I do not wish to Disclose my Ethnicity<br />" +
            "Indian<br />" +
            "Irish<br />" +
            "Pakistani<br />" +
            "White and Asian<br />" +
            "White and Black African<br />" +
            "White and Black Caribbean" +
            $"]",
          Example = new OpenApiString("Asian or Asian British")
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("serviceUserEthnicity", obj);

        schema.Properties.Remove("serviceUserEthnicity");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "SERVICEUSERETHNICITYGROUP")
      {
        OpenApiSchema status = schema.Properties["serviceUserEthnicityGroup"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[" +
            "Asian or Asian British<br />" +
            "Black, African, Caribbean or Black British<br />" +
            "I do not wish to Disclose my Ethnicity<br />" +
            "Mixed or Multiple ethnic groups<br />" +
            "Other ethnic group<br />" +
            "White" +
            $"]",
          Example = new OpenApiString("Asian or Asian British")
        };

        KeyValuePair<string, OpenApiSchema> test = 
          new("serviceUserEthnicityGroup", obj);

        schema.Properties.Remove("serviceUserEthnicityGroup");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "SEX")
      {
        OpenApiSchema status = schema.Properties["sex"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[{string.Join(", ", Enum.GetNames<Sex>())}]",
          Example = new OpenApiString(Sex.Female.ToString())
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("sex", obj);

        schema.Properties.Remove("sex");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "PROGRAMMEOUTCOME")
      {
        OpenApiSchema status = schema.Properties["programmeOutcome"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[{string.Join("<br />", Enum.GetNames<ProgrammeOutcome>())}]",
          Example = new OpenApiString(ProgrammeOutcome.Complete.ToString())
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("programmeOutcome", obj);

        schema.Properties.Remove("programmeOutcome");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "REFERRALSOURCE")
      {
        OpenApiSchema status = schema.Properties["referralSource"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            $"[{string.Join("<br />", Enum.GetNames<ReferralSource>())}]",
          Example = new OpenApiString(ReferralSource.GeneralReferral.ToString())
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("referralSource", obj);

        schema.Properties.Remove("referralSource");
        schema.Properties.Add(test);
      }

      if (propInfo.Name.ToUpper() == "STAFFROLE")
      {
        OpenApiSchema status = schema.Properties["staffRole"];
        OpenApiSchema obj = new OpenApiSchema
        {
          Title = status.Title,
          Type = status.Type,
          Nullable = false,
          Description =
            $"One of the following options: <br />" +
            "[" +
            "Administrative and clerical, Allied Health Professional e.g ." +
            "physiotherapist,<br />Ambulance staff, Doctor, Estates " +
            "and porters,  Healthcare Assistant/Support worker,<br /> " +
            "Healthcare scientists,  Managerial,  Nursing and midwifery," +
            " Other]",
          Example = new OpenApiString("Administrative and clerical")
        };

        KeyValuePair<string, OpenApiSchema> test =
          new KeyValuePair<string, OpenApiSchema>("staffRole", obj);

        schema.Properties.Remove("staffRole");
        schema.Properties.Add(test);
      }
    }

  }
}
