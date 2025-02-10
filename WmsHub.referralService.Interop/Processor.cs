using Microsoft.Office.Interop.Word;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace WmsHub.ReferralService.Interop;

//[ComVisible(true),
// Guid("C54B5663-36E7-4015-82BA-B4DCBFAE1644")]
//[ProgId("DocumentToPdf.Core.Processor")]
//[ClassInterface(ClassInterfaceType.None)]
public class Processor(ILogger _log) : IProcessor
{
  private static Application _wordApp = null;

  public async Task<InteropResult> ConvertToPdfInteropAsync(
    byte[] byteArray,
    string docType,
    string saveLocation,
    bool reformatDocument,
    string[] sectionHeadings)
  {
    InteropResult result = new();
    PrepareWordApp();

    if (_wordApp == null)
    {
      result.ErrorText = "Word was not loaded";
      result.WordError = true;
      result.Data = null;
    }
    else
    {
      try
      {
        Guid docId = Guid.NewGuid();
        string docPath = $"{saveLocation}\\{docId}{docType}";
        string pdfPath = $"{saveLocation}\\{docId}.pdf";

        using (FileStream file = File.OpenWrite(docPath))
        {
          file.Write(byteArray, 0, byteArray.Length);
        }

        Document doc = _wordApp.Documents.Open(docPath);

        if (reformatDocument)
        {
          if (IdentifiedAsReferralDocument(doc))
          {
            RemoveDocumentProtection(doc);
            RemoveHeadersAndFooters(doc);
            InsertPageBreaksBetweenSections(doc, sectionHeadings);
          }
        }
        else
        {
          _log.Debug("Skipping document reformatting as config setting 'ReformatDocument' was " +
            "either not set, or set to 'false'.");
        }

        try
        {
          doc.ExportAsFixedFormat(pdfPath, WdExportFormat.wdExportFormatPDF);
        }
        catch (Exception ex)
        {
          _log.Error(ex, "ExportAsFixedFormat failed.");
          
          result.ErrorText = "Error exporting document to PDF.";
          result.ExportError = true;
        }
        finally
        {
          doc.Close();
          File.Delete(docPath);
        }

        if (File.Exists(pdfPath))
        {
          result.Data = await File.ReadAllBytesAsync(pdfPath);
          File.Delete(pdfPath);
        }
      }
      catch(Exception ex)
      {
        _log.Warning(ex, "ConvertToPdfInteropAsync failed.");
        if (!result.ExportError)
        {
          result.ErrorText = "Error when processing document.";
          result.WordError = true;
        }
      }
    }
    return result;
  }

  //Identify a referral document by the "SYSTEM:" tag which all documents
  //should contain.
  private bool IdentifiedAsReferralDocument(Document document)
  {
    if (document == null)
    {
      _log.Warning("Document was null. Unable to identify as a referral document.");
      return false;
    }

    foreach (Section section in document.Sections)
    {
      Word.Range headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
      if (headerRange == null)
      {
        _log.Debug("No header was found in document.");
      }
      else
      {
        string headerText = headerRange.Text;
        if (headerText.Contains("SYSTEM", StringComparison.InvariantCultureIgnoreCase))
        {
          return true;
        }
      }

      Word.Range footerRange = section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
      if (footerRange == null)
      {
        _log.Debug("No footer was found in document.");
      }
      else
      {
        string footerText = footerRange.Text;
        if (footerText.Contains("SYSTEM", StringComparison.InvariantCultureIgnoreCase))
        {
          return true;
        }
      }

      if (section.Range.Text.Contains("SYSTEM", StringComparison.InvariantCultureIgnoreCase))
      {
        return true;
      }
    }
    return false;
  }

