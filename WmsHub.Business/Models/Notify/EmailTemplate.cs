using System;

namespace WmsHub.Business.Models.Notify;

public class EmailTemplate : ITemplate
{
  public EmailTemplate()
  {}

  public EmailTemplate(string name, Guid id) 
  {
    Name = name;
    Id = id;
  }

  public string ExpectedPersonalisationCsv { get; set; }

  public Guid Id { get; set; }
  public string Name { get; set; }
}
