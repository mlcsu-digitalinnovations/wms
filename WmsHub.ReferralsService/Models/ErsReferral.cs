using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WmsHub.Common.Helpers;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models.Fhir;

namespace WmsHub.ReferralsService.Models;

public class ErsReferral : IErsReferral
{
  public List<ErsAttachment> Attachments { get; set; } = new();
  public List<AttachmentList> Contained { get; set; }
  public HttpStatusCode ErsResponseStatus { get; set; }
  public bool WasRetrievedFromErs => ErsResponseStatus == HttpStatusCode.OK;
  public List<string> ExcludedFiles { get; } = new();
  public string Id { get; set; }
  public ReferralMetaData Meta { get; set; }  
  public static List<string> FileNamesToExclude { get; set; } = new();  

  /// <summary>
  /// Processes the raw attachments and puts them in a list in the order
  /// of most recent first.
  /// </summary>
  /// <param name="supportedFileTypes">File extensions not in this 
  /// pipe-sepated list will be ignored</param>
  public void Finalise(string supportedFileTypes)
  {
    if (string.IsNullOrWhiteSpace(supportedFileTypes))
    {
      throw new ArgumentNullException(nameof(supportedFileTypes));
    }

    AttachmentList[] attachments;

    if (Contained == null)
    {
      attachments = Array.Empty<AttachmentList>();
    }
    else
    {
      attachments = Contained.ToArray();
    }

    if (FileNamesToExclude == null)
    {
      FileNamesToExclude = new();
    }

    for (int i = 0; i < attachments.Length; i++)
    {
      if (attachments[i].Content != null)
      {
        for (int j = 0; j < attachments[i].Content.Count; j++)
        {
          if (attachments[i].Content[j] != null)
          {
            ErsAttachment attachment = attachments[i].Content[j].Attachment;
            //Filter out excluded files
            bool excluded = FileNamesToExclude.Exists(
              n => RegexUtilities.IsWildcardMatch(n, attachment.Title));
            if (excluded)
            {
              ExcludedFiles.Add(attachment.Title);
            }
            else
            {
              //Filter out unsupported file types
              string fileExtension =
                attachment.FileExtension;
              if (supportedFileTypes.Contains(fileExtension))
              {
                Attachments.Add(attachment);
              }
            }
          }
        }
      }
    }
    Attachments.Sort();
  }

  /// <inheritdoc />
  public ErsAttachment GetMostRecentAttachment()
  {
    ErsAttachment mostRecentAttachment = null;

    if (Attachments != null && Attachments.Count > 0)
    {
      mostRecentAttachment = Attachments.OrderByDescending(x => x.Creation).First();
    }

    return mostRecentAttachment;
  }
}
