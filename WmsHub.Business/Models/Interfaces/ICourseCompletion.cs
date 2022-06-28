namespace WmsHub.Business.Models.Interfaces
{
  public interface ICourseCompletion
  {
    int MinimumPossibleScoreCompletion { get; set; }
    int MaximumPossibleScoreCompletion { get; set; }
    int MinimumPossibleScoreWeight { get; set; }
    int MaximumPossibleScoreWeight { get; set; }
    int LowCategoryLowScoreCompletion { get; set; }
    int MediumCategoryLowScoreCompletion { get; set; }
    int HighCategoryLowScoreCompletion { get; set; }
    int LowCategoryHighScoreCompletion { get; set; }
    int MediumCategoryHighScoreCompletion { get; set; }
    int HighCategoryHighScoreCompletion { get; set; }
    int LowCategoryLowScoreWeight { get; set; }
    int MediumCategoryLowScoreWeight { get; set; }
    int HighCategoryLowScoreWeight { get; set; }
    int LowCategoryHighScoreWeight { get; set; }
    int MediumCategoryHighScoreWeight { get; set; }
    int HighCategoryHighScoreWeight { get; set; }
  }
}