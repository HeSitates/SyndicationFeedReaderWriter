﻿using System;
using System.Collections;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions;
using Microsoft.SyndicationFeed.Rss;
using DTO = Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions.DateTimeOffsetExtentions;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Rss;

public partial class RssFormatterTests
{
  [TestCase(null, null, "", TestName = "Ignored test.")]
  [TestCaseSource(nameof(CreateEnclosureMissingRequiredPropertiesData))]
  public void CreateEnclosureContentShouldThrowAnExceptionIfInvalid(Type exceptionType, ISyndicationLink link, string expectedExceptionMessage)
  {
    if (link?.Uri == null)
    {
      Assert.Ignore("Null check is done in another method");
    }

    var sut = new RssFormatter();

    var ex = Assert.Throws(exceptionType, () => sut.CreateEnclosureContent(link));
    Assert.That(ex, Is.Not.Null);
    Assert.That(ex.Message, Is.EqualTo(expectedExceptionMessage));
  }

  [TestCaseSource(nameof(CreateEnclosureValidData))]
  public void CreateEnclosureContentShouldNotThrowAnException(ISyndicationLink syndicationItem, string expectedResult)
  {
    var sut = new RssFormatter();

    var result = sut.CreateEnclosureContent(syndicationItem);
    Assert.That(result, Is.Not.Null.And.TypeOf<SyndicationContent>());
    var serialized = result.Serialize(true);
    TestContext.WriteLine(serialized);
    Assert.That(serialized, Is.EqualTo(expectedResult));
  }

  [TestCaseSource(nameof(CreateCommentsValidData))]
  public void CreateCommentsContentShouldNotThrowAnException(ISyndicationLink syndicationItem, string expectedResult)
  {
    var sut = new RssFormatter();

    var result = sut.CreateCommentsContent(syndicationItem);
    Assert.That(result, Is.Not.Null.And.TypeOf<SyndicationContent>());
    var serialized = result.Serialize(true);
    TestContext.WriteLine(serialized);
    Assert.That(serialized, Is.EqualTo(expectedResult));
  }

  [TestCaseSource(nameof(CreateSourceValidData))]
  public void CreateSourceContentShouldNotThrowAnException(ISyndicationLink syndicationItem, string expectedResult)
  {
    var sut = new RssFormatter();

    var result = sut.CreateSourceContent(syndicationItem);
    Assert.That(result, Is.Not.Null.And.TypeOf<SyndicationContent>());
    var serialized = result.Serialize(true);
    TestContext.WriteLine(serialized);
    Assert.That(serialized, Is.EqualTo(expectedResult));
  }

  [TestCaseSource(nameof(CreateLinkValidData))]
  public void CreateLinkContentShouldNotThrowAnException(ISyndicationLink syndicationItem, string expectedResult)
  {
    var sut = new RssFormatter();

    var result = sut.CreateLinkContent(syndicationItem);
    Assert.That(result, Is.Not.Null.And.TypeOf<SyndicationContent>());
    var serialized = result.Serialize(true);
    TestContext.WriteLine(serialized);
    Assert.That(serialized, Is.EqualTo(expectedResult));
  }

  private static IEnumerable CreateEnclosureMissingRequiredPropertiesData()
  {
    yield return new TestCaseData(typeof(ArgumentException), SyndicationLinkBuilder.CreateEnclosure().Build(), "Enclosure requires length attribute (Parameter 'link')")
      .SetName("CreateEnclosure with default builder");
    yield return new TestCaseData(typeof(ArgumentException), SyndicationLinkBuilder.CreateEnclosure().WithLength(0).Build(), "Enclosure requires length attribute (Parameter 'link')")
      .SetName("CreateEnclosure with length equals 0");
    yield return new TestCaseData(typeof(ArgumentNullException), SyndicationLinkBuilder.CreateEnclosure().WithLength().Build(), "Enclosure requires a MediaType (Parameter 'link')")
      .SetName("CreateEnclosure with no mediatype");
    yield return new TestCaseData(typeof(ArgumentNullException), SyndicationLinkBuilder.CreateEnclosure().WithLength().WithMediaType(string.Empty).Build(), "Enclosure requires a MediaType (Parameter 'link')")
      .SetName("CreateEnclosure with empty mediatype");
  }

  private static IEnumerable CreateEnclosureValidData()
  {
    yield return new TestCaseData(SyndicationLinkBuilder.CreateEnclosure().WithLength().WithMediaType().Build(), RssFormatterResources.CreateEnclosureBasic)
      .SetName("CreateEnclosure basic");
    // The title property of the link is ignored.
    yield return new TestCaseData(SyndicationLinkBuilder.CreateEnclosure().WithLength().WithMediaType().WithTitle().Build(), RssFormatterResources.CreateEnclosureBasic)
      .SetName("CreateEnclosure with title");
    // The lastupdated property of the link is ignored.
    yield return new TestCaseData(SyndicationLinkBuilder.CreateEnclosure().WithLength().WithMediaType().WithLastUpdated().Build(), RssFormatterResources.CreateEnclosureBasic)
      .SetName("CreateEnclosure with last updated");
  }

