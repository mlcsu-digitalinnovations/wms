using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using WmsHub.Business.Models;
using WmsHub.Ui.Models.Profiles;
using Xunit;
using Provider = WmsHub.Ui.Models.Provider;

namespace WmsHub.Ui.Controllers.Tests
{
  public class RmcControllerTests
  {
    public class MapperConfigTests
    {
      //[Fact] TODO: #1619 BUG reported
      public void ProviderProfile_Valid()
      {
        //Arrange
        Mock<Provider> providerModel = new Mock<Provider>();
        providerModel.Object.Id = Guid.NewGuid();
        Mock<Business.Models.Provider> provider =
          new Mock<Business.Models.Provider>();
        provider.Object.Id = providerModel.Object.Id;
        provider.Object.Summary = "Test Summary 1";
        provider.Object.Summary2 = "Test Summary 2";
        provider.Object.Summary3 = "Test Summary 3";
        Mock<IReferral> referral = new Mock<IReferral>();
        List<Business.Models.Provider> providers =
          new List<Business.Models.Provider> {provider.Object};
        referral.Setup(t => t.Providers).Returns(providers);
        //act
        var config = new MapperConfiguration(cfg => {
          cfg.AddProfile<ProviderProfile>();
        });

        IMapper mapper = config.CreateMapper();
        var result = mapper.Map<List<Provider>>(referral);
      }

    }
  }
}