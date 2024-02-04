// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.SyndicationFeed.Utils;

internal static class Converter
{
  public static bool TryParseValue<T>(string value, out T result)
  {
    result = default(T);

    var type = typeof(T);

    if (type == typeof(string))
    {
      result = (T)(object)value;
      return true;
    }

    if (value == null)
    {
      return false;
    }

    if (type == typeof(DateTimeOffset))
    {
      if (DateTimeUtils.TryParseDate(value, out var dt))
      {
        result = (T)(object)dt;
        return true;
      }

      return false;
    }

    if (type == typeof(DateTime))
    {
      if (DateTimeUtils.TryParseDate(value, out var dt))
      {
        result = (T)(object)dt.DateTime;
        return true;
      }

      return false;
    }

#pragma warning disable S125
#pragma warning disable S1135
    // TODO: being added in netstandard 2.0
    //if (type.GetTypeInfo().IsEnum)
    //{
    //    if (Enum.TryParse(typeof(T), value, true, out T o)) {
    //        result = (T)(object)o;
    //        return true;
    //    }
    //}
#pragma warning restore S125
#pragma warning restore S1135

    // Uri
    if (type == typeof(Uri))
    {
      if (UriUtils.TryParse(value, out Uri uri))
      {
        result = (T)(object)uri;
        return true;
      }

      return false;
    }

    result = (T)Convert.ChangeType(value, typeof(T));
    return result != null;
  }
}
