using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class EthnicityServiceTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly EthnicityService _service;

    public EthnicityServiceTests(ServiceFixture serviceFixture) : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);
      _service = new EthnicityService(_context, _serviceFixture.Mapper)
      {
        User = GetClaimsPrincipal()
      };

      AServiceFixtureBase.PopulateEthnicities(_context);
    }

    [Fact]
    public async Task Get()
    {
      // Arrange.
      int expectedNoOfEthnicities = _context.Ethnicities.Count();

      // Act.
      IEnumerable<Ethnicity> ethnicities = await _service.GetAsync();

      // Assert.
      ethnicities.Count().Should().Be(expectedNoOfEthnicities);
    }

    [Fact]
    public async Task GetEthnicityGroupMembersAsync()
    {
      // Arrange.
      string groupName = Enums.EthnicityGroup.White.ToString();
      int expectedNoOfEthnicities = 4;

      // Act.
      IEnumerable<Ethnicity> ethnicities = await _service.GetEthnicityGroupMembersAsync(groupName);

      // Assert.
      ethnicities.Count().Should().Be(expectedNoOfEthnicities);
    }

    [Fact]
    public async Task GetEthnicityGroupNamesAsync()
    {
      // Arrange.
      int expectedGroups = Enum.GetNames(typeof(Enums.EthnicityGroup)).Length;

      // Act.
      IEnumerable<string> ethnicGroupNameList = await _service.GetEthnicityGroupNamesAsync();

      // Assert.
      ethnicGroupNameList.Count().Should().Be(expectedGroups);
    }
  }
}
