using System.Diagnostics.CodeAnalysis;

namespace WmsHub.ReferralsService.Pdf.Models.Configuration
{
  [ExcludeFromCodeCoverage]
  public class PdfParserConfig
  {
    /// <summary>
    /// The number of pixels either side of the second column position which
    /// will determine which column any given text is located in.  This is to
    /// allow for minute inaccuracies in form layouts.
    /// </summary>
    public double ColumnXTolerance { get; set; }
    /// <summary>
    /// If an unidentified document contains fewer words ending with a colon
    /// than this value it won't bother to attempt all of the template
    /// formats.
    /// </summary>
    public int MinimumColons { get; set; }
    /// <summary>
    /// When determining if a form contains two columns, system looks at the
    /// X coordinate of the next word after any word containing a colon. If
    /// it finds the same coordinate a number of times then the system will stop
    /// looking.
    /// </summary>
    public int MinimumQuestionRows { get; set; }
    /// <summary>
    /// Text to the left of the margin will be considered to be in the first
    /// column.
    /// </summary>
    public int MarginSize { get; set; }
    
    public RtfPreprocessorConfig RtfPreprocessorConfig {  get; set; }
  }
}