  private void RemoveDocumentProtection(Document document)
  {
    if (document == null)
    {
      _log.Warning("Document was null. Did not remove protection.");
      return;
    }

    try
    {
      if (document.ProtectionType == WdProtectionType.wdNoProtection)
      {
        _log.Debug("Document is not protected.");
      }
      else
      {
        if (document.HasPassword)
        {
          document.Password = null;
        }
        document.Unprotect();
        _log.Debug("Document has been unprotected.");
      }

    }
    catch (Exception ex)
    {
      _log.Information(ex, $"RemoveDocumentProtection failed.");
    }

  }

  /// <summary>
  /// Remove headers and footers, but retain the content by adding it
  /// to the end of the document
  /// </summary>
  /// <param name="document"></param>
  private void RemoveHeadersAndFooters(Document document)
  {
    if (document == null)
    {
      _log.Warning("Document was null. Did not remove headers or footers.");
      return;
    }
    try
    {
      Paragraph lastParagraph = document.Paragraphs.Last;
      if (lastParagraph == null)
      {
        _log.Debug("Document is empty");
      }
      else
      {

        foreach (Section section in document.Sections)
        {
          string headerText = "";
          string footerText = "";
          //Move the content of the footer to the bottom of the document
          Word.Range footerRange =
          section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
          if (footerRange == null)
          {
            _log.Debug("No Footer");
          }
          else
          {
            footerText = footerRange.Text;
            footerRange.Delete();
          }

          Word.Range headerRange =
          section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
          if (headerRange == null)
          {
            _log.Debug("No Header");
          }
          else
          {
            headerText = headerRange.Text;
            headerRange.Delete();
          }

          document.Paragraphs.Add();
          Paragraph newParagraph = document.Paragraphs.Last;
          newParagraph.Range.Text = $"HEADER:\r{headerText}\rFOOTER:\r{footerText}";
        }
      }
    }
    catch (Exception ex)
    {
      _log.Information(ex, $"RemoveHeadersAndFooters failed.");
    }
  }

  private void InsertPageBreaksBetweenSections(Document document, string[] sectionHeadings)
  {
    if (document == null)
    {
      _log.Warning("Document was null. Did not add page breaks.");
      return;
    }

    try
    {
      if (sectionHeadings == null)
      {
        _log.Debug("No Section Headings are Configured.");
        return;
      }

      foreach (Paragraph paragraph in document.Paragraphs)
      {
        foreach (string heading in sectionHeadings)
        {
          if (paragraph.Range.Text.Contains(heading, StringComparison.InvariantCultureIgnoreCase))
          {
            paragraph.Range.Words.First.InsertBreak(WdBreakType.wdPageBreak);
          }
        }
      }
    }
    catch (Exception ex)
    {
      _log.Information(ex, $"InsertPageBreaksBetweenSections failed.");
    }

  }

  private Application PrepareWordApp(bool retryOnFail = true)
  {
    try
    {
      if (_wordApp == null)
      {
        _log.Debug("Loading Word Instance");
        OpenWord();
      }
      _wordApp.Visible = false;
    }
    catch (Exception ex)
    {
      _log.Error(ex, "PrepareWordApp failed.");
      _wordApp = null;
      if (retryOnFail)
      {
        PrepareWordApp(false);
      }
    }

    return _wordApp;
  }

  private void OpenWord()
  {
    _wordApp = new Application();
    _wordApp.WordBasic.DisableAutoMacros();
  }

  public void CloseWordSingletonIfItExists()
  {
    try
    {
      if (_wordApp != null)
      {
        _log.Debug("Closing Word Instance");
        //Check for open documents and close them
        foreach (Document openDocument in _wordApp.Documents)
        {
          _log.Debug($"Closing {openDocument.Name}.");
          openDocument.Close(false);
        }
        _wordApp.Quit();
#pragma warning disable CA1416 // Validate platform compatibility
        Marshal.ReleaseComObject(_wordApp);
#pragma warning restore CA1416 // Validate platform compatibility
      }
    }
    catch (Exception ex)
    {
      _log.Information(ex, "CloseWordSingletonIfItExists failed.");
    }
    finally
    {
      _wordApp = null;
    }
  }
}