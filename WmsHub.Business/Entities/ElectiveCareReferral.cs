﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities;

public class ElectiveCareReferral : IElectiveCareReferral
{
  [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  public Guid ReferralId { get; set; }

  public string Ubrn => $"EC{Id:0000000000}";
}