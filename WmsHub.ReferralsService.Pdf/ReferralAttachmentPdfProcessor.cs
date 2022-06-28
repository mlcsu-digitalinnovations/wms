using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using WmsHub.Business.Exceptions;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Models;
using WmsHub.ReferralService.Interop;
using WmsHub.ReferralsService.Pdf.Models.Configuration;
using static WmsHub.Common.Enums;

namespace WmsHub.ReferralsService.Pdf
{
  //This class will download the PDF attachment from the eRS API
  public partial class ReferralAttachmentPdfProcessor
  {
    private readonly ILogger _log;
    private readonly ILogger _auditLog;
    private readonly ReferralAttachmentAnswerMap _answerMap;
    public bool Downloaded { get; set; } = false;

    public byte[] OriginalAttachmentDocument { get; set; }
    public byte[] OriginalUnprocessedPDF { get; set; }
    public string OriginalFileExtension { get; set; }

    public decimal DocumentVersion { get; set; }

    public Dictionary<string, string> DocumentValues { get; set; } =
      new Dictionary<string, string>();

    public List<string> DocumentContent { get; set; } = new List<string>();

    public SourceSystem Source { get; set; } = SourceSystem.Unidentified;
    public decimal PdfVersion { get; set; }
    public string ServiceId { get; set; }
    
    public string ObjectParsingReport { get; set; }
    public bool SupressWarnings { get; set; }
    public bool? QuestionsDetectedInForm { get; set; }
    public virtual bool IsValidFileExtension { get; set; }
    public bool ExportFailed { get; set; }
    public bool InteropFailed { get; set; }
    public string Ubrn { get; set; }

    public PdfParserConfig Config { get; set; }

    public ReferralAttachmentPdfProcessor(
      ILogger log,
      ILogger auditLog,
      string serviceId,
      PdfParserConfig config,
      ReferralAttachmentAnswerMap answerMap)
    {
      _answerMap = answerMap;
      _log = log;
      _auditLog = auditLog;
      Config = config;
      ServiceId = serviceId;
    }
    protected virtual bool ValidFileExtension(string extension)
    {
      extension = extension.Replace(".", "").Trim().ToUpper();
      string[] extensions = new[] { "PDF", "RTF", "DOC", "DOCX" };
      if (Array.Exists(extensions, t => t.Equals(extension.ToUpper())))
      {
        IsValidFileExtension = true;
        return true;
      }

      return false;
    }

    public async virtual Task<bool> DownloadFile(
      string filename, 
      string tempFilePath,
      bool reformatDocument, 
      string[] sectionHeadings, 
      bool retryForAllSources,
      int RetryTolerance)
    {
      bool result = true;

      if (File.Exists(filename))
      {
        byte[] content = File.ReadAllBytes(filename);

        string fileExtension = Path.GetExtension(filename).Replace(".", "");

        result = await ProcessDocument(content, fileExtension, tempFilePath, 
          RetryTolerance, retryForAllSources, reformatDocument, 
          sectionHeadings);
      }
      else
      {
        _log.Verbose($"File {filename} Not Found.");
        result = false;
      }
      return result;
    }

    public async Task<bool> DownloadFile(
      string attachmentPath,
      HttpClient client,
      ErsAttachment attachment,
      string tempFilePath,
      ErsWorkListEntry ersWorkListEntry,
      ErsSession activeSession,
      string accreditedSystemsID,
      string fqdn,
      bool reformatDocument,
      string[] sectionHeadings,
      bool retryForAllSources,
      int retryTolerance
      )
    {
      bool result = true;

      string msgTemplate = 
        "EventType=API Retrieve Clinical Attachment;" +
        "ActionType=GET;" +
        "ResourceType=Clinical Attachment;" +
        "UUID={uuid};" +
        "NHSNumber={nhsNumber};" +
        "UBRN={ubrn};" +
        "SessionID={sessionId};" +
        "OrgName={orgname};" +
        "BusinessFunction={businessFunction};" +
        "ClinicalAttachmentFileName={fileName};" +
        "ClinicalAttachmentAttachmentId={attachmentId};" +
        "ASID={asid};" +
        "FQDN={fqdn};" +
        "ApiMethod={apiMethod};";

      _log.Debug($"A006:Retrieve Clinical Attachment '{attachment?.Title}'" +
        $" for UBRN {ersWorkListEntry.Ubrn}");
      Ubrn = ersWorkListEntry?.Ubrn ?? "Unknown";
      _auditLog.Information(msgTemplate,
        activeSession?.User?.Identifier ?? "Unknown",
        ersWorkListEntry?.NhsNumber ?? "Unknown",
        Ubrn,
        activeSession?.Id ?? "Unknown",
        activeSession?.Permission?.OrgName ?? "Unknown",
        activeSession?.Permission?.BusinessFunction ?? "Unknown",
        attachment?.Title,
        attachment?.Id,
        accreditedSystemsID,
        fqdn,
        "A006-Start");
      HttpResponseMessage response = await client
        .GetAsync($"{attachmentPath}{attachment.Url}");
      _auditLog.Information(msgTemplate,
         activeSession?.User?.Identifier ?? "Unknown",
         ersWorkListEntry?.NhsNumber ?? "Unknown",
         Ubrn,
         activeSession?.Id ?? "Unknown",
         activeSession?.Permission?.OrgName ?? "Unknown",
         activeSession?.Permission?.BusinessFunction ?? "Unknown",
         attachment?.Title,
         attachment?.Id,
         accreditedSystemsID,
         fqdn,
         "A006-End");
      if (response.IsSuccessStatusCode)
      {
        //Populate the documentValues dictionary from the resulting PDF file
        byte[] content = await response.Content.ReadAsByteArrayAsync();
        //(content, file extension, retry for all sources
        result = await ProcessDocument(content, attachment.FileExtension,
          tempFilePath, retryTolerance, retryForAllSources, reformatDocument,
          sectionHeadings);
      }
      else
      {
        _log.Error($"Failed to download attachment from the eReferrals" +
          $" system. Attachment request returned status code " +
          $"{(int)response.StatusCode} : {response.ReasonPhrase}");
        result = false;
      }

      return result;
    }

