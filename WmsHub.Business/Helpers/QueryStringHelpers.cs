using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Business.Helpers;

public static class QueryStringHelpers
{
  /// <summary>
  /// The UserActionLogs are populated with a QueryString.  This method looks
  /// for and returns the UBRN.  However, the absolute path of the supplied
  /// URI may not be consistent due to changes on the original Javascript
  /// calling method.  Therefore, testing for "forward-all-as-email" will 
  /// become redundant.
  /// </summary>
  /// <param name="query">URI containing the QueryString</param>
  /// <returns>string UBRN</returns>
  public static string FindUbrn(this Uri query)
  {
    string key = "Ubrn";

    if (query == null)
    {
      throw new ArgumentNullException(nameof(query));
    }

    Dictionary<string, StringValues> queryString = 
      QueryHelpers.ParseQuery(query.Query);

    if (!queryString.ContainsKey(key))
    {
      return string.Empty;
    }

    return queryString[key][0];
  }

  /// <summary>
  /// Searches the URI supplied and returns the UBRN if it exists or
  /// string.Empty if it fails to find the UBRN, or has an exception.
  /// </summary>
  /// <param name="query">URI containing the QueryString</param>
  /// <returns>string UBRN</returns>
  public static string TryFindUbrn(this Uri query)
  {
    try
    {
      return FindUbrn(query);
    }
    catch
    {
      return string.Empty;
    }
  }
}
