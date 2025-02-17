﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Utils;

namespace Microsoft.SyndicationFeed.Atom;

public class AtomFeedReader : XmlFeedReader
{
  private readonly XmlReader _reader;
  private bool _knownFeed;

  public AtomFeedReader(XmlReader reader)
      : this(reader, new AtomParser())
  {
  }

  public AtomFeedReader(XmlReader reader, ISyndicationFeedParser parser)
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

  public virtual async Task<IAtomEntry> ReadEntry()
  {
    if (await ReadItem() is not IAtomEntry item)
    {
      throw new FormatException("Invalid Atom entry");
    }

    return item;
  }

  protected override SyndicationElementType MapElementType(string elementName)
  {
    if (_reader.NamespaceURI != AtomConstants.Atom10Namespace)
    {
      return SyndicationElementType.Content;
    }

    switch (elementName)
    {
      case AtomElementNames.Entry:
        return SyndicationElementType.Item;

      case AtomElementNames.Link:
        return SyndicationElementType.Link;

      case AtomElementNames.Category:
        return SyndicationElementType.Category;

      case AtomElementNames.Logo:
      case AtomElementNames.Icon:
        return SyndicationElementType.Image;

      case AtomContributorTypes.Author:
      case AtomContributorTypes.Contributor:
        return SyndicationElementType.Person;

      default:
        return SyndicationElementType.Content;
    }
  }

  private async Task InitRead()
  {
    // Check <feed>
    if (_reader.IsStartElement(AtomElementNames.Feed, AtomConstants.Atom10Namespace))
    {
      // Read <feed>
      await XmlUtils.ReadAsync(_reader);
    }
    else
    {
      throw new XmlException("Unknown Atom Feed");
    }
  }
}
