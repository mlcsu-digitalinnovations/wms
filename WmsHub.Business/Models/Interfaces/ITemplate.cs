using System;

namespace WmsHub.Business.Models.Notify;

public interface ITemplate
{
  string ExpectedPersonalisationCsv { get; set; }
  Guid Id { get; set; }
  string Name { get; set; }
}
