// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Interfaces;

/// <summary>
/// Consumes an entire atom feed using the AtomFeedReader.
/// </summary>
public class AtomFeedReaderExample
{
  public static async Task ReadAtomFeed(string filePath)
  {
    // Create an XmlReader from file
    // Example: ..\tests\TestFeeds\simpleAtomFeed.xml
    using (var xmlReader = XmlReader.Create(filePath, new XmlReaderSettings() { Async = true }))
    {
      // Create an AtomFeedReader
      var reader = new AtomFeedReader(xmlReader);

      // Read the feed
      while (await reader.Read())
      {
        // Check the type of the current element.
        switch (reader.ElementType)
        {
          // Read category
          case SyndicationElementType.Category:
            ISyndicationCategory category = await reader.ReadCategory();
            Debug.WriteLine(category.Name);
            break;

          // Read image
          case SyndicationElementType.Image:
            ISyndicationImage image = await reader.ReadImage();
            Debug.WriteLine(image.Url);
            break;

          // Read entry 
          case SyndicationElementType.Item:
            IAtomEntry entry = await reader.ReadEntry();
            Debug.WriteLine(entry.Summary);
            break;

          // Read link
          case SyndicationElementType.Link:
            ISyndicationLink link = await reader.ReadLink();
            Debug.WriteLine(link.Uri);
            break;

          // Read person
          case SyndicationElementType.Person:
            ISyndicationPerson person = await reader.ReadPerson();
            Debug.WriteLine(person.Name);
            break;

          // Read content
          default:
            ISyndicationContent content = await reader.ReadContent();
            Debug.WriteLine(content.Name);
            break;
        }
      }
    }
  }
}