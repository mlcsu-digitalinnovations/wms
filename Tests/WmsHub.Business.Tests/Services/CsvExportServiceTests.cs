using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  public class CsvExportServiceTests: CsvExportService
  {
    private readonly MapperConfiguration _config;
    private readonly IMapper _map;
    public CsvExportServiceTests()
    {
      _config = new MapperConfiguration(cfg => cfg
        .CreateMap<Entities.Referral, Referral>()
        .ForMember(dest => dest.CriLastUpdated,
          opt => opt.MapFrom((src, dest) =>
          {
            if (src.Cri != null)
            {
              return src.Cri.ClinicalInfoLastUpdated;
            }

            return (DateTimeOffset?)null;
          })).ReverseMap());
      _map = _config.CreateMapper();
    }
    public class ExportTests : CsvExportServiceTests
    {
      
      [Fact]
      public void Valid()
      {
        //arrange
        Entities.Referral entity =
          RandomEntityCreator.CreateRandomReferral();
        Referral model = _map.Map<Referral>(entity);
        var list = new List<Referral> {model};
        //act
        byte[] result = Export<CsvExportAttribute>(list);
        //assert
        result.Should().BeOfType<byte[]>();
      }
    }
  }
}
