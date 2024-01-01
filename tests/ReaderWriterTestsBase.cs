using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Rss;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests;

public class ReaderWriterTestsBase
{
  protected static async Task TestReadFeedElements(XmlReader outerXmlReader)
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

          switch (items)
          {
            case 1:
              Assert.Multiple(() =>
              {
                Assert.That(item.Title, Is.EqualTo("Lorem ipsum 2017-07-06T20:25:00+00:00"));
                Assert.That(item.Description, Is.EqualTo("Exercitation sit dolore mollit et est eiusmod veniam aute officia veniam ipsum."));
                Assert.That(item.Links.Count(), Is.EqualTo(3));
              });
              break;
            case 2:
              Assert.Multiple(() =>
              {
                Assert.That(item.Title, Is.EqualTo("Lorem ipsum 2017-07-06T20:24:00+00:00"));
                Assert.That(item.Description, Is.EqualTo("Do ipsum dolore veniam minim est cillum aliqua ea."));
                Assert.That(item.Links.Count(), Is.EqualTo(3));
              });
              break;
          }

          break;

        default:
          break;
      }
    }
  }

  protected sealed class StringWriterWithEncoding(Encoding encoding) : StringWriter
  {
    public override Encoding Encoding { get; } = encoding;
  }
}
