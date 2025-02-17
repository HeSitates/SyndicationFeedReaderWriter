﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Utils;

namespace Microsoft.SyndicationFeed.Rss;

public class RssFeedReader : XmlFeedReader
{
  private readonly XmlReader _reader;
  private bool _knownFeed;

  public RssFeedReader(XmlReader reader)
      : this(reader, new RssParser())
  {
  }

  public RssFeedReader(XmlReader reader, ISyndicationFeedParser parser)
      : base(reader, parser)
  {
    _reader = reader;
  }

  public override async Task<bool> Read()
  {
    if (!_knownFeed)
    {
      await InitRead();
      _knownFeed = true;
    }

    return await base.Read();
  }

  protected override SyndicationElementType MapElementType(string elementName)
  {
    if (_reader.NamespaceURI != RssConstants.Rss20Namespace)
    {
      return SyndicationElementType.Content;
    }

    switch (elementName)
    {
      case RssElementNames.Item:
        return SyndicationElementType.Item;

      case RssElementNames.Link:
        return SyndicationElementType.Link;

      case RssElementNames.Category:
        return SyndicationElementType.Category;

      case RssElementNames.Author:
      case RssElementNames.ManagingEditor:
        return SyndicationElementType.Person;

      case RssElementNames.Image:
        return SyndicationElementType.Image;

      default:
        return SyndicationElementType.Content;
    }
  }

  private async Task InitRead()
  {
    // Check <rss>
    var isKnownFeed = _reader.IsStartElement(RssElementNames.Rss, RssConstants.Rss20Namespace) &&
                    (_reader.GetAttribute(RssElementNames.Version)?.Equals(RssConstants.Version) ?? false);

    if (isKnownFeed)
    {
      // Read<rss>
      await XmlUtils.ReadAsync(_reader);

      // Check <channel>
      isKnownFeed = _reader.IsStartElement(RssElementNames.Channel, RssConstants.Rss20Namespace);
    }

    if (!isKnownFeed)
    {
      throw new XmlException("Unknown Rss Feed");
    }

    // Read <channel>
    await XmlUtils.ReadAsync(_reader);
  }
}