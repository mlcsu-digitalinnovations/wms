using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Serilog;
using WmsHub.Business;
using WmsHub.Business.Entities;
using WmsHub.Business.Helpers;
using WmsHub.BusinessIntelligence.Api.Tests;
using WmsHub.Common.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.BusinessIntelligence.Api.Test;

public class ServiceTestsBase
{
  public const string TEST_USER_ID = "571342f1-c67d-49bf-a9c6-40a41e6dc702";
  protected readonly ServiceFixture _serviceFixture;
  protected ILogger _log;
  protected List<Business.Entities.Provider> _providers = new();
  protected List<StaffRole> _staffRoles = new();

  public ServiceTestsBase(ServiceFixture serviceFixture)
  {
    _serviceFixture = serviceFixture;
  }

  public ServiceTestsBase(
    ServiceFixture serviceFixture,
    ITestOutputHelper testOutputHelper)
  {
    _serviceFixture = serviceFixture;
    _log = new LoggerConfiguration()
      .MinimumLevel.Verbose()
      .WriteTo.TestOutput(testOutputHelper)
      .CreateLogger();
  }

  protected static ClaimsPrincipal GetClaimsPrincipal()
  {
    List<Claim> claims = new() { new(ClaimTypes.Sid, TEST_USER_ID) };

    ClaimsIdentity claimsIdentity = new(claims);

    ClaimsPrincipal claimsPrincipal = new(claimsIdentity);

    return claimsPrincipal;
  }

  protected Guid AddProvider(DatabaseContext context, string name)
  {
    Business.Entities.Provider provider =
      context.Providers.FirstOrDefault(t => t.Name == name);
    if (provider != null)
    {
      return provider.Id;
    }

    Business.Entities.Provider entity =
      RandomEntityCreator.CreateRandomProvider(
        id: Guid.NewGuid(),
        isActive: true,
        isLevel1: true,
        isLevel2: true,
        isLevel3: true,
        name: name);

    context.Providers.Add(entity);
    context.SaveChanges();
    _providers.Add(entity);
    return entity.Id;
  }

  protected void AddProviderSubmission(DatabaseContext context,
    Guid providerId,
    int? coaching = null,
    DateTimeOffset? date = null,
    int? measure = null,
    decimal? weight = null)
  {
    Business.Entities.Provider provider =
      context.Providers.SingleOrDefault(t => t.Id == providerId);
    if (provider == null)
    {
      Assert.Fail($"Unable to add provider in test using id {providerId} as " +
        "entity not found.");

    }

    ProviderSubmission entity = new()
    {
      Coaching = coaching ?? 5,
      Date = date ?? DateTime.Now.AddDays(-28),
      Measure = measure ?? 1,
      Weight = weight ?? 87,
      ProviderId = providerId,
      IsActive = true
    };

    context.ProviderSubmissions.Add(entity);
    provider.ProviderSubmissions.Add(entity);
    context.SaveChanges();
  }

  protected void AddStaffRoles(DatabaseContext context)
  {
    context.StaffRoles.RemoveRange(context.StaffRoles);
    context.SaveChanges();
    context.StaffRoles.AddRange(new StaffRole
    {
      DisplayName = "Doctor",
      IsActive = true,
      DisplayOrder = 1
    }, new StaffRole
    {
      DisplayName = "Ambulance Worker",
      IsActive = true,
      DisplayOrder = 2
    }, new StaffRole
    {
      DisplayName = "Nurse",
      IsActive = true,
      DisplayOrder = 3
    }, new StaffRole
    {
      DisplayName = "Porter",
      IsActive = true,
      DisplayOrder = 4
    });
    context.SaveChanges();
    _staffRoles = context.StaffRoles.ToList();
  }


  protected virtual async Task<Guid> AddTestReferral(DatabaseContext context,
    Guid providerId,
    string staffRole,
    int offset,
    EthnicityGrouping ethnictyGrouping = null,
    string ubrn = null)
  {
    EthnicityGrouping eg =
      ethnictyGrouping ?? Generators.GenerateEthnicityGrouping(new Random());

    Referral entity = RandomEntityCreator.CreateRandomReferral(
      dateOfReferral: DateTime.Now.AddDays(offset),
      ethnicity: eg.Ethnicity,
      serviceUserEthnicity: eg.ServiceUserEthnicity,
      serviceUserEthnicityGroup: eg.ServiceUserEthnicityGroup,
      hasDiabetesType1: true,
      hasDiabetesType2: false,
      hasHypertension: true,
      modifiedByUserId: Guid.Parse("fafc7655-89b7-42a3-bdf7-c57c72cd1d41"),
      providerId: providerId,
      referringGpPracticeName: "Referrer One",
      deprivation: "IMD2",
      ubrn: ubrn
    );

    context.Referrals.Add(entity);

    await context.SaveChangesAsync();
    return entity.Id;
  }

  protected virtual void Setup(DatabaseContext context)
  {
    AddStaffRoles(context);

    Guid p1 = AddProvider(context, "Provider One");
    Guid p2 = AddProvider(context, "Provider Two");

    AddProviderSubmission(context, p1, 5, DateTime.Now.AddDays(-28), 1, 87);
    AddProviderSubmission(context, p2, 6, DateTime.Now.AddDays(-14), 7, 97);
  }

  protected void Clear(DatabaseContext context, bool all = false)
  {
    context.ProviderSubmissions.RemoveRange(context.ProviderSubmissions);
    context.Providers.RemoveRange(context.Providers);
    context.StaffRoles.RemoveRange(context.StaffRoles);

    context.Referrals.RemoveRange(context.Referrals);
    context.SaveChanges();
  }
}