    async Task<bool> ProcessDocument(
      byte[] content, 
      string fileExtension, 
      string tempFilePath, 
      int retryTolerance, 
      bool retryForAllSources, 
      bool reformatDocument, 
      string[] sectionHeadings)
    {
      bool result;

      OriginalAttachmentDocument = content;
      OriginalFileExtension = fileExtension;
      //Supported formats are in the appconfig file so all non-PDF attachments
      //should be converted.
      if (!ValidFileExtension(fileExtension))
      {
        throw new IncorrectFileTypeException(
          $"File type expected '.RTF, .PDF, .DOC, .DOCX', " +
          $"but got {fileExtension}");
      }
      if (fileExtension.ToUpper() != "PDF")
      {
        Interop.Processor processor = new Interop.Processor();
        InteropResult interopResult = 
          await processor.ConvertToPdfInteropAsync(
          content, 
          fileExtension, 
          tempFilePath, 
          reformatDocument,
          sectionHeadings);
        content = interopResult.Data;
        
        if (interopResult.ExportError) 
        {
          _log.Debug("Error converting to PDF");
          ExportFailed = true;
          return false;
        }
        if (interopResult.WordError)
        {
          result = false;
          _log.Debug("Interop Error processing document");
          ExportFailed = false;
          InteropFailed = true;
          return result;
        }
      }

      result = true;
      OriginalUnprocessedPDF = (byte[])content.Clone();
      PopulateFormContentFromPdf(content);
      if (QuestionsDetectedInForm == true)
      {
        if (Source == SourceSystem.Unidentified && retryForAllSources)
        {
          _log.Debug("Questions detected. Will attempt to identify form.");
          foreach (SourceSystem system in Enum.GetValues(typeof(SourceSystem)))
          {
            if (system != SourceSystem.Unidentified)
            {
              _log.Debug($"Attempting sources {system}");
              content = (byte[])OriginalUnprocessedPDF.Clone();
              PopulateFormContentFromPdf(content, system, true);
              int validityCheck =
                GenerateReferralCreationObject("test", null, null)
                .NumberOfMissingEntries;
              if (validityCheck <= retryTolerance)
              {
                _log.Debug($"Using system {system}. {validityCheck} " +
                  $"questions unidentified.");

                break;
              }
            }
            Source = SourceSystem.Unidentified;
          }
        }
      }
      else
      {
        result = false;
        if (QuestionsDetectedInForm == null)
        {
          _log.Debug("Document would not parse correctly");
        }
        else
        {
          _log.Debug("Skipped interrogation of document as no questions were" +
            " detected.");
        }
      }
      return result;
    }

    /// <summary>
    /// Processes a PDF file from an array of bytes.
    /// </summary>
    /// <param name="pdfContent">The array of bytes representing
    /// the PDF document.</param>
    /// <returns>The number of questions and answers parsed.</returns>
    private int PopulateFormContentFromPdf(
      byte[] pdfContent,
      SourceSystem forceSourceSystem = SourceSystem.Unidentified,
      bool suppressWarnings = false)
    {
      if (_answerMap == null)
      {
        throw new InvalidOperationException("Answer Map not Loadded. " +
          "Could not process document.");
      }

      SupressWarnings = suppressWarnings;

      int count = 0;
      
      DocumentValues.Clear();
      DocumentContent.Clear();
      try
      {
        using (PdfDocument document = PdfDocument.Open(pdfContent))
        {
          Log.Debug($"Document has {document.NumberOfPages} pages.");
          PdfVersion = document.Version;
          Log.Debug($"PDF version: {document.Version}");
          
          GetSourceSystem(document);
          Log.Debug($"Document is for system {Source} " +
            $"Version {DocumentVersion}.");
          if (forceSourceSystem != SourceSystem.Unidentified)
          {
            Source = forceSourceSystem;
            DocumentVersion = 0;
            Log.Debug($"Attempting to read as {Source}.");
          }

          Parser parser = new Parser(_log, Config);

          bool twoColumnDocument = parser.HasTwoColumns(document.GetPages());
          QuestionsDetectedInForm = parser.QuestionsDetected;
          if (QuestionsDetectedInForm == true)
          {
            //DXS documents should always use the new custom parser
            if (Source == SourceSystem.DXS)
            {
              Log.Debug($"Using custom parser for DXS documents.");
              DocumentContent = parser.ProcessWords(document.GetPages());
              if (DocumentVersion < 2)
              {
                PerformTransformationsV1();
              }
              else
              {
                PerformTransformationsV2();
              }
            }
            else
            {
              //For compatibility, continue to use legacy parser for V1 documents
              if (DocumentVersion < 2)
              {
                Log.Debug($"Using legacy parser for V1 Documents.");
                ParseDocumentPdfPigNative(document);
                PerformTransformationsV1();
              }
              else
              {
                Log.Debug($"Using custom parser for V2+ documents.");
                DocumentContent = parser.ProcessWords(document.GetPages());
                PerformTransformationsV2();
              }
            }
            int lineNumber = 0;
            bool documentComplete = false;
            do
            {
              documentComplete = GetNextQuestionAnswer(
                DocumentContent,
                ref lineNumber);
              count++;
            } while (documentComplete == false);
          }
          else
          {
            Log.Debug($"Custom document parser did not detect questions.");
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex, "Error processing attachment");
      }
      Downloaded = true;
      return count;
    }

    void ParseDocumentPdfPigNative(PdfDocument document)
    {
      for (int pageNumber = 0; pageNumber < document.NumberOfPages;
          pageNumber++)
      {
        var page = document.GetPage(pageNumber + 1);
        var words = page.GetWords();
        UnsupervisedReadingOrderDetector rod =
          new UnsupervisedReadingOrderDetector(5,
          UnsupervisedReadingOrderDetector.SpatialReasoningRules.ColumnWise,
          true);
        var gotBlocks =
          DocstrumBoundingBoxes.Instance.GetBlocks(words);
        var blocks =
          rod.Get(gotBlocks);

        // Each block contains multiple lines seperated by carriage returns 
        // and/or newline characters.  These need to be split into  
        // individual entries.
        List<string> pageContent = new List<string>();
        foreach (var block in blocks)
        {
          string blockText = block.Text;

          pageContent.AddRange(
            blockText.Split(new string[] { "\n", "\r\n" },
            StringSplitOptions.RemoveEmptyEntries).ToList());
        }
        DocumentContent.AddRange(pageContent);
      }
    }

    private bool GetNextQuestionAnswer(
      List<string> pageContent,
      ref int startFromLine)
    {
      bool complete = false;
      int currentLine = startFromLine;
      //Get question - these will always end with ':'
      //Apply mapping to line
      string question;
      do
      {
        //For empty pages, or where you are already after the last line
        if (currentLine >= pageContent.Count()) return true;
        question = _answerMap.MappedItem(pageContent[currentLine].Trim());
        currentLine++;
      } while (question != "#IGNORE" && !question.EndsWith(":"));

      if (currentLine >= pageContent.Count()) return true;
      //Get answer
      string answerText = "";
      while (!_answerMap
        .MappedItem(pageContent[currentLine].Trim()).EndsWith(":"))
      {
        string nextText = _answerMap
          .MappedItem(pageContent[currentLine].Trim());
        if (nextText != "#IGNORE")
        {
          answerText = $"{answerText} {nextText}";
        }
        currentLine++;
        if (currentLine >= pageContent.Count()) break;
      };
      answerText = _answerMap.MappedItem(answerText);
      question = question.ToUpper();
      //Check for duplicate question and append a number to the end if it exists

      if (DocumentValues.ContainsKey(question))
      {
        string newQuestion = question.Substring(0, question.Length - 1);
        int suffix = 1;
        while (DocumentValues.ContainsKey($"{newQuestion}{suffix}:")) suffix++;
        question = $"{newQuestion}{suffix}:";
      }
      DocumentValues.Add(question, answerText);

      startFromLine = currentLine;

      //Return true if the end of the page has been reached
      if (currentLine >= pageContent.Count()) complete = true;

      return complete;
    }

    private void GetSourceSystem(PdfDocument document)
    {
      Source = SourceSystem.Unidentified;
      DocumentVersion = 0;
      try
      {
        for (int pageNumber = 0; pageNumber < document.NumberOfPages;
          pageNumber++)
        {
          var page = document.GetPage(pageNumber + 1);
          var words = page.GetWords();
          var blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
          foreach (var block in blocks)
          {
            // Remove spaces in the block text as some templates have a
            // space e.g. System: EMIS
            string blockText = block.Text.Replace(" ", "");
            if (blockText.Contains("System:"))
            {
              if (blockText.Contains("System:EMIS",
                StringComparison.InvariantCultureIgnoreCase))
              {
                Source = SourceSystem.Emis;
              }
              if (blockText.Contains("System:S1",
                StringComparison.InvariantCultureIgnoreCase))
              {
                Source = SourceSystem.SystemOne;
              }
              if (blockText.Contains("System:VISION",
                StringComparison.InvariantCultureIgnoreCase))
              {
                Source = SourceSystem.Vision;
              }
              if (blockText.Contains("System:DXS",
                StringComparison.InvariantCultureIgnoreCase))
              {
                Source = SourceSystem.DXS;
              }

              if (Source != SourceSystem.Unidentified)
              {
                //Get the version number from the source
                if (blockText.Contains("V"))
                {
                  string[] split = blockText.Split("V");
                  decimal version;
                  decimal.TryParse(split[split.Length-1], out version);
                  DocumentVersion = version;
                }
                return; //Source has been found
              }
            }          
          }
        }
      }
      catch
      {
        Source = SourceSystem.Unidentified;
      }
    }

    public bool DocumentHasValueFor(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        throw new ArgumentNullException("A key must be provided.");
      }
      string answer = "";
      bool result = false;
      result = DocumentValues.TryGetValue(key, out answer);

      return result;
    }

