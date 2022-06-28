using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  [Collection("Service collection")]
  public class EthnicityServiceTests : ServiceTestsBase
  {
    private readonly DatabaseContext _context;
    private readonly EthnicityService _service;

    public EthnicityServiceTests(ServiceFixture serviceFixture)
      : base(serviceFixture)
    {
      _context = new DatabaseContext(_serviceFixture.Options);
      _service = new EthnicityService(_context, _serviceFixture.Mapper)
      {
        User = GetClaimsPrincipal()
      };
    }

    [Fact]
    public async Task Get()
    {
      // arrange
      int expectedNoOfEthnicities = _context.Ethnicities.Count();

      // act
      IEnumerable<Ethnicity> ethnicities = await _service.Get();

      // assert
      ethnicities.Count().Should().Be(expectedNoOfEthnicities);
    }

    [Fact]
    public async Task GetEthnicityGroupMembersAsync()
    {
      // arrange
      string groupName = Enums.EthnicityGroup.White.ToString();
      int expectedNoOfEthnicities = 4;

      // act
      IEnumerable<Ethnicity> ethnicities = 
        await _service.GetEthnicityGroupMembersAsync(groupName);

      // assert
      ethnicities.Count().Should().Be(expectedNoOfEthnicities);
    }
    [Fact]
    public async Task GetEthnicityGroupNamesAsync()
    {
      // arrange
      int expectedGroups = Enum.GetNames(typeof(Enums.EthnicityGroup)).Length;

      // act
      IEnumerable<string> ethnicGroupNameList = 
        await _service.GetEthnicityGroupNamesAsync();

      // assert
      ethnicGroupNameList.Count().Should().Be(expectedGroups);
    }
  }
}
