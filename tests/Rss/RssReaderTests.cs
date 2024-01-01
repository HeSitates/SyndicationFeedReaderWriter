// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Rss;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Rss;

public class RssReaderTests : ReaderWriterTestsBase
{
  [Test]
  public async Task ReadSequential()
  {
    using var stream = new StringReader(TestFeedResources.rss20);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new RssFeedReader(xmlReader);

    await reader.Read();

    for (var i = 0; i < 22; i++)
    {
      ISyndicationContent content = await reader.ReadContent();
      Assert.That(content, Is.Not.Null);
    }

    Assert.ThrowsAsync<InvalidOperationException>(async () => _ = await reader.ReadContent());
  }

  [Test]
  public async Task ReadItemAsContent()
  {
    using var stream = new StringReader(TestFeedResources.rss20);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new RssFeedReader(xmlReader);

    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Item)
      {

        // Read as content
        ISyndicationContent content = await reader.ReadContent();

        var fields = content.Fields.ToArray();
        Assert.That(fields, Has.Length.GreaterThanOrEqualTo(6));

        Assert.Multiple(() =>
        {
          Assert.That(fields[0].Name, Is.EqualTo("title"));
          Assert.That(string.IsNullOrEmpty(fields[0].Value), Is.False);

          Assert.That(fields[1].Name, Is.EqualTo("description"));
          Assert.That(string.IsNullOrEmpty(fields[1].Value), Is.False);

          Assert.That(fields[2].Name, Is.EqualTo("link"));
          Assert.That(string.IsNullOrEmpty(fields[2].Value), Is.False);

          Assert.That(fields[3].Name, Is.EqualTo("guid"));
          Assert.That(fields[3].Attributes.Count(), Is.EqualTo(1));
          Assert.That(string.IsNullOrEmpty(fields[3].Value), Is.False);

          Assert.That(fields[4].Name, Is.EqualTo("creator"));
          Assert.That(fields[4].Namespace, Is.EqualTo("http://purl.org/dc/elements/1.1/"));
          Assert.That(string.IsNullOrEmpty(fields[4].Value), Is.False);

          Assert.That(fields[5].Name, Is.EqualTo("pubDate"));
          Assert.That(string.IsNullOrEmpty(fields[5].Value), Is.False);
        });
      }
    }
  }

  [Test]
  public async Task ReadCategory()
  {
    using var stream = new StringReader(TestFeedResources.rss20);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new RssFeedReader(xmlReader);

    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Category)
      {
        ISyndicationCategory category = await reader.ReadCategory();

        Assert.Multiple(() =>
        {
          Assert.That(category.Name, Is.EqualTo("Newspapers"));
          Assert.That(category.Scheme, Is.EqualTo("http://example.com/news"));
        });
      }
    }
  }

  [Test]
  public async Task ReadItemCategory()
  {
    using var stream = new StringReader(TestFeedResources.rss20);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new RssFeedReader(xmlReader);

    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Item)
      {
        ISyndicationItem item = await reader.ReadItem();

        foreach (var c in item.Categories)
        {
          Assert.Multiple(() =>
          {
            Assert.That(c.Name, Is.EqualTo("Newspapers"));
            Assert.That(c.Scheme, Is.Null.Or.EqualTo("http://example.com/news/item"));
          });
        }
      }
    }
  }

  [Test]
  public async Task CountItems()
  {
    var itemCount = 0;

    using var stream = new StringReader(TestFeedResources.rss20);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    {
      var reader = new RssFeedReader(xmlReader);

      while (await reader.Read())
      {
        if (reader.ElementType == SyndicationElementType.Item)
        {
          itemCount++;
        }
      }
    }

    Assert.That(itemCount, Is.EqualTo(10));
  }

  [Test]
  public async Task ReadWhile()
  {
    using var stream = new StringReader(TestFeedResources.rss20);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new RssFeedReader(xmlReader);

    while (await reader.Read())
    {
      switch (reader.ElementType)
      {
        case SyndicationElementType.Link:
          ISyndicationLink link = await reader.ReadLink();
          Assert.That(link, Is.Not.Null);
          break;

        case SyndicationElementType.Item:
          ISyndicationItem item = await reader.ReadItem();
          Assert.That(item, Is.Not.Null);
          break;

        case SyndicationElementType.Person:
          ISyndicationPerson person = await reader.ReadPerson();
          Assert.That(person, Is.Not.Null);
          break;

        case SyndicationElementType.Image:
          ISyndicationImage image = await reader.ReadImage();
          Assert.That(image, Is.Not.Null);
          break;

        default:
          ISyndicationContent content = await reader.ReadContent();
          Assert.That(content, Is.Not.Null);
          break;
      }
    }
  }

  [Test]
  public static async Task ReadFeedElements()
  {
    using var stream = new StringReader(TestFeedResources.rss20_2items);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    await TestReadFeedElements(xmlReader);
  }
}
