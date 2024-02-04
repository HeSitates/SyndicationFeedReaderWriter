// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions;
using Microsoft.SyndicationFeed.Rss;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Rss;

public class RssWriterTests : ReaderWriterTestsBase
{
  [Test]
  public async Task WriteCategory()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    var cat1 = new SyndicationCategory("Test Category 1")
    {
      Scheme = "http://example.com/test"
    };

    var cat2 = new SyndicationCategory("Test Category 2");

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.Write(cat1);
      await writer.Write(cat2);
      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><category domain=\"{cat1.Scheme}\">{cat1.Name}</category><category>{cat2.Name}</category></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WritePerson()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.Write(new SyndicationPerson("John Doe", "author@email.com"));
      await writer.Write(new SyndicationPerson("John Smith", "mEditor@email.com", RssContributorTypes.ManagingEditor));

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><author>author@email.com (John Doe)</author><managingEditor>mEditor@email.com (John Smith)</managingEditor></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteImage()
  {
    var uri = new Uri("http://testuriforlink.com");

    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.Write(new SyndicationImage(uri)
      {
        Title = "Testing image title",
        Description = "testing image description",
        Link = new SyndicationLink(uri)
      });

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><image><url>{uri}</url><title>Testing image title</title><link>{uri}</link><description>testing image description</description></image></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteLink_onlyUrl()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.Write(new SyndicationLink(new Uri("http://testuriforlink.com")));
      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><link>http://testuriforlink.com/</link></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteLink_allElements()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    var link = new SyndicationLink(new Uri("http://testuriforlink.com"))
    {
      Title = "Test title",
      Length = 123,
      MediaType = "mp3/video"
    };

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.Write(link);
      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><link url=\"{link.Uri}\" type=\"{link.MediaType}\" length=\"{link.Length}\">{link.Title}</link></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteItem()
  {
    var url = new Uri("https://contoso.com/");

    var item = new SyndicationItem()
    {
      Id = "https://contoso.com/28af09b3-86c7-4dd6-b56f-58aaa17cff62",
      Title = "First item on ItemWriter",
      Description = "Brief description of an item",
      Published = DateTimeOffset.UtcNow
    };

    item.AddLink(new SyndicationLink(url));
    item.AddLink(new SyndicationLink(url, RssLinkTypes.Enclosure)
    {
      Title = "https://contoso.com/",
      Length = 4123,
      MediaType = "audio/mpeg"
    });
    item.AddLink(new SyndicationLink(url, RssLinkTypes.Comments));
    item.AddLink(new SyndicationLink(url, RssLinkTypes.Source)
    {
      Title = "Anonymous Blog"
    });

    item.AddLink(new SyndicationLink(new Uri(item.Id), RssLinkTypes.Guid));

    item.AddContributor(new SyndicationPerson("John Doe", "person@email.com"));

    item.AddCategory(new SyndicationCategory("Test Category"));

    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.Write(item);
      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><item><title>First item on ItemWriter</title><link>{url}</link><enclosure url=\"{url}\" length=\"4123\" type=\"audio/mpeg\" /><comments>{url}</comments><source url=\"{url}\">Anonymous Blog</source><guid>{item.Id}</guid><description>Brief description of an item</description><author>person@email.com (John Doe)</author><category>Test Category</category><pubDate>{item.Published.ToRfc1123()}</pubDate></item></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteContent()
  {
    ISyndicationContent content;

    using var stream = new StringReader(TestFeedResources.CustomXml);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    {
      var reader = new RssFeedReader(xmlReader);
      content = await reader.ReadContent();
    }

    var sb = new StringBuilder();

    await using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.Write(content);
      await writer.Flush();
    }

    var res = sb.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><NewItem><enclosure url=\"http://www.scripting.com/mp3s/weatherReportSuite.mp3\" length=\"12216320\" type=\"audio/mpeg\" /><title>Lorem ipsum 2017-07-06T20:25:00+00:00</title><description>Exercitation sit dolore mollit et est eiusmod veniam aute officia veniam ipsum.</description><link>http://example.com/test/1499372700</link><guid isPermaLink=\"true\">http://example.com/test/1499372700</guid><pubDate>Thu, 06 Jul 2017 20:25:00 GMT</pubDate></NewItem></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteValue()
  {
    var sb = new StringBuilder();

    await using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.WriteValue("CustomTag", "Custom Content");
      await writer.Flush();
    }

    var res = sb.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><CustomTag>Custom Content</CustomTag></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteCDataValue()
  {
    var sb = new StringBuilder();

    await using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter, null, new RssFormatter() { UseCDATA = true });

      await writer.WriteTitle("<h1>HTML Title</h1>");
      await writer.Flush();
    }

    var res = sb.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?><rss version=\"2.0\"><channel><title><![CDATA[<h1>HTML Title</h1>]]></title></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task Echo()
  {
    string res;
    using var stream = new StringReader(TestFeedResources.rss20_2items);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    {
      var reader = new RssFeedReader(xmlReader);

      await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

      await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
      {
        var writer = new RssFeedWriter(xmlWriter);

        while (await reader.Read())
        {
          switch (reader.ElementType)
          {
            case SyndicationElementType.Item:
              await writer.Write(await reader.ReadItem());
              break;

            case SyndicationElementType.Person:
              await writer.Write(await reader.ReadPerson());
              break;

            case SyndicationElementType.Image:
              await writer.Write(await reader.ReadImage());
              break;

            default:
              await writer.Write(await reader.ReadContent());
              break;
          }
        }

        await writer.Flush();
      }

      res = sw.ToString();
      var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><title asd=\"123\">Lorem ipsum feed for an interval of 1 minutes</title><description>This is a constantly updating lorem ipsum feed</description><link length=\"123\" type=\"testType\">http://example.com/</link><image><url>http://2.bp.blogspot.com/-NA5Jb-64eUg/URx8CSdcj_I/AAAAAAAAAUo/eCx0irI0rq0/s1600/bg_Microsoft_logo3-20120824073001907469-620x349.jpg</url><title>Microsoft News</title><link>http://www.microsoft.com/news</link><description>Test description</description></image><generator>RSS for Node</generator><lastBuildDate>Thu, 06 Jul 2017 20:25:17 GMT</lastBuildDate><managingEditor>John Smith</managingEditor><pubDate>Thu, 06 Jul 2017 20:25:00 GMT</pubDate><copyright>Michael Bertolacci, licensed under a Creative Commons Attribution 3.0 Unported License.</copyright><ttl>60</ttl><item><title>Lorem ipsum 2017-07-06T20:25:00+00:00</title><enclosure url=\"http://www.scripting.com/mp3s/weatherReportSuite.mp3\" length=\"12216320\" type=\"audio/mpeg\" /><link>http://example.com/test/1499372700</link><guid>http://example.com/test/1499372700</guid><description>Exercitation sit dolore mollit et est eiusmod veniam aute officia veniam ipsum.</description><author>John Smith</author><pubDate>Thu, 06 Jul 2017 20:25:00 GMT</pubDate></item><item><title>Lorem ipsum 2017-07-06T20:24:00+00:00</title><link>http://example.com/test/1499372640</link><guid>http://example.com/test/1499372640</guid><enclosure url=\"http://www.scripting.com/mp3s/weatherReportSuite.mp3\" length=\"12216320\" type=\"audio/mpeg\" /><description>Do ipsum dolore veniam minim est cillum aliqua ea.</description><author>John Smith</author><pubDate>Thu, 06 Jul 2017 20:24:00 GMT</pubDate></item></channel></rss>";
      Assert.That(res, Is.EqualTo(expected));
    }

    await RssReaderTests.TestReadFeedElements(XmlReader.Create(new StringReader(res)));
  }

  [Test]
  public async Task CompareContents()
  {
    string res;

    using (var stream = new StringReader(TestFeedResources.internetRssFeed))
    using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings() { Async = true }))
    {
      var reader = new RssFeedReader(xmlReader);

      await using var sw = new StringWriterWithEncoding(Encoding.UTF8);
      await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
      {
        var writer = new RssFeedWriter(xmlWriter);

        while (await reader.Read())
        {
          switch (reader.ElementType)
          {
            case SyndicationElementType.Item:
              await writer.Write(await reader.ReadItem());
              break;

            case SyndicationElementType.Person:
              await writer.Write(await reader.ReadPerson());
              break;

            case SyndicationElementType.Image:
              await writer.Write(await reader.ReadImage());
              break;

            default:
              await writer.Write(await reader.ReadContent());
              break;
          }
        }

        await writer.Flush();
      }

      res = sw.ToString();
    }

