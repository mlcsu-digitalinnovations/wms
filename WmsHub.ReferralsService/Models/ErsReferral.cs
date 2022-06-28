using System.Collections.Generic;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Models.Fhir;

namespace WmsHub.ReferralsService.Models
{
  public class ErsReferral
  {
    public string id { get; set; }
    public ReferralMetaData meta { get; set; }
    public List<AttachmentList> contained {get; set;}    
    public List<ErsAttachment> Attachments { get; set; } = 
      new List<ErsAttachment>();

    /// <summary>
    /// Processes the raw attachments and puts them in a list in the order
    /// of most recent first.
    /// </summary>
    /// <param name="supportedFileTypes">File extensions not in this 
    /// pipe-sepated list will be ignored</param>
    public void Finalise(string supportedFileTypes)
    {

      AttachmentList[] attachments = contained.ToArray();
      for (int i = 0; i < attachments.Length; i++)
      {
        if (attachments[i].Content != null)
        {
          for (int j = 0; j < attachments[i].Content.Count; j++)
          {
            string fileExtension = 
              attachments[i].Content[j].Attachment.FileExtension;
            if (supportedFileTypes.Contains(fileExtension))
            {
              Attachments.Add(attachments[i].Content[j].Attachment);
            }
          }
        }
      }
      Attachments.Sort();
    }
  }
}
