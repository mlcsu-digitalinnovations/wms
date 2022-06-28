using Serilog;
using System;
using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using WmsHub.ReferralsService.Pdf.Models.Configuration;

namespace WmsHub.ReferralsService.Pdf
{
  public class Parser
  {
    const string ERROR_NO_PAGES = "Cannot process document as no pages were " +
      "provided.";

    //Default values
    int _minimumQuestionRows = 5;
    int _minimumColons = 20;
    double _columnXTolerance = 10;
    int _marginSize = 40;

    int? _numberOfColons = null;

    ILogger _log;

    readonly List<string> _column1 = new();
    readonly List<string> _column2 = new();

    public bool? QuestionsDetected
    {
      get
      {
        if (_numberOfColons == null)
        {
          return null;
        }
        else
        {
          return (_numberOfColons >= _minimumColons);
        }
      }
    }

    
    /// <summary>
    /// A new instance of the PDF rows parser
    /// </summary>
    /// <param name="log">Logger. Set to null for no logging.</param>
    /// <param name="config">Configuration containing margins etc. If
    /// none is provided then default values will be used.</param>
    public Parser(ILogger log, PdfParserConfig config)
    {
      _log = log;
      if (config != null)
      {
        _minimumQuestionRows = config.MinimumQuestionRows;
        _minimumColons = config.MinimumColons;
        _columnXTolerance = config.ColumnXTolerance;
        _marginSize = config.MarginSize;
      }
    }

    /// <summary>
    /// Process a set of pages.
    /// </summary>
    /// <param name="pages">pages to process</param>
    /// <returns>An array of strings containing two columns of text</returns>
    public List<string> ProcessWords(IEnumerable<Page> pages)
    {
      if (pages == null)
      {
        throw new ArgumentNullException(ERROR_NO_PAGES);
      }

      List<string> result = new();
      
      bool twoColumn = false;
      double secondColumnStart = SecondColumnPosition(pages);
      if (secondColumnStart == 0)
      {
        _log?.Debug("PDF Parser: Single column document detected");
      }
      else
      {
        _log?.Debug("PDF Parser: Two column document detected");
        twoColumn = true;
      }

      foreach (Page page in pages)
      {
        string rowC1 = "";
        string rowC2 = "";
        //Add a separator to the two columns.  This will stop footers from
        //being carried into the next page, and isolate headers by forcing
        //them to be treated as question answers
        _column1.Add("PageBreak:");
        if (twoColumn)
        {
          _column2.Add("PageBreak:");
        }
        IEnumerable<Word> words = page.GetWords();

        double previousWordX = 0;

        foreach (Word word in words)
        {
          if (previousWordX == 0 || word.BoundingBox.Left <= previousWordX)
          {
            //New line detected
            AddRow(rowC1, rowC2);
            rowC1 = "";
            rowC2 = "";
          }
          if (twoColumn && 
            word.BoundingBox.Left >= secondColumnStart - _columnXTolerance)
          {
            rowC2 = $"{rowC2} {word.Text}";
          }
          else
          {
            rowC1 = $"{rowC1} {word.Text}";
          }
          //A word will be judged to be on the next line if the end of the
          //previous word occurs to the right of the next word.
          previousWordX =
            word.BoundingBox.Left + word.BoundingBox.Width;

        } //word
        AddRow(rowC1, rowC2);
      } //page

      result.AddRange(_column1);
      if (twoColumn)
      {
        result.AddRange(_column2);
      }

      return result;
    }

    void AddRow(string column1Content, string column2Content)
    {
      if (!string.IsNullOrWhiteSpace(column1Content))
      {
        _column1.Add(column1Content.Trim());
      }
      if (!string.IsNullOrWhiteSpace(column2Content))
      {
        _column2.Add(column2Content.Trim());
      }
    }

    /// <summary>
    /// So the provided pages appear to have two columns
    /// </summary>
    /// <param name="pages"></param>
    /// <returns>True if two columns have been detected</returns>
    public bool HasTwoColumns(IEnumerable<Page> pages)
    {
      if (pages == null)
      {
        throw new ArgumentNullException(ERROR_NO_PAGES);
      }

      return (SecondColumnPosition(pages) > 0);
    }

    /// <summary>
    /// The X Coordinate of the second column
    /// </summary>
    /// <param name="pages"></param>
    /// <returns>X Coordinate value, or zero for a single column page</returns>
    double SecondColumnPosition(IEnumerable<Page> pages)
    {
      if (pages == null)
      {
        throw new ArgumentNullException(ERROR_NO_PAGES);
      }

      double result = 0;
      double currentX = 0;
      bool checkWord = false;
      
      _numberOfColons = 0;
      List<ValueCounter> Values = new();

      foreach (Page page in pages)
      {
        IEnumerable<Word> words = page.GetWords();
        foreach (Word word in words)
        {
          if (checkWord)
          {
            //Don't bother to check if the current word is further left then the
            //previous one - that means a new row has begun.
            if (currentX <= word.BoundingBox.Left)
            {
              bool foundCounter = false;
              foreach (ValueCounter counter in Values)
              {
                if (counter.Value == word.BoundingBox.Left)
                {
                  foundCounter = true;
                  counter.Count++;
                  if (counter.Count >= _minimumQuestionRows)
                  {
                    result = counter.Value;
                  }
                  break;
                }
              }
              if (!foundCounter)
              {
                Values.Add(new ValueCounter()
                {
                  Value = word.BoundingBox.Left,
                  Count = 1
                });
              }
            }
            checkWord = false;
          }
          if (result >= _marginSize)
          {
            break;
          }
          if (word.Text.EndsWith(':'))
          {
            _numberOfColons++;
            checkWord = true;
            currentX = word.BoundingBox.Left;
          }
        }//words in page

        //Stop processing pages if column 2 has been detected
        if (result >= _marginSize)
        {
          break;
        }
      }//pages
      if (result < _marginSize)
      {
        result = 0;
      }
      return result;
    }

  }
}
