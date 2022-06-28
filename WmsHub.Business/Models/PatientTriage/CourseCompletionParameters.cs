namespace WmsHub.Business.Models.PatientTriage
{
  public class CourseCompletionParameters
  {
    public Enums.AgeGroup AgeGroup { get; set; }
    public Enums.Sex Sex { get; set; }
    public Enums.Ethnicity Ethnicity { get; set; }
    public Enums.Deprivation Deprivation { get; set; }

    public CourseCompletionParameters(
      Enums.AgeGroup ageGroup,
      Enums.Sex sex,
      Enums.Ethnicity ethnicity,
      Enums.Deprivation deprivation)
    {
      AgeGroup = ageGroup;
      Sex = sex;
      Ethnicity = ethnicity;
      Deprivation = deprivation;
    }

    public CourseCompletionParameters(
      int age,
      Enums.Sex sex,
      Enums.Ethnicity ethnicity,
      Enums.Deprivation deprivation)
    {
      AgeGroup = CalculateAgeGroupForAge(age);
      Sex = sex;
      Ethnicity = ethnicity;
      Deprivation = deprivation;
    }

    private static Enums.AgeGroup CalculateAgeGroupForAge(int age)
    {
      const int MAXAGEBAND00to39 = 39;
      const int MAXAGEBAND40to44 = 44;
      const int MAXAGEBAND45to49 = 49;
      const int MAXAGEBAND50to54 = 54;
      const int MAXAGEBAND55to59 = 59;
      const int MAXAGEBAND60to64 = 64;
      const int MAXAGEBAND65to69 = 69;
      const int MAXAGEBAND70to74 = 74;

      if (age <= MAXAGEBAND00to39)
      {
        return Enums.AgeGroup.Age00to39;
      }
      else if (age <= MAXAGEBAND40to44)
      {
        return Enums.AgeGroup.Age40to44;
      }
      else if (age <= MAXAGEBAND45to49)
      {
        return Enums.AgeGroup.Age45to49;
      }
      else if (age <= MAXAGEBAND50to54)
      {
        return Enums.AgeGroup.Age50to54;
      }
      else if (age <= MAXAGEBAND55to59)
      {
        return Enums.AgeGroup.Age55to59;
      }
      else if (age <= MAXAGEBAND60to64)
      {
        return Enums.AgeGroup.Age60to64;
      }
      else if (age <= MAXAGEBAND65to69)
      {
        return Enums.AgeGroup.Age65to69;
      }
      else if (age <= MAXAGEBAND70to74)
      {
        return Enums.AgeGroup.Age70to74;
      }
      else
      {
        return Enums.AgeGroup.Age75Plus;
      }
    }
  }
}
