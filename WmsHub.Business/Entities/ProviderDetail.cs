using System;
using WmsHub.Business.Entities.Interfaces;

namespace WmsHub.Business.Entities;

public class ProviderDetail : ProviderDetailBase, IProviderDetail
{
  public ProviderDetail()
  {
    IsActive = true;
  }

  public virtual Provider Provider { get; set; }
}