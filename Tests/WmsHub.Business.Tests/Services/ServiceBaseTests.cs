using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Common.Validation;
using Xunit;

namespace WmsHub.Business.Tests.Services
{
  /// <summary>
  /// ServiceBase is an abstract class
  /// </summary>
  public class ServiceBaseTests
  {
    public class ModelValidationTests: ServiceBaseTests
    {
      public class ServiceUserRequestTests: ModelValidationTests
      {
        [Theory]
        [InlineData("Rejected", "reason test",false, true)]
        [InlineData("Started","", false, true)]
        [InlineData("Update","",false,false)]
        [InlineData("Terminated","reason test", false, true)]
        [InlineData("Completed","", false, true)]
        [InlineData("Rejected", "reason test", true, false)]
        [InlineData("Started", "", true, true)]
        [InlineData("Update", "", true, true)]
        [InlineData("Terminated", "reason test", true, false)]
        [InlineData("Completed", "", true, true)]
        [InlineData("Rejected", "", false, false)]
        [InlineData("Terminated", "", false, false)]
        public void ValidModelWithOptionalUpdates(string type, 
          string reason, 
          bool include, 
          bool expected)
        {
          //arrange
          var request = new ServiceUserSubmissionRequest
          {
            Ubrn = "000000000001",
            Date = DateTime.UtcNow,
            Type = type,
            Reason = reason
          };

          if (include)
          {
            var updates = new List<ServiceUserUpdatesRequest>();
            updates.Add(new ServiceUserUpdatesRequest
            {
              Date = DateTime.UtcNow,
              Weight = 78.2M
            });
            request.Updates = updates;
          }
          
          //act
          var context = new ValidationContext(instance: request);

          var result = new ValidateModelResult();
          result.IsValid = Validator.TryValidateObject(
            request, context, result.Results, validateAllProperties: true);

          //assert
          Assert.Equal(expected, result.IsValid);
        }
      }
    }
  }


}
