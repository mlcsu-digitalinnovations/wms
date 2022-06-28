using System;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Entities;

namespace WmsHub.Business.Models.PatientTriage
{
  public class PatientTriage
  {
    public string TriageSection { get; set; }
    public string Key { get; set; }
    public string Descriptions { get; set; }
    public int Value { get; set; }
    public int CheckSum { get; set; }
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid ModifiedByUserId { get; set; }
  }
}
