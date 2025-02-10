using System.Collections.Generic;
using System.Linq;
using WmsHub.Business.Extensions;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class ExtensionsTests
  {
    [Fact]
    public void SingleItemCorrectOrderReturned()
    {
      //arrange
      IEnumerable<TestItem> items = TestList.TestItems0();
      
      //act
      IEnumerable<TestItem> result =
        items.BringToTop(t => t.Status == "up");
      
      var expectedCount = items.Count();
      var actualCount = result.Count();

      var actualFirstItem = result.ToList()[0];
      var expectedFirstItem = items.ToList()[0];

      //assert
      Assert.Equal(expectedCount, actualCount);
      Assert.Equal(expectedFirstItem.Status, actualFirstItem.Status);
    }
    
    [Fact]
    public void CorrectOrderReturned()
    {
      //arrange
      IEnumerable<TestItem> items = TestList.TestItems1();

      //act 
      IEnumerable<TestItem> result =
        items.BringToTop(t => t.Status == "left");

      var expectedLefts = result.Take(3);
      var actualLefts = expectedLefts.Where(t => t.Status == "left");

      //assert
      Assert.Equal(20, result.Count());
      Assert.Equal(expectedLefts.Count(), actualLefts.Count());
    }
  }

  public static class TestList
  {
    public static IEnumerable<TestItem> TestItems0()
    {
      return new List<TestItem>
      {
        new TestItem
        {
          Id = 1,
          Name = "A_Test",
          Status = "up"
        }
      };
    }
    public static IEnumerable<TestItem> TestItems1()
    {
      return new List<TestItem>
      {
        new TestItem
        {
          Id = 1,
          Name = "A_Test",
          Status = "up"
        },new TestItem
        {
          Id = 2,
          Name = "B_Test",
          Status = "down"
        },new TestItem
        {
          Id = 3,
          Name = "C_Test",
          Status = "up"
        },new TestItem
        {
          Id = 4,
          Name = "D_Test",
          Status = "up"
        },new TestItem
        {
          Id = 5,
          Name = "E_Test",
          Status = "left"
        },new TestItem
        {
          Id = 6,
          Name = "F_Test",
          Status = "down"
        },new TestItem
        {
          Id = 7,
          Name = "G_Test",
          Status = "down"
        },new TestItem
        {
          Id = 8,
          Name = "H_Test",
          Status = "up"
        },new TestItem
        {
          Id = 9,
          Name = "I_Test",
          Status = "up"
        },new TestItem
        {
          Id = 10,
          Name = "J_Test",
          Status = "up"
        },new TestItem
        {
          Id = 11,
          Name = "K_Test",
          Status = "down"
        },new TestItem
        {
          Id = 12,
          Name = "L_Test",
          Status = "left"
        },new TestItem
        {
          Id = 13,
          Name = "M_Test",
          Status = "up"
        },new TestItem
        {
          Id = 14,
          Name = "N_Test",
          Status = "up"
        },new TestItem
        {
          Id = 15,
          Name = "O_Test",
          Status = "down"
        },new TestItem
        {
          Id = 16,
          Name = "P_Test",
          Status = "up"
        },new TestItem
        {
          Id = 17,
          Name = "Q_Test",
          Status = "down"
        },new TestItem
        {
          Id = 18,
          Name = "R_Test",
          Status = "up"
        },new TestItem
        {
          Id = 19,
          Name = "S_Test",
          Status = "left"
        },new TestItem
        {
          Id = 20,
          Name = "T_Test",
          Status = "down"
        },
      };
    }
  }

  public class TestItem
  {
    public int Id { get; set; }
    public string Status { get; set; }
    public string Name { get; set; }
  }
}
