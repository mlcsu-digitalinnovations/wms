using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Business.Entities;

namespace WmsHub.Utilities.Seeds;

public class ElectiveCarePostSeeder: SeederBaseBase
{
  private static string[] _errorMessages = new[]
  {
    "The field 'Date of Birth' must equate to an age between 18 and 110.",
    "The field 'Date of Birth' cannot be in the future.",
    "The field 'Date of Trust Reported BMI' cannot be in the future.",
    "The field 'Date Placed On Waiting List' cannot be more than 3 years ago.",
    "The field 'Date Placed On Waiting List' cannot be in the future.",
    "The field 'Family Name' contains invalid characters.",
    "The field 'Given Name' contains invalid characters.",
    "The field 'Mobile' must be a valid UK mobile number.",
    "The field 'Mobile' does not contain enough digits to be a valid UK " +
    "mobile number.",
    "The field 'NHS Number' is invalid.",
    "The field 'OPCS surgery code(s)' does not contain a valid OPCS code.",
    "The field 'OPCS surgery code(s)' does not contain an eligible OPCS code.",
    "The field 'Postcode' does not contain an existing English postcode.",
    "The field 'Postcode' does not contain a valid English postcode.",
    "The field 'Sex at Birth' must be one of the following: M, MALE, F or " +
    "FEMALE.",
    "The field 'Surgery in less than 18 weeks?' must be N, No or False to be" +
    " eligible.",
    "The field 'Trust ODS code' does not contain the expected ODS code of RLY.",
    "The field 'Trust Reported BMI' must be between 27.5 and 90.",
    "The field 'Trust Reported BMI' does not contain an eligible BMI for the" +
    " provided ethnicity.",
    "The field 'Spell Identifier' must not exceed 20 characters",
    "The field 'Spell Identifier' is a duplicate of row 2.",
    "The field 'NHS Number' is a duplicate of row 2.",
    "The field 'Trust Reported BMI' does not contain an eligible BMI for the" +
    " provided ethnic group.",
    "The field 'Family Name' does not contain any alpha characters.",
    "The field 'Given Name' does not contain any alpha characters.",
    "The field 'Surgery in less than 18 weeks?' must have a value and not " +
    "be blank."
  };
  public static void AddErrors()
  {
    Guid trustUserId = Guid.NewGuid();
    Random random = new();
    int numberOfRows = random.Next(6, 20);
    for (int j = 0; j < numberOfRows; j++)
    {
      List<int> errorList = new List<int>();
      for (int i = 0; i < 10; i++)
      {
        errorList.Add(random.Next(0, _errorMessages.Length));       
      }
      foreach(int errorNum in errorList.Distinct())
      {
        ElectiveCarePostError error = new()
        {
          PostError = _errorMessages[errorNum],
          ProcessDate = DateTime.Now,
          RowNumber = j,
          TrustOdsCode = "TST1",
          TrustUserId = trustUserId
        };

        DatabaseContext.ElectiveCarePostErrors.Add(error);
        DatabaseContext.SaveChanges();
      }
    }
  }
  public static void Clean()
  {
    DatabaseContext.ElectiveCarePostErrors
      .RemoveRange(DatabaseContext.ElectiveCarePostErrors);
  }

  internal void ReSeed()
  {
    Clean();
    AddErrors();
  }
}
