using System;
using System.Diagnostics.CodeAnalysis;
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

  [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Just conveniance for testcases.")]
  public static DateTimeOffset Create(int year, int month, int day, int hour, int minute, int second, int hoursTimeZone = 0, int minutesTimezone = 0)
  {
    return new DateTimeOffset(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddHours(hoursTimeZone).AddMinutes(minutesTimezone));
  }
}
