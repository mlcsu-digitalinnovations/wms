using System;

namespace WmsHub.Business.Models.PatientTriage
{
  public class CourseCompletionResult
  {
    const int UNASSIGNEDSCORE = -1;

    public CourseCompletionResult()
    { }

    public int MinimumPossibleScoreCompletion { get; set; }
    public int MinimumPossibleScoreWeight { get; set; }
    public int LowCategoryLowScoreCompletion { get; set; }
    public int LowCategoryLowScoreWeight { get; set; }
    public int LowCategoryHighScoreCompletion { get; set; }
    public int MediumCategoryLowScoreCompletion { get; set; }
    public int LowCategoryHighScoreWeight { get; set; }
    public int MediumCategoryLowScoreWeight { get; set; }
    public int MediumCategoryHighScoreCompletion { get; set; }
    public int HighCategoryLowScoreCompletion { get; set; }
    public int MaximumPossibleScoreCompletion { get; set; }
    public int HighCategoryHighScoreCompletion { get; set; }
    public int MediumCategoryHighScoreWeight { get; set; }
    public int HighCategoryLowScoreWeight { get; set; }
    public int MaximumPossibleScoreWeight { get; set; }
    public int HighCategoryHighScoreWeight { get; set; }

    public int CompletionScoreAge { get; set; } = UNASSIGNEDSCORE;
    public int CompletionScoreSex { get; set; } = UNASSIGNEDSCORE;
    public int CompletionScoreDeprivation { get; set; } = UNASSIGNEDSCORE;
    public int CompletionScoreEthnicity { get; set; } = UNASSIGNEDSCORE;
    public int WeightScoreAge { get; set; } = UNASSIGNEDSCORE;
    public int WeightScoreSex { get; set; } = UNASSIGNEDSCORE;
    public int WeightScoreDeprivation { get; set; } = UNASSIGNEDSCORE;
    public int WeightScoreEthnicity { get; set; } = UNASSIGNEDSCORE;

    public int TotalCompletionScore
    {
      get
      {
        return CompletionScoreAge +
          CompletionScoreSex +
          CompletionScoreEthnicity +
          CompletionScoreDeprivation;
      }
    }

    public int TotalWeightScore
    {
      get
      {
        return WeightScoreAge +
          WeightScoreSex +
          WeightScoreEthnicity +
          WeightScoreDeprivation;
      }
    }

    public virtual Enums.TriageLevel TriagedCompletionLevel
    {
      get
      {
        if (CompletionScoreAge == UNASSIGNEDSCORE
          || CompletionScoreDeprivation == UNASSIGNEDSCORE
          || CompletionScoreEthnicity == UNASSIGNEDSCORE
          || CompletionScoreSex == UNASSIGNEDSCORE)
        {
          throw new InvalidOperationException(
            "All Scores are not assigned values.");
        }

        Enums.TriageLevel result = Enums.TriageLevel.Undetermined;
        int totalScore = TotalCompletionScore;

        if (totalScore < MinimumPossibleScoreCompletion
          || totalScore > MaximumPossibleScoreCompletion)
        {
          throw new InvalidOperationException(
            $"Total Score is invalid ({totalScore}).");
        }
        else if (totalScore >= LowCategoryLowScoreCompletion
          && totalScore <= LowCategoryHighScoreCompletion)
        {
          result = Enums.TriageLevel.Low;
        }
        else if (totalScore >= MediumCategoryLowScoreCompletion
          && totalScore <= MediumCategoryHighScoreCompletion)
        {
          result = Enums.TriageLevel.Medium;
        }
        else if (totalScore >= HighCategoryLowScoreCompletion
          && totalScore <= HighCategoryHighScoreCompletion)
        {
          result = Enums.TriageLevel.High;
        }

        return result;
      }
    }

    public virtual Enums.TriageLevel TriagedWeightedLevel
    {
      get
      {
        if (WeightScoreAge == UNASSIGNEDSCORE
          || WeightScoreDeprivation == UNASSIGNEDSCORE
          || WeightScoreEthnicity == UNASSIGNEDSCORE
          || WeightScoreSex == UNASSIGNEDSCORE)
        {
          throw new InvalidOperationException(
            "All Scores are not assigned values.");
        }

        Enums.TriageLevel result = Enums.TriageLevel.Undetermined;
        int totalScore = TotalWeightScore;

        if (totalScore < MinimumPossibleScoreWeight ||
          totalScore > MaximumPossibleScoreWeight)
        {
          throw new InvalidOperationException(
            $"Total Score is invalid ({totalScore}).");
        }
        else if (totalScore >= LowCategoryLowScoreWeight
          && totalScore <= LowCategoryHighScoreWeight)
        {
          result = Enums.TriageLevel.Low;
        }
        else if (totalScore >= MediumCategoryLowScoreWeight
          && totalScore <= MediumCategoryHighScoreWeight)
        {
          result = Enums.TriageLevel.Medium;
        }
        else if (totalScore >= HighCategoryLowScoreWeight
          && totalScore <= HighCategoryHighScoreWeight)
        {
          result = Enums.TriageLevel.High;
        }

        return result;
      }
    }
  }
}
