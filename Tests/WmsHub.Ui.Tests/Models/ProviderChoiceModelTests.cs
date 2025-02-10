using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Common.Helpers;
using WmsHub.Tests.Helper;
using WmsHub.Ui.Models;
using Xunit;

namespace WmsHub.Ui.Tests.Models
{
  public class ProviderChoiceModelTests : AModelsBaseTests
  {
    [Fact]
    public void Valid()
    {
      // arrange
      var model = Create();

      // act
      var result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
    }

    [Fact]
    public void Invalid_ProviderId()
    {
      // arrange
      var model = Create();
      model.ProviderId = Guid.Empty;

      // act
      var result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Count.Should().Be(1);
      result.Results[0].ErrorMessage.Should().Be("A service must be selected");
      result.Results[0].MemberNames.ToList().Single().Should().Be("ProviderId");
    }

    private static ProviderChoiceModel Create(
      Guid id = default,
      Provider provider = null,
      Guid providerId = default,
      List<Provider> providers = null,
      string token = null,
      string ubrn = null)
    {
      Random random = new Random();
      return new ProviderChoiceModel()
      {
        Id = id == default ? Guid.NewGuid() : id,
        Provider = provider,
        ProviderId = providerId == default ? Guid.NewGuid() : providerId,
        Providers = providers,
        Token = token,
        Ubrn = string.IsNullOrWhiteSpace(ubrn)
          ? Generators.GenerateUbrn(random)
          : ubrn
      };
    }
  }
}
