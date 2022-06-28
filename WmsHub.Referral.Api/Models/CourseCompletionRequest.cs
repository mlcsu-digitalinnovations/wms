using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WmsHub.Referral.Api.Models
{
  public class CourseCompletionRequest
  {
    [Required, Range(1,50)]
    public int MinimumPossibleScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int MaximumPossibleScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int MinimumPossibleScoreWeight { get; set; }
    [Required,Range(1,50)]
    public int MaximumPossibleScoreWeight { get; set; }
    [Required,Range(1,50)]
    public int LowCategoryLowScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int MediumCategoryLowScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int HighCategoryLowScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int LowCategoryHighScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int MediumCategoryHighScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int HighCategoryHighScoreCompletion { get; set; }
    [Required,Range(1,50)]
    public int LowCategoryLowScoreWeight { get; set; }
    [Required,Range(1,50)]
    public int MediumCategoryLowScoreWeight { get; set; }
    [Required,Range(1,50)]
    public int HighCategoryLowScoreWeight { get; set; }
    [Required,Range(1,50)]
    public int LowCategoryHighScoreWeight { get; set; }
    [Required,Range(1,50)]
    public int MediumCategoryHighScoreWeight { get; set; }
    [Required,Range(1,50)]
    public int HighCategoryHighScoreWeight { get; set; }
  }
}
