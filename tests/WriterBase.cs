using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests;

public class WriterBase
{
  protected static readonly XmlWriterSettings XmlWriterSettings = new() { Async = true };

  protected sealed class StringWriterWithEncoding : StringWriter
  {
    public StringWriterWithEncoding(Encoding encoding)
    {
      this.Encoding = encoding;
    }

    public override Encoding Encoding { get; }
  }
}