  private static IEnumerable CreateCommentsValidData()
  {
    yield return new TestCaseData(SyndicationLinkBuilder.CreateComments().Build(), RssFormatterResources.CreateCommentsBasic).SetName("CreateComments basic");
    // The title property of the link is ignored.
    yield return new TestCaseData(SyndicationLinkBuilder.CreateComments().WithTitle().Build(), RssFormatterResources.CreateCommentsBasic).SetName("CreateComments with title");
  }

  private static IEnumerable CreateSourceValidData()
  {
    yield return new TestCaseData(SyndicationLinkBuilder.CreateSource().Build(), RssFormatterResources.CreateSourceBasic)
      .SetName("CreateSource basic");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateSource().WithTitle(string.Empty).Build(), RssFormatterResources.CreateSourceWithEmptyTitle)
      .SetName("CreateSource with empty title");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateSource().WithTitle().Build(), RssFormatterResources.CreateSourceWithTitle)
      .SetName("CreateSource with title");
  }

  private static IEnumerable CreateLinkValidData()
  {
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink().Build(), RssFormatterResources.CreateLinkBasic)
      .SetName("CreateLink basic");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink(RssLinkTypes.Alternate).Build(), RssFormatterResources.CreateLinkBasic)
      .SetName("CreateLink basic and alternate link type");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink().WithTitle(string.Empty).Build(), RssFormatterResources.CreateLinkBasic)
      .SetName("CreateLink with empty title");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink().WithTitle().Build(), RssFormatterResources.CreateLinkWithTitle)
      .SetName("CreateLink with title");

    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink().WithMediaType().Build(), RssFormatterResources.CreateLinkBasicWithMediaType)
      .SetName("CreateLink basic with media type");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink().WithLength().Build(), RssFormatterResources.CreateLinkBasicWithLength)
      .SetName("CreateLink basic with length");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink().WithMediaType().WithLength().Build(), RssFormatterResources.CreateLinkBasicWithMediaTypeAndLength)
      .SetName("CreateLink basic with media type and length");

    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink("Test").Build(), RssFormatterResources.CreateLinkWithLinkTypeTest)
      .SetName("CreateLink link type \"Test\"");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink("Test").WithMediaType().Build(), RssFormatterResources.CreateLinkWithLinkTypeTestAndMediaType)
      .SetName("CreateLink link type \"Test\" with media type");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink("Test").WithLength().Build(), RssFormatterResources.CreateLinkWithLinkTypeTestAndLength)
      .SetName("CreateLink link type \"Test\" with length");
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink("Test").WithLength().WithMediaType().Build(), RssFormatterResources.CreateLinkWithLinkTypeTestAndMediaTypeAndLength)
      .SetName("CreateLink link type \"Test\" with media type and length");
  }

  internal class SyndicationLinkBuilder
  {
    private readonly SyndicationLink _item;

    private SyndicationLinkBuilder(Uri uri, string linkTypes)
    {
      _item = new SyndicationLink(uri, linkTypes);
      if (linkTypes == RssLinkTypes.Source)
      {
        _item.Title = uri.AbsoluteUri;
      }
    }

    public static SyndicationLinkBuilder CreateLink(string linkType = "", Uri uri = null)
    {
      if (uri == null)
      {
        uri = new Uri("https://contoso.com/podcast");
      }

      var builder = new SyndicationLinkBuilder(uri, linkType);
      return builder;
    }

    public static SyndicationLinkBuilder CreateEnclosure(Uri uri = null)
    {
      if (uri == null)
      {
        uri = new Uri("https://contoso.com/podcast");
      }

      var builder = new SyndicationLinkBuilder(uri, RssElementNames.Enclosure);
      return builder;
    }

    public static SyndicationLinkBuilder CreateComments(Uri uri = null)
    {
      if (uri == null)
      {
        uri = new Uri("https://contoso.com/podcast");
      }

      var builder = new SyndicationLinkBuilder(uri, RssElementNames.Comments);
      return builder;
    }

    public static SyndicationLinkBuilder CreateSource(Uri uri = null)
    {
      if (uri == null)
      {
        uri = new Uri("https://contoso.com/podcast");
      }

      var builder = new SyndicationLinkBuilder(uri, RssElementNames.Source);
      return builder;
    }

    public SyndicationLinkBuilder WithMediaType(string mediaType = "AMediaType")
    {
      _item.MediaType = mediaType;
      return this;
    }

    public SyndicationLinkBuilder WithLength(long length = 1234)
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      _item.Length = length;
      return this;
    }

    public SyndicationLinkBuilder WithTitle(string title = "ATitle")
    {
      _item.Title = title;
      return this;
    }

    public SyndicationLinkBuilder WithLastUpdated(DateTimeOffset? lastUpdated = null)
    {
      lastUpdated ??= DTO.Create(2017, 07, 06, 20, 25, 0);

      _item.LastUpdated = lastUpdated.Value;
      return this;
    }

    public ISyndicationLink Build()
    {
      return _item;
    }
  }
}
