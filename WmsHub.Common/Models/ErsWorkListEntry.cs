using System;
using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Common.Models
{
  public class ErsWorkListEntry
  {
    protected const char REFERENCE_SEPARATOR = '-';
    protected const int REFERENCE_NO_OF_SPLITS = 2;
    protected const string EXTENSION_URL_PATIENT = "patient";
    protected const string EXTENSION_CRI_UPDATED = "clinicalInfoLastUpdated";
    protected const string EXTENSION_CRI_SUBMITTED =
      "clinicalInfoFirstSubmitted";

    public string ServiceIdentifier { get; set; }
    public virtual ERSWorkListItem Item { get; set; }

    public ExtensionModel[] Extension { get; set; }

    public virtual string NhsNumber => Extension
      .FirstOrDefault(e => e.NhsNumber != null)?.NhsNumber;

    public virtual DateTimeOffset? ClinicalInfoLastUpdated => Extension
      .FirstOrDefault(e => e.ClinicalInfoLastUpdated != null)?
      .ClinicalInfoLastUpdated;

    public string Ubrn => Item.Ubrn;

    public class ExtensionModel
    {
      public List<ExtensionSubModel> Extension { get; set; }

      public string NhsNumber => Extension
        .FirstOrDefault(e => e.Url == EXTENSION_URL_PATIENT)
        ?.ValueReference
        ?.Id;

      public DateTimeOffset? ClinicalInfoLastUpdated
      {
        get
        {
           DateTimeOffset? currentValue =
          Extension.FirstOrDefault(e => e.Url == EXTENSION_CRI_UPDATED)
            ?.ValueDateTime??null;
          if (currentValue == null)
          {
            currentValue =
            Extension.FirstOrDefault(e => e.Url == EXTENSION_CRI_SUBMITTED)
              ?.ValueDateTime ?? null;
          }
          return currentValue;
        }
      }

      
      public class ExtensionSubModel
      {
        public string Url { get; set; }

        public ValueReferenceModel ValueReference { get; set; }
        public DateTimeOffset ValueDateTime { get; set; }
        public class ValueReferenceModel
        {
          private string _reference;

          public string Reference
          {
            get
            {
              return _reference;
            }
            set
            {
              _reference = value;

              string[] splitValue = value.Split(
                REFERENCE_SEPARATOR,
                REFERENCE_NO_OF_SPLITS, 
                System.StringSplitOptions.RemoveEmptyEntries);

              if (splitValue.Length == REFERENCE_NO_OF_SPLITS)
              {
                ResourceType = splitValue[0];
                Id = splitValue[1];
              }
            }
          }

          public string Id { get; private set; }
          public string ResourceType { get; private set; }

          public string Ubrn => Id;
        }
      }
    }
  }
}
