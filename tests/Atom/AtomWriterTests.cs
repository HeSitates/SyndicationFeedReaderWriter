// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions;
using Microsoft.SyndicationFeed.Rss;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Atom;

public class AtomWriterTests : ReaderWriterTestsBase
{
  [Test]
  public async Task WriteCategory()
  {
    var category = new SyndicationCategory("Test Category");

    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.Write(category);
      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<category term=\"{category.Name}\" />";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WritePerson()
  {
    await using var sw = new StringWriterWithEncoding();

    var p1 = new SyndicationPerson("John Doe", "johndoe@contoso.com");
    var p2 = new SyndicationPerson("Jane Doe", "janedoe@contoso.com", AtomContributorTypes.Contributor)
    {
      Uri = "www.contoso.com/janedoe"
    };

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.Write(p1);
      await writer.Write(p2);

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<author><name>{p1.Name}</name><email>{p1.Email}</email></author><contributor><name>{p2.Name}</name><email>{p2.Email}</email><uri>{p2.Uri}</uri></contributor>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WriteImage()
  {
    var icon = new SyndicationImage(new Uri("http://contoso.com/icon.ico"), AtomImageTypes.Icon);
    var logo = new SyndicationImage(new Uri("http://contoso.com/logo.png"), AtomImageTypes.Logo);

    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.Write(icon);
      await writer.Write(logo);

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<icon>{icon.Url}</icon><logo>{logo.Url}</logo>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WriteLink()
  {
    await using var sw = new StringWriterWithEncoding();

    var link = new SyndicationLink(new Uri("http://contoso.com"))
    {
      Title = "Test title",
      Length = 123,
      MediaType = "mp3/video"
    };

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.Write(link);

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<link title=\"{link.Title}\" href=\"{link.Uri}\" type=\"{link.MediaType}\" length=\"{link.Length}\" />";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WriteEntry()
  {
    var link = new SyndicationLink(new Uri("https://contoso.com/alternate"));
    var related = new SyndicationLink(new Uri("https://contoso.com/related"), AtomLinkTypes.Related);
    var self = new SyndicationLink(new Uri("https://contoso.com/28af09b3"), AtomLinkTypes.Self);
    var enclosure = new SyndicationLink(new Uri("https://contoso.com/podcast"), AtomLinkTypes.Enclosure)
    {
      Title = "Podcast",
      MediaType = "audio/mpeg",
      Length = 4123
    };
    var source = new SyndicationLink(new Uri("https://contoso.com/source"), AtomLinkTypes.Source)
    {
      Title = "Blog",
      LastUpdated = DateTimeOffset.UtcNow.AddDays(-10)
    };
    var author = new SyndicationPerson("John Doe", "johndoe@email.com");
    var category = new SyndicationCategory("Lorem Category");

    // 
    // Construct entry
    var entry = new AtomEntry()
    {
      Id = "https://contoso.com/28af09b3",
      Title = "Lorem Ipsum",
      Description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit...",
      LastUpdated = DateTimeOffset.UtcNow,
      ContentType = "text/html",
      Summary = "Proin egestas sem in est feugiat, id laoreet massa dignissim",
      Rights = $"copyright (c) {DateTimeOffset.UtcNow.Year}"
    };

    entry.AddLink(link);
    entry.AddLink(enclosure);
    entry.AddLink(related);
    entry.AddLink(source);
    entry.AddLink(self);

    entry.AddContributor(author);

    entry.AddCategory(category);

    //
    // Write
    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.Write(entry);
      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<entry><id>{entry.Id}</id><title>{entry.Title}</title><updated>{entry.LastUpdated.ToRfc3339()}</updated><link href=\"{link.Uri}\" /><link title=\"{enclosure.Title}\" href=\"{enclosure.Uri}\" rel=\"{enclosure.RelationshipType}\" type=\"{enclosure.MediaType}\" length=\"{enclosure.Length}\" /><link href=\"{related.Uri}\" rel=\"{related.RelationshipType}\" /><source><title>{source.Title}</title><link href=\"{source.Uri}\" /><updated>{source.LastUpdated.ToRfc3339()}</updated></source><link href=\"{self.Uri}\" rel=\"{self.RelationshipType}\" /><author><name>{author.Name}</name><email>{author.Email}</email></author><category term=\"{category.Name}\" /><content type=\"{entry.ContentType}\">{entry.Description}</content><summary>{entry.Summary}</summary><rights>{entry.Rights}</rights></entry>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WriteValue()
  {
    const string Title = "Example Feed";
    var id = Guid.NewGuid();
    var updated = DateTimeOffset.UtcNow.AddDays(-21);

    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.WriteTitle(Title);
      await writer.WriteId(id.ToString());
      await writer.WriteUpdated(updated);

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<title>{Title}</title><id>{id}</id><updated>{updated.ToRfc3339()}</updated>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WriteContent()
  {
    const string Uri = "https://contoso.com/generator";
    const string Version = "1.0";
    const string Generator = "Example Toolkit";

    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.WriteGenerator(Generator, Uri, Version);

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<generator uri=\"{Uri}\" version=\"{Version}\">{Generator}</generator>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WritePrefixedAtomNs()
  {
    const string Title = "Example Feed";
    const string Uri = "https://contoso.com/generator";
    const string Generator = "Example Toolkit";

    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter,
        new ISyndicationAttribute[] { new SyndicationAttribute("xmlns:atom", "http://www.w3.org/2005/Atom") });

      await writer.WriteTitle(Title);
      await writer.WriteGenerator(Generator, Uri, null);

      await writer.Flush();
    }

    var res = sw.ToString();
    var (expected, prefix) = ($"<atom:title>{Title}</atom:title><atom:generator uri=\"{Uri}\">{Generator}</atom:generator>", "atom");
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected, prefix)));
  }

  [Test]
  public async Task EmbededAtomInRssFeed()
  {
    var author = new SyndicationPerson("john doe", "johndoe@contoso.com");
    var entry = new AtomEntry()
    {
      Id = "https://contoso.com/28af09b3",
      Title = "Atom Entry",
      Description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit...",
      LastUpdated = DateTimeOffset.UtcNow
    };
    entry.AddContributor(author);

    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var attributes = new ISyndicationAttribute[] { new SyndicationAttribute("xmlns:atom", "http://www.w3.org/2005/Atom") };
      var writer = new RssFeedWriter(xmlWriter, attributes);
      var formatter = new AtomFormatter(attributes, xmlWriter.Settings);

      //
      // Write Rss elements
      await writer.WriteValue(RssElementNames.Title, "Rss Title");
      await writer.Write(author);
      await writer.Write(new SyndicationItem()
      {
        Title = "Rss Item",
        Id = "https://contoso.com/rss/28af09b3",
        Description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium",
        LastUpdated = DateTimeOffset.UtcNow
      });

      //
      // Write atom entry
      await writer.WriteRaw(formatter.Format(entry));

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<atom:entry><atom:id>{entry.Id}</atom:id><atom:title>{entry.Title}</atom:title><atom:updated>{entry.LastUpdated.ToRfc3339()}</atom:updated><atom:author><atom:name>{author.Name}</atom:name><atom:email>{author.Email}</atom:email></atom:author><atom:content>{entry.Description}</atom:content></atom:entry>";
    Assert.That(res, Does.Contain(expected));
  }

  [Test]
  public async Task WriteXhtmlTextConstruct()
  {
    await using var sw = new StringWriterWithEncoding();

    const string Content = "<h1><b href=\"foo\">Heading</b><br foo=\"bar\" /></h1><br />";

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.WriteText("title", Content, "xhtml");

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<title type=\"xhtml\"><div xmlns=\"http://www.w3.org/1999/xhtml\">{Content}</div></title>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WriteXmlContent()
  {
    await using var sw = new StringWriterWithEncoding();

    const string Content = "<h1 xmlns=\"boooo\"><b href=\"foo\">Heading</b><br foo=\"bar\" /></h1><br xmlns=\"\" />";

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter);

      await writer.WriteText("content", Content, "application/xml");

      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<content type=\"application/xml\">{Content}</content>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  [Test]
  public async Task WriteCDataValue()
  {
    const string Title = "Title & Markup";
    await using var sw = new StringWriterWithEncoding();

    await using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true }))
    {
      var writer = new AtomFeedWriter(xmlWriter, null, new AtomFormatter() { UseCDATA = true });

      await writer.WriteTitle(Title);
      await writer.Flush();
    }

    var res = sw.ToString();
    var expected = $"<title><![CDATA[{Title}]]></title>";
    Assert.That(res, Is.EqualTo(CreateExpectedResult(expected)));
  }

  private static string CreateExpectedResult(string expected)
  {
    return $"<?xml version=\"1.0\" encoding=\"utf-8\"?><feed xmlns=\"http://www.w3.org/2005/Atom\">{expected}</feed>";
  }

  private static string CreateExpectedResult(string expected, string prefix)
  {
    return $"<?xml version=\"1.0\" encoding=\"utf-8\"?><feed xmlns:{prefix}=\"http://www.w3.org/2005/Atom\">{expected}</feed>";
  }
}
