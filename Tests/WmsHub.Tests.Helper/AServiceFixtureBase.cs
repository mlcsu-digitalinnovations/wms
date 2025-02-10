using System;
using WmsHub.Business;
using WmsHub.Business.Helpers;

namespace WmsHub.Tests.Helper;

public class AServiceFixtureBase
{
  public static void PopulateEthnicities(DatabaseContext context)
  {
    context.Ethnicities.RemoveRange(context.Ethnicities);

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("3C69F5AE-073F-F180-3CAC-2197EB73E369"),
      displayName: "Indian",
      groupName: "Asian or Asian British",
      census2001: "Indian or British Indian",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 1,
      nhsDataDictionary2001Code: "H",
      nhsDataDictionary2001Description: "Asian or Asian British - Indian"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("76D69A87-D9A7-EAC6-2E2D-A6017D02E04F"),
      displayName: "Pakistani",
      groupName: "Asian or Asian British",
      census2001: "Pakistani or British Pakistani",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 2,
      nhsDataDictionary2001Code: "J",
      nhsDataDictionary2001Description: "Asian or Asian British - Pakistani"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("5BF8BFAB-DAB1-D472-51CA-9CF0CB056D3F"),
      displayName: "Bangladeshi",
      groupName: "Asian or Asian British",
      census2001: "Pakistani or British Pakistani",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 3,
      nhsDataDictionary2001Code: "K",
      nhsDataDictionary2001Description:
        "Asian or Asian British - Bangladeshi"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("EFC61F30-F872-FA71-9709-1A416A51982F"),
      displayName: "Chinese",
      groupName: "Asian or Asian British",
      census2001: "Chinese",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 4,
      nhsDataDictionary2001Code: "R",
      nhsDataDictionary2001Description: "Other Ethnic Groups - Chinese"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("CB5CA465-C397-A34F-F32B-729A38932E0E"),
      displayName: "Any other Asian background",
      groupName: "Asian or Asian British",
      census2001: "Other Asian background",
      triageName: "Asian",
      minimumBmi: 27.50M,
      groupOrder: 3,
      displayOrder: 5,
      nhsDataDictionary2001Code: "L",
      nhsDataDictionary2001Description:
        "Asian or Asian British - Any other Asian background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("F6C29207-A3FC-163B-94BC-2CE840AF9396"),
      displayName: "African",
      groupName: "Black, African, Caribbean or Black British",
      census2001: "African",
      triageName: "Black",
      minimumBmi: 27.50M,
      groupOrder: 4,
      displayOrder: 1,
      nhsDataDictionary2001Code: "N",
      nhsDataDictionary2001Description: "Black or Black British - African"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("36FE1D6A-3B04-5A31-FBD9-8D378C2CB86A"),
      displayName: "Caribbean",
      groupName: "Black, African, Caribbean or Black British",
      census2001: "Caribbean",
      triageName: "Black",
      minimumBmi: 27.50M,
      groupOrder: 4,
      displayOrder: 2,
      nhsDataDictionary2001Code: "M",
      nhsDataDictionary2001Description: "Black or Black British - Caribbean"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("E0694F9A-2D9E-BEF6-2F46-6EB9FB7891AD"),
      displayName: "Any other Black, African or Caribbean background",
      groupName: "Black, African, Caribbean or Black British",
      census2001: "Other Black background",
      triageName: "Black",
      minimumBmi: 27.50M,
      groupOrder: 4,
      displayOrder: 3,
      nhsDataDictionary2001Code: "P",
      nhsDataDictionary2001Description:
        "Black or Black British - Any other Black background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("3185A21D-2FD4-4313-4A59-43DB28A2E89A"),
      displayName: "White and Black Caribbean",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "White and Black Caribbean",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 1,
      nhsDataDictionary2001Code: "D",
      nhsDataDictionary2001Description: "Mixed - White and Black Caribbean"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("EDFE5D64-E5D8-9D27-F9C5-DC953D351CF7"),
      displayName: "White and Black African",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "White and Black African",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 2,
      nhsDataDictionary2001Code: "E",
      nhsDataDictionary2001Description: "Mixed - White and Black African"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("279DC2CB-6F4B-96BC-AE72-B96BF7A2579A"),
      displayName: "White and Asian",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "White and Asian",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 3,
      nhsDataDictionary2001Code: "F",
      nhsDataDictionary2001Description: "Mixed - White and Asian"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("4E84EFCD-3DBA-B459-C302-29BCBD9E8E64"),
      displayName: "Any other Mixed or Multiple ethnic background",
      groupName: "Mixed or Multiple ethnic groups",
      census2001: "Any other Mixed background",
      triageName: "Mixed",
      minimumBmi: 27.50M,
      groupOrder: 2,
      displayOrder: 4,
      nhsDataDictionary2001Code: "G",
      nhsDataDictionary2001Description: "Mixed - Any other mixed background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("5DC90D60-F03C-3CE6-72F6-A34D4E6F163B"),
      displayName: "English, Welsh, Scottish, Northern Irish or British",
      groupName: "White",
      census2001: "British",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 1,
      nhsDataDictionary2001Code: "A",
      nhsDataDictionary2001Description: "White - British"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("5D2B37FD-24C4-7572-4AEA-D437C6E17318"),
      displayName: "Irish",
      groupName: "White",
      census2001: "Irish",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 2,
      nhsDataDictionary2001Code: "B",
      nhsDataDictionary2001Description: "White - Irish"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("A1B8C48B-FA12-E001-9F8E-3C9BA9D3D065"),
      displayName: "Gypsy or Irish Traveller",
      groupName: "White",
      census2001: "Other White background",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 3,
      nhsDataDictionary2001Code: null,
      nhsDataDictionary2001Description: null));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("75E8313C-BFDF-5ABF-B6DA-D6CA64138CF4"),
      displayName: "Any other White background",
      groupName: "White",
      census2001: "Any other White background",
      triageName: "White",
      minimumBmi: 30.00M,
      groupOrder: 1,
      displayOrder: 4,
      nhsDataDictionary2001Code: "C",
      nhsDataDictionary2001Description: "White - Any other White background"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("934A2FA6-F541-60F1-D08D-46F5E647A28D"),
      displayName: "Arab",
      groupName: "Other ethnic group",
      census2001: "Other - ethnic category",
      triageName: "Other",
      minimumBmi: 27.50M,
      groupOrder: 5,
      displayOrder: 1,
      nhsDataDictionary2001Code: null,
      nhsDataDictionary2001Description: null));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("D15B2787-7926-1EF6-704E-1012F9298AE1"),
      displayName: "Any other ethnic group",
      groupName: "Other ethnic group",
      census2001: "Other - ethnic category",
      triageName: "Other",
      minimumBmi: 27.50M,
      groupOrder: 5,
      displayOrder: 2,
      nhsDataDictionary2001Code: "S",
      nhsDataDictionary2001Description:
        "Other Ethnic Groups - Any other ethnic group"));

    context.Ethnicities.Add(RandomEntityCreator.CreateRandomEthnicity(
      id: Guid.Parse("95b0feb5-5ece-98ed-1269-c71e327e98c5"),
      displayName: "I do not wish to Disclose my Ethnicity",
      groupName: "I do not wish to Disclose my Ethnicity",
      census2001: null,
      triageName: "Other",
      minimumBmi: 30.00M,
      groupOrder: 6,
      displayOrder: 1,
      nhsDataDictionary2001Code: "Z",
      nhsDataDictionary2001Description: "Not stated"));

    context.SaveChanges();
  }
}
