using System;
using System.Collections;
using System.Text;
using Microsoft.SyndicationFeed.Utils;
using DTO = Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions.DateTimeOffsetExtentions;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Utils;

public class DateTimeUtilsTests
{
  [TestCaseSource(nameof(TryParseDateData))]
  public void TryParseDateShouldReturnExpectedValues(string pubDate, bool expectedResult, DateTimeOffset expectedDate)
  {
    var result = DateTimeUtils.TryParseDate(pubDate, out var actualDate);
    Assert.Multiple(() =>
    {
      Assert.That(result, Is.EqualTo(expectedResult));
      Assert.That(actualDate, Is.EqualTo(expectedDate));
    });
  }

  [TestCase("abc", "abc")]
  [TestCase("a  bc", "a bc")]
  [TestCase("a\tbc", "a bc")]
  [TestCase("a \tbc", "a bc")]
  [TestCase("a \r\nbc", "a bc")]
  [TestCase("a\t\r\n\v\fbc", "a bc")]
  public void CollapseWhitespacesShouldReturnExpectedValues(string input, string expectedResult)
  {
    var sb = new StringBuilder(input);
    DateTimeUtils.CollapseWhitespaces(sb);
    Assert.That($"{sb}", Is.EqualTo(expectedResult));
  }

  [TestCaseSource(nameof(NormalizeTimeZoneData))]
  public void NormalizeTimeZoneShouldReturnExpectedValues(string rfc822TimeZone, bool expectedIsUtc, string expectedResult)
  {
    var result = DateTimeUtils.NormalizeTimeZone(rfc822TimeZone, out var isUtc);
    Assert.Multiple(() =>
    {
      Assert.That(isUtc, Is.EqualTo(expectedIsUtc));
      Assert.That(result, Is.EqualTo(expectedResult));
    });
  }

  private static IEnumerable TryParseDateData()
  {
    yield return new TestCaseData("", false, new DateTimeOffset());
    yield return new TestCaseData(null, false, new DateTimeOffset());
    yield return new TestCaseData("Sun, 13 Aug 1998 04:27:00 CET", false, new DateTimeOffset());
    yield return new TestCaseData("Thu, 06 Jul 2017 20:25:00", false, new DateTimeOffset());
    yield return new TestCaseData("Thu, 06 Jul 2017 20:25:00 GMT", true, DTO.Create(2017, 07, 06, 20, 25, 0));
    yield return new TestCaseData("Sun, 13 Aug 1998 04:27:00 A", true, DTO.Create(1998, 08, 13, 04, 27, 0, 1));
    yield return new TestCaseData("Thu, 03 Aug 2017 02:37:09 +0530", true, DTO.Create(2017, 08, 03, 02, 37, 09, -5, -30));
    yield return new TestCaseData("Thu,\t03\tAug\t2017\t02:37:09\t+0530", true, DTO.Create(2017, 08, 03, 02, 37, 09, -5, -30));
    yield return new TestCaseData("2003-12-13T08:29:29-04:00", true, DTO.Create(2003, 12, 13, 12, 29, 29));
    yield return new TestCaseData("2003-12-13T08:29:29-4:00", true, DTO.Create(2003, 12, 13, 12, 29, 29));
  }

  private static IEnumerable NormalizeTimeZoneData()
  {
    yield return new TestCaseData("UT", true, "-00:00");
    yield return new TestCaseData("Z", true, "-00:00");
    yield return new TestCaseData("GMT", false, "-00:00");
    yield return new TestCaseData("A", false, "-01:00");
    yield return new TestCaseData("+0530", false, "+05:30");
    yield return new TestCaseData("-0400", false, "-04:00");
    yield return new TestCaseData("-400", false, "-04:00");
  }
}