    public string DocumentValue(
      string key,
      int maxLength,
      bool alwaysSupressWarnings = false)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        throw new ArgumentNullException("A key must be provided.");
      }
      string result;
      key = key.ToUpper();
      bool questionExists = DocumentValues.TryGetValue(key, out result);

      if (questionExists == false)
      {
        result = "";
        if (alwaysSupressWarnings)
        {
          _log.Verbose($"Ignored question {key}");
        }
        else
        {
          string logEntry = $"UBRN {Ubrn}: Question '{key}' is either missing " +
            $"from the form for system '{Source} V{DocumentVersion}' or " +
            $"exists with a different heading and needs to be mapped.";
          if (SupressWarnings)
          {
            _log.Debug(logEntry);
          }
          else
          {
            _log.Warning(logEntry);
          }
          AppendToParsingReport(logEntry);
        }
      }
      else
      {
        result = result.Trim();
        if (result.Length > maxLength)
        {
          _log.Debug($"Truncated {key} field from {result.Length} to " +
            $"the maximum length of {maxLength}");
          result = result.Substring(0, maxLength);
        }
      }

      return result;
    }

    /// <summary>
    /// Reads a document value without logging an error
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string DocumentValueQuiet(string key)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        throw new ArgumentNullException("A key must be provided.");
      }
      string result;
      key = key.ToUpper();
      bool questionExists = DocumentValues.TryGetValue(key, out result);

      if (questionExists == false)
      {
        result = "";
      }
      result = result.Trim();
      return result;
    }

    public virtual ReferralPut GenerateReferralUpdateObject(
      string ubrn,
      long? attachmentId, 
      long? mostRecentAttachmentId)
    {
      if (Downloaded == false)
      {
        throw new InvalidDataException($"An update record for UBRN {ubrn} " +
          $"could not be created as there was no attachment downloaded.");
      }
      ObjectParsingReport = "";
      ReferralPut result = new ReferralPut()
      {
        Address1 = DocumentValue("Address 1:", 200, true),
        Address2 = DocumentValue("Address 2:", 200, true),
        Ethnicity = DocumentValue("Ethnicity:", 200),
        FamilyName = DocumentValue("Surname:", 200),
        Sex = DocumentValue("Gender:", 200),
        GivenName = DocumentValue("Forename:", 200),
        NhsNumber = ConvertToNumbersOnly(DocumentValue("NHS No:", 200)),
        Postcode = DocumentValue("Postcode:", 200),
        ReferringGpPracticeName = DocumentValue("Practice Name:", 500),
        ReferringGpPracticeNumber = DocumentValue("Practice Code:", 200),
        Address3 = DocumentValue("Address 3:", 200, true),
        VulnerableDescription = DocumentValue("Vulnerable Reason:", 1000, true),
        Mobile = ConvertToTelephoneNumber(DocumentValue("Mobile Tel No:", 200)),
        Telephone = ConvertToTelephoneNumber(DocumentValue("Home Tel No:", 200)),
        ReferralAttachmentId = attachmentId,
        MostRecentAttachmentId = mostRecentAttachmentId,
        DocumentVersion = ConvertToDecimal(DocumentValue("DocumentVersion:", 10)),
        SourceSystem = Source,
        ServiceId = ServiceId
      };

      string bmi = DocumentValue("Value of last BMI:", 200);
      decimal? bmiResult = ConvertToBMI("Value of last BMI:", bmi);
      result.CalculatedBmiAtRegistration = bmiResult;

      string dateOfBmi = DocumentValue("Date of last BMI:", 200);
      DateTimeOffset? dateOfBmiResult =
        ConvertToDateTimeOffset("Date of last BMI:", dateOfBmi);
      result.DateOfBmiAtRegistration = dateOfBmiResult;

      string dateOfBirth = DocumentValue("Date of Birth:", 200);
      DateTimeOffset? dateOfBirthResult =
        ConvertToDateTimeOffset("Date of Birth:", dateOfBirth);
      result.DateOfBirth = dateOfBirthResult;

      string dateOfReferral = DocumentValue("Referral Date:", 200);
      DateTimeOffset? dateOfReferralResult =
        ConvertToDateTimeOffset("Referral Date:", dateOfReferral);
      result.DateOfReferral = dateOfReferralResult;

      string hasDiabetesType1 = DocumentValue("Diabetes Type 1:", 200);
      bool? hasDiabetesType1Result =
        ConvertDiabetesAnswerToBool("Diabetes Type 1:", hasDiabetesType1);
      result.HasDiabetesType1 = hasDiabetesType1Result;

      string hasDiabetesType2 = DocumentValue("Diabetes Type 2:", 200);
      bool? hasDiabetesType2Result =
        ConvertDiabetesAnswerToBool("Diabetes Type 2:", hasDiabetesType2);
      result.HasDiabetesType2 = hasDiabetesType2Result;

      string hasPhysicalDisability = DocumentValue("Physical Disability:", 200);
      bool? hasDisabilityResult =
        ConvertToBool("Physical Disability:", hasPhysicalDisability);
      result.HasAPhysicalDisability = hasDisabilityResult;

      string hasLearningDisability = DocumentValue("Learning Disability:", 200);
      bool? hasLearningDisabilityResult =
        ConvertToBool("Learning Disability:", hasLearningDisability);
      result.HasALearningDisability = hasLearningDisabilityResult;


      string hasHypertension = DocumentValue("Hypertension:", 200);
      bool? hasHypertensionResult =
        ConvertHypertensionAnswerToBool("Hypertension:", hasHypertension);
      result.HasHypertension = hasHypertensionResult;

      string hasSMI = DocumentValue("Severe Mental Illness:", 200);
      bool? hasSMIResult =
        ConvertToBool("Severe Mental Illness:", hasSMI);
      result.HasRegisteredSeriousMentalIllness = hasSMIResult;

      string isVulnerable =
        DocumentValue("Identified as a Vulnerable Adult:", 200);
      bool? isVulnerableResult =
        ConvertToBool("Identified as a Vulnerable Adult:", isVulnerable);
      result.IsVulnerable = isVulnerableResult;

      string weightKg = DocumentValue("Weight:", 200);
      decimal? weightKgResult =
        ConvertToWeightinKg("Weight:", weightKg);
      result.WeightKg = weightKgResult;

      string heightCm = DocumentValue("Height:", 200);
      decimal? heightCmResult =
        ConvertToHeightinCm("Height:", heightCm);
      result.HeightCm = heightCmResult;

      PerformReferralTransformations(result);

      result.PdfParseLog = ObjectParsingReport;

      return result;
    }

    public ReferralPost CreateplaceholderReferralCreationObject(string ubrn)
    {
      ReferralPost result = new ReferralPost
      {
        Ubrn = ubrn,
        GivenName = "Placeholder",
        FamilyName = "NoAttachment",
      };
      return result;
    }

    public virtual ReferralPost GenerateReferralCreationObject(
      string ubrn,
      long? attachmentId, 
      long? mostRecentAttachmentId)
    {
      ReferralPost result;
      if (Downloaded == false)
      {
        result = CreateplaceholderReferralCreationObject(ubrn);
        result.MostRecentAttachmentId = mostRecentAttachmentId;
        _log.Debug($"A Placeholder Referral was created for UBRN {ubrn}");
      }
      else
      {
        ObjectParsingReport = "";
        result = new ReferralPost
        {
          Ubrn = ubrn,
          Address1 = DocumentValue("Address 1:", 200, true),
          Address2 = DocumentValue("Address 2:", 200, true),
          //This is currently incorrect on the PDF form
          Ethnicity = DocumentValue("Ethnicity:", 200),
          FamilyName = DocumentValue("Surname:", 200),
          Sex = DocumentValue("Gender:", 200),
          GivenName = DocumentValue("Forename:", 200),
          NhsNumber = ConvertToNumbersOnly(DocumentValue("NHS No:", 200)),
          Postcode = DocumentValue("Postcode:", 200),
          ReferringGpPracticeName = DocumentValue("Practice Name:", 500),
          ReferringGpPracticeNumber = DocumentValue("Practice Code:", 200),
          Address3 = DocumentValue("Address 3:", 200, true),
          VulnerableDescription = DocumentValue("Vulnerable Reason:", 200, true),
          Mobile =
            ConvertToTelephoneNumber(DocumentValue("Mobile Tel No:", 200)),
          Telephone =
            ConvertToTelephoneNumber(DocumentValue("Home Tel No:", 200)),
          Email = DocumentValue("Email:", 200),
          ReferralAttachmentId = attachmentId,
          MostRecentAttachmentId = mostRecentAttachmentId,
          DocumentVersion = 
            ConvertToDecimal(DocumentValue("DocumentVersion:", 10)),
          SourceSystem = Source,
          ServiceId = ServiceId
        };

        string bmi = DocumentValue("Value of last BMI:", 200);
        decimal? bmiResult = ConvertToBMI("Value of last BMI:", bmi);
        result.CalculatedBmiAtRegistration = bmiResult;

        string dateOfBmi = DocumentValue("Date of last BMI:", 200);
        DateTimeOffset? dateOfBmiResult =
          ConvertToDateTimeOffset("Date of last BMI:", dateOfBmi);
        result.DateOfBmiAtRegistration = dateOfBmiResult;

        string dateOfBirth = DocumentValue("Date of Birth:", 200);
        DateTimeOffset? dateOfBirthResult =
          ConvertToDateTimeOffset("Date of Birth:", dateOfBirth);
        result.DateOfBirth = dateOfBirthResult;

        string dateOfReferral = DocumentValue("Referral Date:", 200);
        DateTimeOffset? dateOfReferralResult =
          ConvertToDateTimeOffset("Referral Date:", dateOfReferral);
        result.DateOfReferral = dateOfReferralResult;

        string hasDiabetesType1 = DocumentValue("Diabetes Type 1:", 200);
        bool? hasDiabetesType1Result =
          ConvertDiabetesAnswerToBool("Diabetes Type 1:", hasDiabetesType1);
        result.HasDiabetesType1 = hasDiabetesType1Result;

        string hasDiabetesType2 = DocumentValue("Diabetes Type 2:", 200);
        bool? hasDiabetesType2Result =
          ConvertDiabetesAnswerToBool("Diabetes Type 2:", hasDiabetesType2);
        result.HasDiabetesType2 = hasDiabetesType2Result;

        string hasPhysicalDisability =
          DocumentValue("Physical Disability:", 200);
        bool? hasDisabilityResult =
          ConvertToBool("Physical Disability:", hasPhysicalDisability);
        result.HasAPhysicalDisability = hasDisabilityResult;

        string hasLearningDisability =
          DocumentValue("Learning Disability:", 200);
        bool? hasLearningDisabilityResult =
          ConvertToBool("Learning Disability:", hasLearningDisability);
        result.HasALearningDisability = hasLearningDisabilityResult;


        string hasHypertension = DocumentValue("Hypertension:", 200);
        bool? hasHypertensionResult =
          ConvertHypertensionAnswerToBool("Hypertension:", hasHypertension);
        result.HasHypertension = hasHypertensionResult;

        string hasSMI = DocumentValue("Severe Mental Illness:", 200);
        bool? hasSMIResult =
          ConvertToBool("Severe Mental Illness:", hasSMI);
        result.HasRegisteredSeriousMentalIllness = hasSMIResult;

        string isVulnerable =
          DocumentValue("Identified as a Vulnerable Adult:", 200);
        bool? isVulnerableResult =
          ConvertToBool("Identified as a Vulnerable Adult:", isVulnerable);
        result.IsVulnerable = isVulnerableResult;

        string weightKg = DocumentValue("Weight:", 200);
        decimal? weightKgResult =
          ConvertToWeightinKg("Weight:", weightKg);
        result.WeightKg = weightKgResult;

        string heightCm = DocumentValue("Height:", 200);
        decimal? heightCmResult =
          ConvertToHeightinCm("Height:", heightCm);
        result.HeightCm = heightCmResult;
        result.PdfParseLog = ObjectParsingReport;
      }
      PerformReferralTransformations(result);
      return result;
    }

    public decimal? ConvertToBMI(
      string question, 
      string answer)
    {
      decimal? result = null;
      if (string.IsNullOrWhiteSpace(answer))
      {
        LogParsingError("ConvertToBMI", question, answer);
        return null;
      }
      answer = answer.Trim().ToUpper();
      answer = answer.Trim().Replace(":", " ");
      answer = answer.Replace("=", "");
      answer = answer.Replace("≈", "");

      try
      {
        string[] splitAnswer = answer.Split(' ');
        if (splitAnswer.Length == 1)
        {
          answer = TruncateFrom(answer, "KG");
          result = Convert.ToDecimal(answer);
        }
        else
        {
          decimal foundBmi = 0;
          bool success = false;
          //look for 'kg'
          for (int i = 0; i < splitAnswer.Length; i++)
          {
            //looks for '23.0 kg/m2'
            if (splitAnswer[i].StartsWith("KG") && i > 0)
            {
              success = decimal.TryParse(splitAnswer[i - 1], out foundBmi);
              if (success == true)
              {
                break;
              }
            }

            //look for '23.0kg/m2'
            if (splitAnswer[i].Contains("KG") == true)
            {
              splitAnswer[i] = TruncateFrom(splitAnswer[i], "KG");
              success = decimal.TryParse(splitAnswer[i], out foundBmi);
              if (success == true)
              {
                break;
              }
            }
          }
          if (success == true)
          {
            _log?.Debug($"Converted BMI Value {answer} to '{foundBmi}'.");
            return foundBmi;
          }
          else
          {
            //Look for the first decimal in the answer and return it

            for (int i = 0; i < splitAnswer.Length; i++)
            {
              success = decimal.TryParse(splitAnswer[i], out foundBmi);
              if (success == true)
              {
                _log?.Debug($"Converted BMI Value {answer} to " +
                  $"'{foundBmi}'.");
                return foundBmi;
              }
            }
          }
          LogParsingError("ConvertToBMI", question, answer);
        }

      }
      catch (Exception ex)
      {
        LogParsingError("ConvertToBMI", question, answer, ex);
        return null;
      }
      _log?.Debug($"Converted BMI Value {answer} to " +
        $"'{result}'.");
      return result;
    }

    /// <summary>
    /// Returns ASCII version of string containing unicode characters
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private string UnicodeToAscii(string input)
    {
      string result =
        input.Replace('‐', '-') //U+2010
        .Replace('‑', '-')      //U+2011
        .Replace('‒', '-')      //U+2012
        .Replace('–', '-')      //U+2013
        .Replace('—', '-')      //U+2014
        .Replace('⁄', '/');     //U+2044

      return result;
    }

    /// <summary>
    /// Take the first date from an answer and return it as a datetime
    /// </summary>
    /// <param name="question"></param>
    /// <param name="answer"></param>
    /// <returns></returns>
    public DateTimeOffset? ConvertToDateTimeOffset(
      string question,
      string answer)
    {
      DateTimeOffset? result = null;
      if (string.IsNullOrWhiteSpace(answer)) return null;
      try
      {
        DateTime dateTime;
        string[] months = { "jan", "feb", "mar", "apr", "may", "jun",
            "jul", "aug", "sep", "oct", "nov", "dec"};

        answer = UnicodeToAscii(answer);

        //To allow for dates in the format like '23rd September 2021'
        //we need to look for these specifically and try to parse them first
        string[] splitValue = answer.Split(' ');

        //First fix malformed dates such as "1stjan", "9thsept" etc
        for (int i = 0; i < splitValue.Length; i++)
        {
          splitValue[i] = MalformedDateFix(splitValue[i]);
        }

        //Look for dates in the format of 'YYYYMMDD' and transform into
        //'YYYY-MM-DD'
        for (int i = 0; i < splitValue.Length; i++)
        {
          if (splitValue[i].Length == 8)
          {
            if (int.TryParse(splitValue[i], out int dateAsNumber) == true)
            {
              bool foundDateFormat = false;
              string checkDate
                = $"{splitValue[i].Substring(0, 4)}-" +
                $"{splitValue[i].Substring(4, 2)}-" +
                $"{splitValue[i].Substring(6, 2)}";
              if (DateTime.TryParse(checkDate, out DateTime date) == true)
              {
                if (date <= DateTime.Today && date >= new DateTime(1900, 1, 1))
                {
                  splitValue[i] = checkDate;
                  foundDateFormat = true;
                }
              }
              //'DD-MM-YYYY
              if (!foundDateFormat)
              {
                checkDate
                  = $"{splitValue[i].Substring(4, 4)}-" +
                  $"{splitValue[i].Substring(2, 2)}-" +
                  $"{splitValue[i].Substring(0, 2)}";
                if (DateTime.TryParse(checkDate, out date) == true)
                {
                  if (date <= DateTime.Today && date >= new DateTime(1900, 1, 1))
                  {
                    splitValue[i] = checkDate;
                  }
                }
              }
            }
          }
        }

        if (splitValue.Length >= 3)
        {
          //Account for superscript text within a date - this will cause
          //that text to appear at the start of the line since it appears above
          //the rest of the answer
          if (splitValue[0].Length == 2 &&
            PositionOfNumberEnd(splitValue[0]) == 0)
          {
            //Look for a month name part
            bool foundMonth = false;
            for (int i = 2; i < splitValue.Length - 1; i++)
            {
              for (int j = 0; j < months.Length; j++)
              {
                if (splitValue[i].StartsWith(months[j],
                  StringComparison.InvariantCultureIgnoreCase))
                {
                  int day = 0;
                  int year = 0;
                  splitValue[i + 1] =
                      NormaliseTwoDigitYear(splitValue[i + 1]);
                  if (int.TryParse(splitValue[i - 1], out day) == true &&
                      int.TryParse(splitValue[i + 1], out year) == true)
                  {
                    if (day > 0 && day <= 31 && year > 1900)
                    {
                      splitValue[i - 1] = string.Concat(splitValue[i - 1], " ",
                        splitValue[i], " ", splitValue[i + 1]);
                      splitValue[i] = "";
                      splitValue[i + 1] = "";
                      foundMonth = true;
                      break;
                    }
                  }
                  //Also look for dates where the month is presented before
                  //the day
                  if (i < splitValue.Length - 2)
                  {
                    splitValue[i + 2] =
                      NormaliseTwoDigitYear(splitValue[i + 2]);
                    if (int.TryParse(splitValue[i + 1], out day) == true &&
                        int.TryParse(splitValue[i + 2], out year) == true)
                    {
                      if (day > 0 && day <= 31 && year > 1900)
                      {
                        splitValue[i] = string.Concat(splitValue[i], " ",
                          splitValue[i + 1], " ", splitValue[i + 2]);
                        splitValue[i + 1] = "";
                        splitValue[i + 2] = "";
                        foundMonth = true;
                        break;
                      }
                    }
                  }
                };
              }
              if (foundMonth == true)
              {
                break;
              }
            }
          }
          else
          {
            //Look for non-superscript number decoration such as "1st" "2nd" etc
            for (int i = 0; i < splitValue.Length - 1; i++)
            {
              int day = 0;
              int year = 0;

              if (
                splitValue[i].Length > 2 &&
                PositionOfNumberEnd(splitValue[i]) == splitValue[i].Length - 2)
              {
                for (int j = 0; j < months.Length; j++)
                {
                  if (splitValue[i + 1].StartsWith(months[j],
                    StringComparison.InvariantCultureIgnoreCase))
                  {
                    splitValue[i + 2] =
                      NormaliseTwoDigitYear(splitValue[i + 2]);
                    if (int.TryParse(splitValue[i][0..^2], out day) == true &&
                      int.TryParse(splitValue[i + 2], out year) == true)
                    {
                      if (day > 0 && day <= 31 && year > 1900)
                      {
                        splitValue[i] = string.Concat(splitValue[i][0..^2], " ",
                          splitValue[i + 1], " ", splitValue[i + 2]);
                        splitValue[i + 1] = "";
                        splitValue[i + 2] = "";
                        break;
                      }
                    }
                  }
                  if (i > 0 && splitValue[i - 1].StartsWith(months[j],
                    StringComparison.InvariantCultureIgnoreCase))
                  {
                    splitValue[i + 1] =
                      NormaliseTwoDigitYear(splitValue[i + 1]);
                    if (int.TryParse(splitValue[i][0..^2], out day) == true &&
                       int.TryParse(splitValue[i + 1], out year) == true)
                    {
                      if (day > 0 && day <= 31 && year > 1900)
                        splitValue[i] = string.Concat(splitValue[i - 1], " ",
                      splitValue[i][0..^2], " ", splitValue[i + 1]);
                      splitValue[i - 1] = "";
                      splitValue[i + 1] = "";
                      break;
                    }
                  }
                }
                break;
              }
              //Support format like '3 March 2001'
              else if (int.TryParse(splitValue[i], out day) == true)
              {
                if (i <= splitValue.Length - 2)
                {
                  splitValue[i + 2] =
                      NormaliseTwoDigitYear(splitValue[i + 2]);
                  if (int.TryParse(splitValue[i + 2], out year) == true)
                  {
                    if (day > 0 && day <= 31 && year > 1900)
                      splitValue[i] = string.Concat(splitValue[i], " ",
                    splitValue[i + 1], " ", splitValue[i + 2]);
                    splitValue[i + 1] = "";
                    splitValue[i + 2] = "";
                  }
                }
              }
            }
          }
        }
        bool foundDate = false;
        for (int i = 0; i < splitValue.Length; i++)
        {
          if (string.IsNullOrWhiteSpace(splitValue[i]))
          {
            foundDate = false;
          }
          else
          {
            if (i < splitValue.Length - 1)
            {
              if (DateTime.TryParse(
                $"{splitValue[i]} {splitValue[i+1]}", out dateTime) == true)
              {
                result = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                foundDate = true;
                break;
              }
            }
            if (DateTime.TryParse(splitValue[i], out dateTime) == true)
            {
              result = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
              foundDate = true;
            }
          }
          if (foundDate == true)
          {
            break;
          }
        }
      }
      catch (Exception ex)
      {
        LogParsingError("ConvertToDateTimeOffset", question, answer, ex);
        return null;
      }
      if (result == null)
      {
        LogParsingError("ConvertToDateTimeOffset", question, answer);
      }
      else
      {
        _log.Debug($"Parsed date {answer} as {result:dd/MM/yyyy}.");
      }

      return result;
    }

    private string NormaliseTwoDigitYear(string yearAsTwoDigitText)
    {
      string result = yearAsTwoDigitText;
      int year;

      if (int.TryParse(yearAsTwoDigitText, out year) == true)
      {
        //Handle two digit years
        if (year < 100)
        {
          if (year + 2000 <= DateTime.Now.Year)
          {
            year += 2000;
          }
          else
          {
            year += 1900;
          }
          result = $"{year}";
        }
      }
      return result;
    }

    //Put spaces into dates with number endings and month names where
    //they are missing
    private string MalformedDateFix(string inputValue)
    {
      string result = inputValue;

      //Malformed month name
      result = result.Replace("sept ", "sep ",
              StringComparison.InvariantCultureIgnoreCase);
      result = result.Replace("sept-", "sep",
        StringComparison.InvariantCultureIgnoreCase);
      result = result.Replace(",", " ",
        StringComparison.CurrentCultureIgnoreCase);

      //Numbers with letters and no space before dates
      int numberEndPosition = PositionOfNumberEnd(inputValue);
      if (numberEndPosition >= 0)
      {
        int monthPosition = PositionOfDatePart(inputValue);
        if (monthPosition == numberEndPosition + 2)
        {
          result = inputValue.Substring(0, numberEndPosition) + " " +
            inputValue.Substring(numberEndPosition + 2);
        }
      }

      return result;
    }

    /// <summary>
    /// Find number endings like 'st', 'nd' etc within a string
    /// </summary>
    /// <param name="numberAsString"></param>
    /// <returns>Index of occurrence, or -1 if not found</returns>
    private int PositionOfNumberEnd(string numberAsString)
    {
      string[] numberEnds = { "st", "nd", "rd", "th" };
      int result = -1;

      for (int i = 0; i < numberEnds.Length; i++)
      {
        if (numberAsString.Contains(numberEnds[i],
          StringComparison.InvariantCultureIgnoreCase))
        {
          result = numberAsString.IndexOf(numberEnds[i],
            StringComparison.InvariantCultureIgnoreCase);
          break;
        }
      }

      return result;
    }

    /// <summary>
    /// Find month part like 'jan', 'feb' etc within a string
    /// </summary>
    /// <param name="valueToCheck"></param>
    /// <returns>Index of occurrence, or -1 if not found</returns>
    private int PositionOfDatePart(string valueToCheck)
    {
      string[] months = { "jan", "feb", "mar", "apr", "may", "jun",
            "jul", "aug", "sep", "oct", "nov", "dec"};
      int result = -1;

      for (int i = 0; i < months.Length; i++)
      {
        if (valueToCheck.Contains(months[i],
          StringComparison.InvariantCultureIgnoreCase))
        {
          result = valueToCheck.IndexOf(months[i],
            StringComparison.InvariantCultureIgnoreCase);
          break;
        }
      }

      return result;
    }

    public bool? ConvertToBool(
      string question,
      string answer)
    {
      bool? result;
      if (string.IsNullOrWhiteSpace(answer)) return null;
      try
      {
        result = Convert.ToBoolean(answer);
      }
      catch (Exception ex)
      {
        LogParsingError("ConvertToBool", question, answer, ex);
        return null;
      }
      return result;
    }

    //looks for a hypertension related keyword in the answer and
    //automatically sets the answer to true or false if it finds one
    public bool? ConvertHypertensionAnswerToBool(
      string question, 
      string answer)
    {
      //Should also pick up 'Hypertensive' and 'Hypertention' misspellings
      if (answer.Contains("HYPERTEN",
        StringComparison.InvariantCultureIgnoreCase) ||
        answer.Contains("YES", StringComparison.InvariantCultureIgnoreCase) ||
        answer
        .Contains("DIAGNOSED", StringComparison.InvariantCultureIgnoreCase)
        )
      {
        answer = "true";
      }
      else if (answer.Contains("NO EVENT FOUND",
        StringComparison.InvariantCultureIgnoreCase) ||
       answer.Contains("NO EVENTS FOUND",
        StringComparison.InvariantCultureIgnoreCase) ||
        string.IsNullOrWhiteSpace(answer))
      {
        answer = "false";
      }

      return ConvertToBool(question, answer);
    }

    //Looks for a diabetes related keyword in the answer and automatically sets
    //the answer to true or false if it find one
    public bool? ConvertDiabetesAnswerToBool(
      string question, 
      string answer)
    {
      if (answer.Contains("NO EVENT FOUND",
         StringComparison.InvariantCultureIgnoreCase) ||
        answer.Contains("NO EVENTS FOUND",
         StringComparison.InvariantCultureIgnoreCase) ||
        string.IsNullOrWhiteSpace(answer))
      {
        answer = "false";
      }
      else if (answer.Contains("MELLITUS",
          StringComparison.InvariantCultureIgnoreCase) ||
         answer.Contains("YES",
         StringComparison.InvariantCultureIgnoreCase) ||
         answer.Contains("TYPE 1",
          StringComparison.InvariantCultureIgnoreCase) ||
         answer.Contains("TYPE 2",
          StringComparison.InvariantCultureIgnoreCase) ||
         answer.Contains("TYPE I", //Type II is covered by this
          StringComparison.InvariantCultureIgnoreCase) ||
         answer.Contains("DIAGNOSED",
          StringComparison.InvariantCultureIgnoreCase))
      {
        answer = "true";
      }
      return ConvertToBool(question, answer);
    }

    public string StripDateFromStart(string answer)
    {
      string result = answer.Trim();
      DateTime containedDate;
      string[] words = answer.Split(' ');
      if (words.Length > 1)
      {
        if (DateTime.TryParse(words[0], out containedDate) == true)
        {
          result = answer.Substring(words[0].Length + 1);
        }
      }
      return result;
    }

    public decimal? ConvertToWeightinKg(
      string question, 
      string answer)
    {
      decimal? result = null;
      if (string.IsNullOrWhiteSpace(answer))
      {
        LogParsingError("ConvertToWeightinKg", question, answer);
        return null;
      }
      answer = answer.Trim().Replace(":", " ");
      answer = answer.Replace("=", "");
      answer = answer.Replace("≈", "");
      try
      {
        string[] splitAnswer = answer.Split(' ');
        if (splitAnswer.Length == 1)
        {
          answer = TruncateFrom(answer, "KG");
          result = Convert.ToDecimal(answer);
        }
        else
        {
          decimal foundWeight = 0;
          bool success = false;
          //look for 'kg'
          for (int i = 0; i < splitAnswer.Length; i++)
          {
            //looks for '23.0 kg/m2'
            if (splitAnswer[i].
              StartsWith("KG", StringComparison.InvariantCultureIgnoreCase)
              && i > 0)
            {
              success = decimal.TryParse(splitAnswer[i - 1], out foundWeight);
              if (success == true)
              {
                break;
              }
            }

            //look for '23.0kgxxxxx'
            if (splitAnswer[i]
              .Contains("KG", StringComparison.InvariantCultureIgnoreCase))
            {
              splitAnswer[i] = TruncateFrom(splitAnswer[i], "KG");
              success = decimal.TryParse(splitAnswer[i], out foundWeight);
              if (success == true)
              {
                break;
              }
            }
          }
          if (success == true)
          {
            _log?.Debug($"Converted Weight Value {answer} to " +
              $"'{foundWeight}'.");
            return foundWeight;
          }
          else
          {
            //Look for the first decimal in the answer and return it

            for (int i = 0; i < splitAnswer.Length; i++)
            {
              success = decimal.TryParse(splitAnswer[i], out foundWeight);
              if (success == true)
              {
                _log?.Debug($"Converted Weight Value {answer} to " +
                  $"'{foundWeight}'.");
                return foundWeight;
              }
            }
          }
          LogParsingError("ConvertToWeightinKg", question, answer);
        }

      }
      catch (Exception ex)
      {
        LogParsingError("ConvertToWeightinKg", question, answer, ex);
        return null;
      }
      _log?.Debug($"Converted Weight Value {answer} to " +
        $"'{result}'.");
      return result;
    }

    public decimal? ConvertToHeightinCm(
      string question, 
      string answer)
    {
      decimal? result = null;
      if (string.IsNullOrWhiteSpace(answer)) return null;
      try
      {
        answer = Regex.Replace(answer, "[^0-9.]", "");
        result = Convert.ToDecimal(answer);
        if (result < 3) result *= 100;
        if (result < Int32.MinValue || result > Int32.MaxValue)
        {
          result = null;
        }
      }
      catch (Exception ex)
      {
        LogParsingError("ConvertToHeightinCm", question, answer, ex);
        return null;
      }
      return result;
    }

    public string ConvertToNumbersOnly(string value)
    {
      return Regex.Replace(value, "[^0-9]", "");
    }

    public decimal ConvertToDecimal(string value)
    {
      string numbers = Regex.Replace(value, "[^0-9.]", "");

      decimal result;
      if (decimal.TryParse(numbers, out result))
      {
        _log.Debug($"Converted number '{value}' to {result}");
      }
      else
      {
        _log.Debug($"Failed to convert number '{value}'");
        result = 0;
      }
      return result;
    }

    public string ConvertToTelephoneNumber(string value)
    {
      value = value.Trim();
      string telephoneNumber = ConvertToNumbersOnly(value);
      if (value.StartsWith("+44"))
      {
        telephoneNumber = $"+{telephoneNumber[..]}";
      }
      else if (telephoneNumber.StartsWith("0"))
      {
        telephoneNumber = $"+44{telephoneNumber[1..]}";
      }
      return telephoneNumber;
    }

    private void LogParsingError(
      string field, 
      string question, 
      string answer,
      Exception ex = null)
    {
      string logEntry
        = $"{field}: Cannot map question '{question}' answer '{answer}'";
      _log?.Debug(logEntry);
      AppendToParsingReport(logEntry);
    }

    private string TruncateFrom(
      string original, 
      string from)
    {
      string result = original;
      int suffixposition =
        original.IndexOf(from, StringComparison.InvariantCultureIgnoreCase);
      if (suffixposition > 0)
      {
        result = original.Substring(0, suffixposition);
      }
      return result;
    }


    /// <summary>
    /// Produce diagnostic information showing the results of the process.
    /// </summary>
    public void ShowDiagnostics(
      string identifier, 
      long? attachmentId)
    {
      ReferralPost referralCreate =
        GenerateReferralCreationObject(identifier, attachmentId, null);
      //Show the resulting createReferral object content
      Console.WriteLine("****************************");
      Console.WriteLine("ReferralPost (Create) Object");
      Console.WriteLine($"{referralCreate.Ubrn}");
      Console.WriteLine($"For System : {Source}");
      Console.WriteLine("****************************");
      Console.WriteLine($"Address1|{referralCreate.Address1}");
      Console.WriteLine($"Address2|{referralCreate.Address2}");
      Console.WriteLine($"Address3|{referralCreate.Address3}");
      Console.WriteLine($"CalculatedBmiAtRegistration|" +
        $"{referralCreate.CalculatedBmiAtRegistration}");
      Console.WriteLine($"DateOfBirth|{referralCreate.DateOfBirth}");
      Console.WriteLine($"DateOfBmiAtRegistration|" +
        $"{referralCreate.DateOfBmiAtRegistration}");
      Console.WriteLine($"DateOfReferral|{referralCreate.DateOfReferral}");
      Console.WriteLine($"DocumentVersion|{referralCreate.DocumentVersion}");
      Console.WriteLine($"Email|{referralCreate.Email}");
      Console.WriteLine($"Ethnicity|{referralCreate.Ethnicity}");
      Console.WriteLine($"FamilyName|{referralCreate.FamilyName}");
      Console.WriteLine($"GivenName|{referralCreate.GivenName}");
      Console.WriteLine($"HasALearningDisability|" +
        $"{referralCreate.HasALearningDisability}");
      Console.WriteLine($"HasAPhysicalDisability|" +
        $"{referralCreate.HasAPhysicalDisability}");
      Console.WriteLine($"HasDiabetesType1|{referralCreate.HasDiabetesType1}");
      Console.WriteLine($"HasDiabetesType2|{referralCreate.HasDiabetesType2}");
      Console.WriteLine($"HasHypertension|{referralCreate.HasHypertension}");
      Console.WriteLine($"HasRegisteredSeriousMentalIllness|" +
        $"{referralCreate.HasRegisteredSeriousMentalIllness}");
      Console.WriteLine($"HeightCm|{referralCreate.HeightCm}");
      Console.WriteLine($"IsVulnerable|{referralCreate.IsVulnerable}");
      Console.WriteLine($"Mobile|{referralCreate.Mobile}");
      Console.WriteLine($"NhsNumber|{referralCreate.NhsNumber}");
      Console.WriteLine($"Postcode|{referralCreate.Postcode}");
      Console.WriteLine($"ReferringGpPracticeName|" +
        $"{referralCreate.ReferringGpPracticeName}");
      Console.WriteLine($"ReferringGpPracticeNumber|" +
        $"{referralCreate.ReferringGpPracticeNumber}");
      Console.WriteLine($"ServiceId|{referralCreate.ServiceId}");
      Console.WriteLine($"Sex|{referralCreate.Sex}");
      Console.WriteLine($"SourceSystem|{referralCreate.SourceSystem}");
      Console.WriteLine($"Telephone|{referralCreate.Telephone}");
      Console.WriteLine($"Ubrn|{referralCreate.Ubrn}");
      Console.WriteLine($"VulnerableDescription|" +
        $"{referralCreate.VulnerableDescription}");
      Console.WriteLine($"WeightKg|{referralCreate.WeightKg}");
      Console.WriteLine($"PdfParseLog|{referralCreate.PdfParseLog}");
      if (Downloaded == true)
      {
        Console.WriteLine("***************************");
        Console.WriteLine("ReferralPut (Update) Object");
        Console.WriteLine($"{referralCreate.Ubrn}");
        Console.WriteLine("***************************");
        ReferralPut referralUpdate =
          GenerateReferralUpdateObject(identifier, attachmentId, null);
        Console.WriteLine($"Address1|{referralUpdate.Address1}");
        Console.WriteLine($"Address2|{referralUpdate.Address2}");
        Console.WriteLine($"Address3|{referralUpdate.Address3}");
        Console.WriteLine($"CalculatedBmiAtRegistration|" +
          $"{referralUpdate.CalculatedBmiAtRegistration}");
        Console.WriteLine($"DateOfBirth|{referralUpdate.DateOfBirth}");
        Console.WriteLine($"DateOfBmiAtRegistration|" +
          $"{referralUpdate.DateOfBmiAtRegistration}");
        Console.WriteLine($"DateOfReferral|{referralUpdate.DateOfReferral}");
        Console.WriteLine($"DocumentVersion|{referralUpdate.DocumentVersion}");
        Console.WriteLine($"Email|{referralUpdate.Email}");
        Console.WriteLine($"Ethnicity|{referralUpdate.Ethnicity}");
        Console.WriteLine($"FamilyName|{referralUpdate.FamilyName}");
        Console.WriteLine($"GivenName|{referralUpdate.GivenName}");
        Console.WriteLine($"HasALearningDisability|" +
          $"{referralUpdate.HasALearningDisability}");
        Console.WriteLine($"HasAPhysicalDisability|" +
          $"{referralUpdate.HasAPhysicalDisability}");
        Console.WriteLine($"HasDiabetesType1|{referralUpdate.HasDiabetesType1}");
        Console.WriteLine($"HasDiabetesType2|{referralUpdate.HasDiabetesType2}");
        Console.WriteLine($"HasHypertension|{referralUpdate.HasHypertension}");
        Console.WriteLine($"HasRegisteredSeriousMentalIllness|" +
          $"{referralUpdate.HasRegisteredSeriousMentalIllness}");
        Console.WriteLine($"HeightCm|{referralUpdate.HeightCm}");
        Console.WriteLine($"IsVulnerable|{referralUpdate.IsVulnerable}");
        Console.WriteLine($"Mobile|{referralUpdate.Mobile}");
        Console.WriteLine($"NhsNumber|{referralUpdate.NhsNumber}");
        Console.WriteLine($"Postcode|{referralUpdate.Postcode}");
        Console.WriteLine($"ReferringGpPracticeName|" +
          $"{referralUpdate.ReferringGpPracticeName}");
        Console.WriteLine($"ReferringGpPracticeNumber|" +
          $"{referralUpdate.ReferringGpPracticeNumber}");
        Console.WriteLine($"ServiceId|{referralUpdate.ServiceId}");
        Console.WriteLine($"Sex|{referralUpdate.Sex}");
        Console.WriteLine($"SourceSystem|{referralUpdate.SourceSystem}");
        Console.WriteLine($"Telephone|{referralUpdate.Telephone}");
        Console.WriteLine($"VulnerableDescription|" +
          $"{referralUpdate.VulnerableDescription}");
        Console.WriteLine($"WeightKg|{referralUpdate.WeightKg}");
        Console.WriteLine($"PdfParseLog|{referralUpdate.PdfParseLog}");
        Console.WriteLine("**************************");
        Console.WriteLine("PROCESSED DOCUMENT CONTENT");
        Console.WriteLine($"{referralCreate.Ubrn}");
        Console.WriteLine("**************************");
        foreach (KeyValuePair<string, string> dictionaryItem in DocumentValues)
        {
          Console.WriteLine($"{dictionaryItem.Key}|{dictionaryItem.Value}");
        }
        Console.WriteLine("********************");
        Console.WriteLine("RAW DOCUMENT CONTENT");
        Console.WriteLine($"{referralCreate.Ubrn}");
        Console.WriteLine("********************");
        foreach (string s in DocumentContent)
        {
          Console.WriteLine(s);
        }
      }
      Console.WriteLine("*********************");
    }

    private void AppendToParsingReport(string textToAppend)
    {
      if (ObjectParsingReport != "")
      {
        ObjectParsingReport = ObjectParsingReport + $"; {textToAppend}";
      }
      else
      {
        ObjectParsingReport = textToAppend;
      }
    }

  }
}
