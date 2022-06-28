using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;
using WmsHub.ReferralsService.Pdf.Models.Configuration;
using WmsHub.ReferralsService.Pdf;

namespace WmsHub.ReferralsService.Models.Configuration
{
  [ExcludeFromCodeCoverage]
  public class DataConfiguration
  {
    public string Fqdn { get; set; }
    /// <summary>
    /// The base URL of the referrals API
    /// </summary>
    public string BaseUrl { get; set; }
    /// <summary>
    /// Our Accredited Systems ID
    /// </summary>
    public string AccreditedSystemsID { get; set; }
    /// <summary>
    /// Our business function for NHS eReferrals API calls
    /// </summary>
    public string BusinessFunction { get; set; }
    /// <summary>
    /// Our organisation identifier for NHS eReferrals API calls
    /// </summary>
    public string OrgIdentifier { get; set; }
    /// <summary>
    /// Our service identifier list for NHS eReferrals API calls
    /// </summary>
    public List<string> ServiceIdentifiers { get; set; }
    /// <summary>
    /// Smart card ticket override for NHS eReferrals API calls
    /// </summary>
    public string OverrideGaTicket { get; set; }
    /// <summary>
    /// The path to create a professional session (A001)
    /// </summary>
    public string CreateProfessionalSessionPath { get; set; }
    /// <summary>
    /// The path to select a professional session role (A002)
    /// </summary>
    public string ProfessionalSessionSelectRolePath { get; set; }
    /// <summary>
    /// The path to retieve the clinical information (A007)
    /// </summary>
    public string RetrieveClinicalInformationPath { get; set; }
    /// <summary>
    /// The path to retrieve the worklist (A008)
    /// </summary>
    public string RetrieveWorklistPath { get; set; }
    /// <summary>
    /// The path to retrieve a referral request (A005)
    /// </summary>
    public string RegistrationPath { get; set; }
    /// <summary>
    /// The path to retrieve an attachment (A006)
    /// </summary>
    public string AttachmentPath { get; set; }
    /// <summary>
    /// The path to reject a referral (A014)
    /// </summary>
    public string RecordReviewOutcomePath { get; set; }
    /// <summary>
    /// The path to determine the avialable actions which can be performed on 
    /// a referral record
    /// </summary>
    public string AvailableActionsPath { get; set; }
    /// <summary>
    /// Outgoing Hub API path
    /// </summary>
    public string HubRegistrationAPIPath { get; set; }
    /// <summary>
    /// Outgoing Hub API path for exceptions
    /// </summary>
    public string HubRegistrationExceptionAPIPath { get; set; }
    /// <summary>
    /// Outgoing Hub API Key
    /// </summary>
    public string HubRegistrationAPIKey { get; set; }
    /// <summary>
    /// Secure certificate thumbprint for connections to NHS eReferrals API
    /// </summary>
    public object ClientCertificateThumbprint { get; set; }
    /// <summary>
    /// The amount of time time to wait between API calls for attachment 
    /// downloads, which is throttled by NHS Digital.
    /// </summary>
    public float MinimumAttachmentDownloadTimeSeconds { get; set; }
    /// <summary>
    /// The timeout applied to download attachment API calls
    /// </summary>
    public double TimeoutAttachmentDownloadTimeSeconds { get; set; } = 240;
    /// <summary>
    /// Pipe-separated list of supporder file types. Eg "|PDF|DOC|DOCX|RTF|"
    /// </summary>
    public string SupportedAttachmentFileTypes { get; set; }
    /// <summary>
    /// Temporary File Path for word interop document conversion into PDF format
    /// </summary>
    public string InteropTemporaryFilePath { get; set; }
    /// <summary>
    /// Path of the certificate used when authenticating
    /// </summary>
    public string ClientCertificateFilePath { get; set; }

    public string ClientCertificatePassword { get; set; }
    /// <summary>
    /// Attachment filenames with wild cards which will not be processed
    /// </summary>
    public List<string> ExcludedFiles { get; set; }
    /// <summary>
    /// If set to true, some document formatting is performed to remove
    /// headers and footers and ensure answers are on the same page as questions
    /// </summary>
    public bool ReformatDocument { get; set; } 
    /// <summary>
    /// Section headings used when splitting the document during reformating
    /// </summary>
    public string[] SectionHeadings { get; set; }
    /// <summary>
    /// When true, attachments an attempt to parse documents with no source
    /// will be performed
    /// </summary>
    public bool RetryAllSources { get; set; }
    /// <summary>
    /// When attempting to identify a document, the number of missing questions
    /// which will be allowed before the source type is abandoned
    /// </summary>
    public int NumberOfMissingQuestionsTolerance { get; set; }
    public ReferralAttachmentAnswerMap AnswerMap { get; set; }
    /// <summary>
    /// Configuration items for the Pdf parser
    /// </summary>
    public PdfParserConfig ParserConfig { get; set; }
    /// <summary>
    /// If set to TRUE, the Service Id will be sent as a parameter to the hub
    /// when retrieving referral records.
    /// </summary>
    public bool SendServiceIdToHubForReferralList { get; set; }
  }
}
