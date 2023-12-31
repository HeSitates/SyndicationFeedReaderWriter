using System;
using System.Globalization;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions;

internal static class DateTimeOffsetExtentions
{
    public static string ToRfc1123(this DateTimeOffset dto)
    {
        return dto.ToString("r");
    }

    public static string ToRfc3339(this DateTimeOffset dto)
    {
      if (dto.Offset == TimeSpan.Zero)
      {
        return dto.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
      }

      return dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
    }
}
