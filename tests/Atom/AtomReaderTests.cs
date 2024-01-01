// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Interfaces;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Atom;

public class AtomReaderTests
{
  [Test]
  public async Task ReadPerson()
  {
    using var stream = new StringReader(TestFeedResources.simpleAtomFeed);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var persons = new List<ISyndicationPerson>();
    var reader = new AtomFeedReader(xmlReader);
    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Person)
      {
        var person = await reader.ReadPerson();
        persons.Add(person);
      }
    }

    Assert.That(persons, Has.Count.EqualTo(2));
    Assert.Multiple(() =>
    {
      Assert.That(persons[0].Name, Is.EqualTo("Mark Pilgrim"));
      Assert.That(persons[0].Email, Is.EqualTo("f8dy@example.com"));
      Assert.That(persons[0].Uri, Is.EqualTo("http://example.org/"));
      Assert.That(persons[1].Name, Is.EqualTo("Sam Ruby"));
    });
  }

  [Test]
  public async Task ReadImage()
  {
    using var stream = new StringReader(TestFeedResources.simpleAtomFeed);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new AtomFeedReader(xmlReader);
    var imagesRead = 0;

    List<string> contentsOfImages = [];

    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Image)
      {
        ISyndicationImage image = await reader.ReadImage();
        imagesRead++;
        contentsOfImages.Add(image.Url.OriginalString);
      }
    }
    Assert.That(imagesRead, Is.EqualTo(2));
    Assert.Multiple(() =>
    {
      Assert.That(contentsOfImages[0], Is.EqualTo("/icon.jpg"));
      Assert.That(contentsOfImages[1], Is.EqualTo("/logo.jpg"));
    });
  }

  [Test]
  public async Task ReadCategory()
  {
    using var stream = new StringReader(TestFeedResources.simpleAtomFeed);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new AtomFeedReader(xmlReader);
    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Category)
      {
        ISyndicationCategory category = await reader.ReadCategory();
        Assert.That(category.Name, Is.EqualTo("sports"));
        Assert.Multiple(() =>
        {
          Assert.That(category.Label, Is.EqualTo("testLabel"));
          Assert.That(category.Scheme, Is.EqualTo("testScheme"));
        });
      }
    }
  }

  [Test]
  public async Task ReadLink()
  {
    using var stream = new StringReader(TestFeedResources.simpleAtomFeed);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new AtomFeedReader(xmlReader);
    List<string> hrefs = [];
    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Link)
      {
        ISyndicationLink link = await reader.ReadLink();
        hrefs.Add(link.Uri.OriginalString);
      }
    }

    Assert.Multiple(() =>
    {
      Assert.That(hrefs[0], Is.EqualTo("http://example.org/"));
      Assert.That(hrefs[1], Is.EqualTo("http://example.org/feed.atom"));
    });
  }

  [Test]
  public async Task ReadItem()
  {
    using var stream = new StringReader(TestFeedResources.simpleAtomFeed);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new AtomFeedReader(xmlReader);
    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Item)
      {
        IAtomEntry item = await reader.ReadEntry();

        //Assert content of item
        Assert.Multiple(() =>
        {
          Assert.That(item.Title, Is.EqualTo("Atom draft-07 snapshot"));
          Assert.That(item.Links.Count(), Is.EqualTo(3));
          Assert.That(item.Contributors.Count(), Is.EqualTo(3));
          Assert.That(item.Rights, Is.EqualTo("All rights Reserved. Contoso."));
          Assert.That(item.Id, Is.EqualTo("tag:example.org,2003:3.2397"));
          Assert.That(string.IsNullOrEmpty(item.Description), Is.False);
        });
      }
    }
  }

  [Test]
  public async Task ReadItemContent()
  {
    using var stream = new StringReader(TestFeedResources.simpleAtomFeed);
    using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
    var reader = new AtomFeedReader(xmlReader);

    while (await reader.Read())
    {
      if (reader.ElementType == SyndicationElementType.Item)
      {
        ISyndicationContent content = await reader.ReadContent();

        var fields = content.Fields.ToArray();

        Assert.Multiple(() =>
        {
          Assert.That(fields, Has.Length.EqualTo(12));

          Assert.That(fields[0].Name, Is.EqualTo("title"));
          Assert.That(string.IsNullOrEmpty(fields[0].Value), Is.False);

          Assert.That(fields[1].Name, Is.EqualTo("link"));
          Assert.That(fields[1].Attributes.Count(), Is.GreaterThan(0));

          Assert.That(fields[2].Name, Is.EqualTo("link"));
          Assert.That(fields[2].Attributes.Count(), Is.GreaterThan(0));

          Assert.That(fields[3].Name, Is.EqualTo("id"));
          Assert.That(string.IsNullOrEmpty(fields[3].Value), Is.False);

          Assert.That(fields[4].Name, Is.EqualTo("updated"));
          Assert.That(string.IsNullOrEmpty(fields[4].Value), Is.False);

          Assert.That(fields[5].Name, Is.EqualTo("published"));
          Assert.That(string.IsNullOrEmpty(fields[5].Value), Is.False);

          Assert.That(fields[6].Name, Is.EqualTo("source"));
          Assert.That(fields[6].Fields.Count(), Is.GreaterThan(0));

          Assert.That(fields[7].Name, Is.EqualTo("author"));
          Assert.That(fields[7].Fields.Count(), Is.GreaterThan(0));

          Assert.That(fields[8].Name, Is.EqualTo("contributor"));
          Assert.That(fields[8].Fields.Count(), Is.GreaterThan(0));

          Assert.That(fields[9].Name, Is.EqualTo("contributor"));
          Assert.That(fields[9].Fields.Count(), Is.GreaterThan(0));

          Assert.That(fields[10].Name, Is.EqualTo("rights"));
          Assert.That(string.IsNullOrEmpty(fields[10].Value), Is.False);

          Assert.That(fields[11].Name, Is.EqualTo("content"));
          Assert.That(string.IsNullOrEmpty(fields[11].Value), Is.False);
        });
      }
    }
  }
}