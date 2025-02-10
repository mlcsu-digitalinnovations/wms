using System;

namespace WmsHub.Business.Models.Notify;

public partial class SmsTemplate : ITemplate
{
  public string ExpectedPersonalisationCsv { get; set; }
  public SmsTemplate() { }

  public SmsTemplate(Guid id, string name)
  {
    Id = id;
    Name = name ?? throw new ArgumentNullException(nameof(name));
  }

  public string Name { get; set; }

  public Guid Id { get; set; }
}
