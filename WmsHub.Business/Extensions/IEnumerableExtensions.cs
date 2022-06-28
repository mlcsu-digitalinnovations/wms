using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace WmsHub.Business.Extensions
{
  public static  class IEnumerableExtensions
  {
    public static IEnumerable<T> BringToTop<T>(this IEnumerable<T> list,
      Func<T, bool> predicate)
    { 
      List<T> topOfList = new List<T>();
      List<T> bottomOfList = new List<T>();
      
      foreach (T item in list)
      {
        if (predicate.Invoke(item))
        {
          topOfList.Add(item);
        }
        else
        {
          bottomOfList.Add(item);
        }
      }
      
      return topOfList.Union(bottomOfList);
    }
  }
}
