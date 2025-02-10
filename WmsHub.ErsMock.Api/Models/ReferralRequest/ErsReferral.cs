using WmsHub.Common.Helpers;
using WmsHub.Common.Models;

namespace WmsHub.ErsMock.Api.Models.ReferralRequest;

public class ErsReferral : IErsReferral
{
  public List<ErsAttachment> Attachments { get; set; } = new();
  public List<AttachmentList>? Contained { get; set; }
  public List<string> ExcludedFiles { get; } = new();
  public string? Id { get; set; }
  public ReferralMetaData? Meta { get; set; }
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

    AttachmentList[] attachments = Contained == null 
      ? Array.Empty<AttachmentList>() 
      : Contained.ToArray();

    FileNamesToExclude ??= new();

    for (int i = 0; i < attachments.Length; i++)
    {
      AttachmentList attachmentList = attachments[i];

      if (attachmentList.Content != null)
      {

        for (int j = 0; j < attachmentList.Content.Count; j++)
        {
          if (attachmentList.Content[j] != null)
          {
            ErsAttachment attachment = 
              attachmentList.Content[j].Attachment ?? new();

            //Filter out excluded files
            bool excluded = FileNamesToExclude
              .Exists(n => RegexUtilities.IsWildcardMatch(n, attachment.Title));

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
}
