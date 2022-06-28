using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ReferralsService.Tests.Models
{
  public class ErsReferralTests: AModelsBaseTests
  {
    private readonly string[] _fields = new string[]
    {
      "id",
      "contained",
      "Attachments",
      "meta"
    };

    private readonly string _validFileTypes = "PDF|DOC|RTF|DOCX";
    [Fact]
    public void CorrectNumberFields()
    {
      //Arrange

      //Act
      PropertyInfo[] propinfo = GetAllProperties(new ErsReferral());
      //Assert
      propinfo.Length.Should().Be(_fields.Length);
      foreach (PropertyInfo info in propinfo)
      {
        Array.IndexOf(_fields, info.Name).Should()
          .BeGreaterThan(-1, info.Name);
      }
    }

    [Fact]
    public void Valid()
    {
      //Arrange
      ErsReferral model = new ErsReferral();
      model.contained = new List<AttachmentList>
      {
        new AttachmentList
        {
          Content = new List<AttachmentContainer>
          {
            new AttachmentContainer
            {
              Attachment = new ErsAttachment
              {
                ContentType = "pdf",
                Creation = DateTime.Now,
                Id = "100001",
                Size = 2048,
                Title = "TestFileName.pdf",
                Url = "127.0.0.1/test.pdf"
              }
            },
          },
          Indexed = DateTimeOffset.Now,
          Status = "Test"
        }
      };

      //Act
      model.Finalise(_validFileTypes);
      //Assert
      model.Attachments.Count.Should().Be(model.contained.Count);
    }

    [Fact]
    public void InValid_FileType()
    {
      //Arrange
      ErsReferral model = new ErsReferral();
      model.contained = new List<AttachmentList>
      {
        new AttachmentList
        {
          Content = new List<AttachmentContainer>
          {
            new AttachmentContainer
            {
              Attachment = new ErsAttachment
              {
                ContentType = "png",
                Creation = DateTime.Now,
                Id = "100001",
                Size = 2048,
                Title = "TestFileName.png",
                Url = "127.0.0.1/test.png"
              }
            },
          },
          Indexed = DateTimeOffset.Now,
          Status = "Test"
        }
      };
      //Act
      model.Finalise(_validFileTypes);
      //Assert
      model.Attachments.Count.Should().Be(0);
    }

    [Fact]
    public void InValid_Null_FileTypes()
    {
      //Arrange
      ErsReferral model = new ErsReferral();
      //Act
      try
      {
        model.Finalise(null);
        //Assert
      }
      catch (Exception ex)
      {
        ex.Should().BeOfType<NullReferenceException>();
      }

      model.Attachments.Count.Should().Be(0);
    }
  }
}
