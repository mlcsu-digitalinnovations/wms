using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using WmsHub.ReferralService.Interop;
using Word = Microsoft.Office.Interop.Word;

namespace WmsHub.ReferralsService.Interop
{
  //[ComVisible(true),
  // Guid("C54B5663-36E7-4015-82BA-B4DCBFAE1644")]
  //[ProgId("DocumentToPdf.Core.Processor")]
  //[ClassInterface(ClassInterfaceType.None)]
  public class Processor : IProcessor
  {
    static Application _wordApp = null;

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

          using (Stream file = File.OpenWrite(docPath))
          {
            file.Write(byteArray, 0, (int)byteArray.Length);
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
            Console.WriteLine("Skipping document reformatting as config setting" +
              "'ReformatDocument' was either not set, or set to 'false'.");
          }
          try
          {
            doc.ExportAsFixedFormat(pdfPath, WdExportFormat.wdExportFormatPDF);
          }
          catch
          {
            result.WordError = true;
            result.ExportError = true;
            result.ErrorText = "Error exporting document to PDF.";
          }
          doc.Close();
          byte[] pdfBytes = await File.ReadAllBytesAsync(pdfPath);
          Cleanup(docPath, pdfPath);
          result.Data = pdfBytes;
        }
        catch
        {
          if (!result.ExportError)
          {
            result.ErrorText = "Error when processing document.";
            result.WordError = true;
          }
        }
      }
      return result;
    }

    public void Cleanup(string docPath, string pdfPath)
    {
      //TODO: Evaluate why this doesn't work and possibly fix to replace
      //the current catch-all Word killer added to the main program
      //WordHelper.KillProcess();
      //TODO: Store this in a file just in case system crashes
      //Or use a tempo folder and delete everything on init
      // but for this
      File.Delete(docPath);
      File.Delete(pdfPath);
    }

    //Identify a referral document by the "SYSTEM:" tag which all documents
    //should contain.
    bool IdentifiedAsReferralDocument(Document document)
    {
      if (document == null)
      {
        Console.WriteLine("Document was null.");
        return false;
      }

      foreach (Section section in document.Sections)
      {
        Word.Range headerRange =
            section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
        if (headerRange == null)
        {
          Console.WriteLine("No Header");
        }
        else
        {
          string headerText = headerRange.Text;
          if (headerText.Contains(
            "SYSTEM", StringComparison.InvariantCultureIgnoreCase))
          {
            return true;
          }
        }

        Word.Range footerRange =
          section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
        if (footerRange == null)
        {
          Console.WriteLine("No Footer");
        }
        else
        {
          string footerText = footerRange.Text;
          if (footerText.Contains(
            "SYSTEM", StringComparison.InvariantCultureIgnoreCase))
          {
            return true;
          }
        }

        if (section.Range.Text.Contains(
          "SYSTEM", StringComparison.InvariantCultureIgnoreCase))
        {
          return true;
        }
      }
      return false;
    }

    void RemoveDocumentProtection(Document document)
    {
      if (document == null)
      {
        Console.WriteLine("Document was null. Did not remove protection.");
        return;
      }

      try
      {
        if (document.ProtectionType == WdProtectionType.wdNoProtection)
        {
          Console.WriteLine("Document is not protected.");
        }
        else
        {
          if (document.HasPassword)
          {
            document.Password = null;
          }
          document.Unprotect();
          Console.WriteLine("Document has been unprotected.");
        }

      }
      catch (Exception ex)
      {
        Console.Write($"Error removing Protection: {ex.Message}");
      }

    }

    /// <summary>
    /// Remove headers and footers, but retain the content by adding it
    /// to the end of the document
    /// </summary>
    /// <param name="document"></param>
    void RemoveHeadersAndFooters(Document document)
    {
      if (document == null)
      {
        Console.WriteLine("Document was null. Did not remove Headers or " +
          "Footers");
        return;
      }
      try
      {
        Paragraph lastParagraph = document.Paragraphs.Last;
        if (lastParagraph == null)
        {
          Console.WriteLine("Document is empty");
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
              Console.WriteLine("No Footer");
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
              Console.WriteLine("No Header");
            }
            else
            {
              headerText = headerRange.Text;
              headerRange.Delete();
            }

            document.Paragraphs.Add();
            Paragraph newParagraph = document.Paragraphs.Last;
            newParagraph.Range.Text =
              $"HEADER:\r{headerText}\rFOOTER:\r{footerText}";
          }
        }
      }
      catch (Exception ex)
      {
        Console.Write($"Error removing Headers and Footers: {ex.Message}");
      }
    }

    void InsertPageBreaksBetweenSections(Document document,
      string[] sectionHeadings)
    {
      if (document == null)
      {
        Console.WriteLine("Document was null. Did not add page breaks.");
        return;
      }

      try
      {
        if (sectionHeadings == null)
        {
          Console.WriteLine("No Section Headings are Configured.");
          return;
        }
        if (document == null)
        {
          Console.WriteLine("Document was null.");
          return;
        }
        foreach (Paragraph paragraph in document.Paragraphs)
        {
          foreach (string heading in sectionHeadings)
          {
            if (paragraph.Range.Text.Contains(
              heading, StringComparison.InvariantCultureIgnoreCase))
            {
              paragraph.Range.Words.First.InsertBreak(WdBreakType.wdPageBreak);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.Write($"Error inserting Page Breaks: {ex.Message}");
      }

    }

    Application PrepareWordApp(bool retryOnFail = true)
    {
      try
      {
        if (_wordApp == null)
        {
          Console.WriteLine("Loading Word Instance");
          OpenWord();
        }
        _wordApp.Visible = false;
      }
      catch 
      {
        _wordApp = null;
        if (retryOnFail)
        {
          PrepareWordApp(false);
        }
      }

      return _wordApp;
    }

    void OpenWord()
    {
      _wordApp = new Application();
      _wordApp.WordBasic.DisableAutoMacros();
    }

    public static void CloseWordSingletonIfItExists()
    {
      try
      {
        if (_wordApp != null)
        {
          Console.WriteLine("Closing Word Instance");
          //Check for open documents and close them
          foreach(Document openDocument in _wordApp.Documents)
          {
            Console.WriteLine($"Closing {openDocument.Name}.");
            openDocument.Close(false);
          }
          _wordApp.Quit();
#pragma warning disable CA1416 // Validate platform compatibility
          Marshal.ReleaseComObject(_wordApp);
#pragma warning restore CA1416 // Validate platform compatibility
        }
      }
      catch
      {
        Console.WriteLine("Could not close Word Instance");
      }
      finally
      {
        _wordApp = null;
      }
    }
  }
}
