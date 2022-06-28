using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.PatientTriage
{
  public class CourseCompletion : ICourseCompletion
  {
    public int MinimumPossibleScoreCompletion { get; set; }
    public int MaximumPossibleScoreCompletion { get; set; }
    public int MinimumPossibleScoreWeight { get; set; }
    public int MaximumPossibleScoreWeight { get; set; }
    public int LowCategoryLowScoreCompletion { get; set; }
    public int MediumCategoryLowScoreCompletion { get; set; }
    public int HighCategoryLowScoreCompletion { get; set; }
    public int LowCategoryHighScoreCompletion { get; set; }
    public int MediumCategoryHighScoreCompletion { get; set; }
    public int HighCategoryHighScoreCompletion { get; set; }
    public int LowCategoryLowScoreWeight { get; set; }
    public int MediumCategoryLowScoreWeight { get; set; }
    public int HighCategoryLowScoreWeight { get; set; }
    public int LowCategoryHighScoreWeight { get; set; }
    public int MediumCategoryHighScoreWeight { get; set; }
    public int HighCategoryHighScoreWeight { get; set; }
  }
}
