using System;
using System.Globalization;
using System.Text;

namespace Microsoft.SyndicationFeed.Utils;

internal static class DateTimeUtils
{
  private const string Rfc3339LocalDateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
  private const string Rfc3339UtcDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

  public static bool TryParseDate(string value, out DateTimeOffset result)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      result = default;
      return false;
    }

    if (TryParseDateRfc3339(value, out result))
    {
      return true;
    }

    return TryParseDateRssSpec(value, out result);
  }

  public static string ToRfc3339String(DateTimeOffset dto)
  {
    if (dto.Offset == TimeSpan.Zero)
    {
      return dto.ToUniversalTime().ToString(Rfc3339UtcDateTimeFormat, CultureInfo.InvariantCulture);
    }
    else
    {
      return dto.ToString(Rfc3339LocalDateTimeFormat, CultureInfo.InvariantCulture);
    }
  }

  public static string ToRfc1123String(DateTimeOffset dto)
  {
    return dto.ToString("r");
  }

  internal static void CollapseWhitespaces(StringBuilder builder)
  {
    var index = 0;
    var whiteSpaceStart = -1;
    while (index < builder.Length)
    {
      if (char.IsWhiteSpace(builder[index]))
      {
        if (whiteSpaceStart < 0)
        {
          whiteSpaceStart = index;
          // normalize all white spaces to be ' ' so that the date time parsing works
          builder[index] = ' ';
        }
      }
      else if (whiteSpaceStart >= 0)
      {
#pragma warning disable S2583
        if (index > whiteSpaceStart + 1)
#pragma warning restore S2583
        {
          // there are at least 2 spaces... replace by 1
          builder.Remove(whiteSpaceStart + 1, index - whiteSpaceStart - 1);
          index = whiteSpaceStart + 1;
        }

        whiteSpaceStart = -1;
      }

      ++index;
    }
  }

  internal static string NormalizeTimeZone(string rfc822TimeZone, out bool isUtc)
  {
    isUtc = false;

    // return a string in "-08:00" format
    if (rfc822TimeZone[0] == '+' || rfc822TimeZone[0] == '-')
    {
      // the time zone is supposed to be 4 digits but some feeds omit the initial 0
      var result = new StringBuilder(rfc822TimeZone);
      if (result.Length == 4)
      {
        // the timezone is +/-HMM. Convert to +/-HHMM
        result.Insert(1, '0');
      }
      result.Insert(3, ':');
      return result.ToString();
    }

    switch (rfc822TimeZone)
    {
      case "UT":
      case "Z":
        isUtc = true;
        return "-00:00";
      case "GMT":
        return "-00:00";
      case "A":
        return "-01:00";
      case "B":
        return "-02:00";
      case "C":
        return "-03:00";
      case "D":
      case "EDT":
        return "-04:00";
      case "E":
      case "EST":
      case "CDT":
        return "-05:00";
      case "F":
      case "CST":
      case "MDT":
        return "-06:00";
      case "G":
      case "MST":
      case "PDT":
        return "-07:00";
      case "H":
      case "PST":
        return "-08:00";
      case "I":
        return "-09:00";
      case "K":
        return "-10:00";
      case "L":
        return "-11:00";
      case "M":
        return "-12:00";
      case "N":
        return "+01:00";
      case "O":
        return "+02:00";
      case "P":
        return "+03:00";
      case "Q":
        return "+04:00";
      case "R":
        return "+05:00";
      case "S":
        return "+06:00";
      case "T":
        return "+07:00";
      case "U":
        return "+08:00";
      case "V":
        return "+09:00";
      case "W":
        return "+10:00";
      case "X":
        return "+11:00";
      case "Y":
        return "+12:00";
      default:
        return "";
    }
  }

  private static bool TryParseDateRssSpec(string value, out DateTimeOffset result)
  {
    if (string.IsNullOrEmpty(value))
    {
      return false;
    }

    var sb = new StringBuilder(value.Trim());

    if (sb.Length < 18)
    {
      return false;
    }

    if (sb[3] == ',')
    {
      // There is a leading (e.g.) "Tue, ", strip it off
      sb.Remove(0, 4);

      // There's supposed to be a space here but some implementations dont have one
      TrimStart(sb);
    }

    CollapseWhitespaces(sb);

    if (!char.IsDigit(sb[1]))
    {
      sb.Insert(0, '0');
    }

    if (sb.Length < 19)
    {
      return false;
    }

    var thereAreSeconds = sb[17] == ':';
    var timeZoneStartIndex = thereAreSeconds ? 21 : 18;

    if (timeZoneStartIndex > sb.Length)
    {
      return false;
    }

    var timeZoneSuffix = sb.ToString().Substring(timeZoneStartIndex);
    sb.Remove(timeZoneStartIndex, sb.Length - timeZoneStartIndex);

    sb.Append(NormalizeTimeZone(timeZoneSuffix, out var isUtc));

    var wellFormattedString = sb.ToString();

    var parseFormat = thereAreSeconds ? "dd MMM yyyy HH:mm:ss zzz" : "dd MMM yyyy HH:mm zzz";

    var dateTimeStylesToUse = isUtc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
    return DateTimeOffset.TryParseExact(wellFormattedString, parseFormat, CultureInfo.InvariantCulture.DateTimeFormat, dateTimeStylesToUse, out result);
  }

  private static void TrimStart(StringBuilder sb)
  {
    var i = 0;
    while (i < sb.Length)
    {
      if (!char.IsWhiteSpace(sb[i]))
      {
        break;
      }
      ++i;
    }

    if (i > 0)
    {
      sb.Remove(0, i);
    }
  }

  private static bool TryParseDateRfc3339(string dateTimeString, out DateTimeOffset result)
  {
    dateTimeString = dateTimeString.Trim();

    if (dateTimeString[19] == '.')
    {
      // remove any fractional seconds, we choose to ignore them
      int i = 20;
      while (dateTimeString.Length > i && char.IsDigit(dateTimeString[i]))
      {
        ++i;
      }
      dateTimeString = dateTimeString.Substring(0, 19) + dateTimeString.Substring(i);
    }

    if (DateTimeOffset.TryParseExact(dateTimeString, Rfc3339LocalDateTimeFormat, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out var localTime))
    {
      result = localTime;
      return true;
    }

    if (DateTimeOffset.TryParseExact(dateTimeString, Rfc3339UtcDateTimeFormat, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utcTime))
    {
      result = utcTime;
      return true;
    }

    return false;
  }
}
