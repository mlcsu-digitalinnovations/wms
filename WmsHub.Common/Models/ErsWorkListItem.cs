using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Models
{
  public class ERSWorkListItem
  {
    private const char REFERENCE_SEPARATOR = '/';
    private const int REFERENCE_NO_OF_SPLITS = 2;
    private string _reference;

    [Required]
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

    public string Id { get; set; }
    public string ResourceType { get; set; }

    public string Ubrn => Id;

  }
}