    using var streamSource = new StringReader(TestFeedResources.internetRssFeed);
    using var xmlReaderSource = XmlReader.Create(streamSource, new XmlReaderSettings() { Async = true });
    using var streamResult = new StringReader(res);
    using var xmlReaderResult = XmlReader.Create(streamResult, new XmlReaderSettings() { Async = true });
    await CompareFeeds(new RssFeedReader(xmlReaderSource), new RssFeedReader(xmlReaderResult));
  }

  [Test]
  public async Task WriteNamespaces()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter, new SyndicationAttribute[] { new("xmlns:content", "http://contoso.com/") });

      await writer.Write(new SyndicationContent("hello", "http://contoso.com/", "world"));
      await writer.Write(new SyndicationContent("world", "http://contoso.com/", "hello"));

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss xmlns:content=\"http://contoso.com/\" version=\"2.0\"><channel><content:hello>world</content:hello><content:world>hello</content:world></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteCloud()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.WriteCloud(new Uri("http://podcast.contoso.com/rpc"), "xmlStorageSystem.rssPleaseNotify", "xml-rpc");

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><cloud domain=\"podcast.contoso.com\" port=\"80\" path=\"/rpc\" registerProcedure=\"xmlStorageSystem.rssPleaseNotify\" protocol=\"xml-rpc\" /></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteSkipDays()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.WriteSkipDays(new DayOfWeek[] { DayOfWeek.Friday, DayOfWeek.Monday });

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><skipDays><day>Friday</day><day>Monday</day></skipDays></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task WriteSkipHours()
  {
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new RssFeedWriter(xmlWriter);

      await writer.WriteSkipHours(new byte[] { 0, 4, 1, 11, 23, 20 });

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\"><channel><skipHours><hour>0</hour><hour>4</hour><hour>1</hour><hour>11</hour><hour>23</hour><hour>20</hour></skipHours></channel></rss>";
    Assert.That(res, Is.EqualTo(expected));
  }

  [Test]
  public async Task FormatterWriterWithNamespaces()
  {
    const string ExampleNs = "http://contoso.com/syndication/feed/examples";
    await using var sw = new StringWriterWithEncoding(Encoding.UTF8);

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true, Indent = true }))
    {
      var attributes = new SyndicationAttribute[] { new("xmlns:example", ExampleNs) };

      var formatter = new RssFormatter(attributes, xmlWriter.Settings);
      var writer = new RssFeedWriter(xmlWriter, attributes, formatter);

      var item = new SyndicationItem()
      {
        Title = "Rss Writer Available",
        Description = "The new RSS Writer is now open source!",
        Id = "https://github.com/dotnet/wcf/tree/lab/lab/src/Microsoft.SyndicationFeed/src",
        Published = new DateTimeOffset(new DateTime(2024, 1, 1, 14, 44, 25, DateTimeKind.Utc))
      };

      item.AddCategory(new SyndicationCategory("Technology"));
      item.AddContributor(new SyndicationPerson(null, "test@mail.com"));

      var content = new SyndicationContent(formatter.CreateContent(item));

      content.AddField(new SyndicationContent("customElement", ExampleNs, "Custom Value"));

      await writer.Write(content);
      await writer.Write(content);

      await writer.Flush();
    }

    var res = sw.ToString();

    await CompareXml(res, TestFeedResources.RssFormatterWriterWithNamespacesResult);
  }

  private static async Task CompareFeeds(RssFeedReader f1, RssFeedReader f2)
  {
    while (await f1.Read() && await f2.Read())
    {
      Assert.That(f1.ElementType, Is.EqualTo(f2.ElementType));

      switch (f1.ElementType)
      {
        case SyndicationElementType.Item:
          CompareItem(await f1.ReadItem(), await f2.ReadItem());
          break;

        case SyndicationElementType.Person:
          ComparePerson(await f1.ReadPerson(), await f2.ReadPerson());
          break;

        case SyndicationElementType.Image:
          CompareImage(await f1.ReadImage(), await f2.ReadImage());
          break;

        case SyndicationElementType.Link:
          CompareLink(await f1.ReadLink(), await f2.ReadLink());
          break;

        case SyndicationElementType.None:
        case SyndicationElementType.Content:
        case SyndicationElementType.Category:
        default:
          CompareContent(await f1.ReadContent(), await f2.ReadContent());
          break;
      }
    }
  }

  private static void ComparePerson(ISyndicationPerson person1, ISyndicationPerson person2)
  {
    Assert.Multiple(() =>
    {
      Assert.That(person1.Email, Is.EqualTo(person2.Email));
      Assert.That(person1.RelationshipType, Is.EqualTo(person2.RelationshipType));
    });
  }

  private static void CompareImage(ISyndicationImage image1, ISyndicationImage image2)
  {
    Assert.Multiple(() =>
    {
      Assert.That(image1.RelationshipType, Is.EqualTo(image2.RelationshipType));
      Assert.That(image1.Url, Is.EqualTo(image2.Url));
      Assert.That(image1.Link.Uri, Is.EqualTo(image2.Link.Uri));
      Assert.That(image1.Description, Is.EqualTo(image2.Description));
    });
  }

  private static void CompareItem(ISyndicationItem item1, ISyndicationItem item2)
  {
    Assert.Multiple(() =>
    {
      Assert.That(item1.Id, Is.EqualTo(item2.Id));
      Assert.That(item1.Title, Is.EqualTo(item2.Title));
      Assert.That(item1.LastUpdated, Is.EqualTo(item2.LastUpdated));
    });
  }

  private static void CompareLink(ISyndicationLink link1, ISyndicationLink link2)
  {
    Assert.Multiple(() =>
    {
      Assert.That(link1.Uri, Is.EqualTo(link2.Uri));
      Assert.That(link1.Title, Is.EqualTo(link2.Title));
      Assert.That(link1.LastUpdated, Is.EqualTo(link2.LastUpdated));
    });
  }

  private static void CompareContent(ISyndicationContent content1, ISyndicationContent content2)
  {
    Assert.That(content1.Name, Is.EqualTo(content2.Name));

    //Compare attributes
    foreach (var a in content1.Attributes)
    {
      var a2 = content2.Attributes.Single(att => att.Name == a.Name);
      Assert.Multiple(() =>
      {
        Assert.That(a.Name, Is.EqualTo(a2.Name));
        Assert.That(a.Namespace, Is.EqualTo(a2.Namespace));
        Assert.That(a.Value, Is.EqualTo(a2.Value));
      });
    }

    //Compare fields
    foreach (var f in content1.Fields)
    {
      var f2 = content2.Fields.Single(field => field.Name == f.Name && field.Value == f.Value);
      CompareContent(f, f2);
    }

    Assert.That(content1.Value, Is.EqualTo(content2.Value));
  }
}