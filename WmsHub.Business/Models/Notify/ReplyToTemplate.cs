using System;

namespace WmsHub.Business.Models.Notify;

public class ReplyToTemplate : ITemplate
{
  public ReplyToTemplate()
  {}

  public ReplyToTemplate(Guid id, string name)
  {
    Id = id;
    Name = name;
  }

  public Guid Id { get; set; }
  public string Name { get; set; }
  public string ExpectedPersonalisationCsv {get; set; }
}
