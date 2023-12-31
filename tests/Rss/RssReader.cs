// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Rss;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Rss;

public class RssReader
{
    [Test]
    public async Task ReadSequential()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        await reader.Read();

        _ = await reader.ReadContent();
        _ = await reader.ReadContent();
        _ = await reader.ReadContent();
    }

    [Test]
    public async Task ReadItemAsContent()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
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
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        while (await reader.Read())
        {
            if (reader.ElementType == SyndicationElementType.Category)
            {
                ISyndicationCategory category = await reader.ReadCategory();

                Assert.That(category.Name, Is.EqualTo("Newspapers"));
                Assert.That(category.Scheme, Is.EqualTo("http://example.com/news"));
            }
        }
    }

    [Test]
    public async Task ReadItemCategory()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        while (await reader.Read())
        {
            if (reader.ElementType == SyndicationElementType.Item)
            {
                ISyndicationItem item = await reader.ReadItem();

                foreach (var c in item.Categories)
                {
                    Assert.That(c.Name, Is.EqualTo("Newspapers"));
                    Assert.That(c.Scheme, Is.Null.Or.EqualTo("http://example.com/news/item"));
                }
            }
        }
    }

    [Test]
    public async Task CountItems()
    {
        var itemCount = 0;

        using (var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true }))
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
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        while (await reader.Read())
        {
            switch (reader.ElementType)
            {
                case SyndicationElementType.Link:
                    ISyndicationLink link = await reader.ReadLink();
                    break;

                case SyndicationElementType.Item:
                    ISyndicationItem item = await reader.ReadItem();
                    break;

                case SyndicationElementType.Person:
                    ISyndicationPerson person = await reader.ReadPerson();
                    break;

                case SyndicationElementType.Image:
                    ISyndicationImage image = await reader.ReadImage();
                    break;

                default:
                    ISyndicationContent content = await reader.ReadContent();
                    break;
            }
        }
    }

    [Test]
    public static async Task ReadFeedElements()
    {
        using var reader = XmlReader.Create(@"..\..\..\TestFeeds\rss20-2items.xml", new XmlReaderSettings() { Async = true });
        await TestReadFeedElements(reader);
    }

    internal static async Task TestReadFeedElements(XmlReader outerXmlReader)
    {
        using var xmlReader = outerXmlReader;
        var reader = new RssFeedReader(xmlReader);
        var items = 0;
        while (await reader.Read())
        {
            switch (reader.ElementType)
            {
                case SyndicationElementType.Person:
                    ISyndicationPerson person = await reader.ReadPerson();
                    Assert.That(person.Email, Is.EqualTo("John Smith"));
                    break;

                case SyndicationElementType.Link:
                    ISyndicationLink link = await reader.ReadLink();
                    Assert.That(link.Length, Is.EqualTo(123));
                    Assert.That(link.MediaType, Is.EqualTo("testType"));
                    Assert.That(link.Uri.OriginalString, Is.EqualTo("http://example.com/"));
                    break;

                case SyndicationElementType.Image:
                    ISyndicationImage image = await reader.ReadImage();
                    Assert.That(image.Title, Is.EqualTo("Microsoft News"));
                    Assert.That(image.Description, Is.EqualTo("Test description"));
                    Assert.That(image.Url.OriginalString, Is.EqualTo("http://2.bp.blogspot.com/-NA5Jb-64eUg/URx8CSdcj_I/AAAAAAAAAUo/eCx0irI0rq0/s1600/bg_Microsoft_logo3-20120824073001907469-620x349.jpg"));
                    break;

                case SyndicationElementType.Item:
                    items++;
                    ISyndicationItem item = await reader.ReadItem();

                    if (items == 1)
                    {
                        Assert.That(item.Title, Is.EqualTo("Lorem ipsum 2017-07-06T20:25:00+00:00"));
                        Assert.That(item.Description, Is.EqualTo("Exercitation sit dolore mollit et est eiusmod veniam aute officia veniam ipsum."));
                        Assert.That(item.Links.Count() == 3);
                    }
                    else if (items == 2)
                    {
                        Assert.That(item.Title, Is.EqualTo("Lorem ipsum 2017-07-06T20:24:00+00:00"));
                        Assert.That(item.Description, Is.EqualTo("Do ipsum dolore veniam minim est cillum aliqua ea."));
                        Assert.That(item.Links.Count(), Is.EqualTo(3));
                    }

                    break;

                default:
                    break;
            }
        }
    }
}