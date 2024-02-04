// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Microsoft.SyndicationFeed.Extentions;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Utils;

namespace Microsoft.SyndicationFeed.Rss;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global", Justification = "It is allowed to overwrite them...")]
public class RssParser : ISyndicationFeedParser
{
  public ISyndicationCategory ParseCategory(string value)
  {
    var content = ParseContent(value);

    if (content.Name != RssElementNames.Category || content.Namespace != RssConstants.Rss20Namespace)
    {
      throw new FormatException("Invalid Rss category");
    }

    return CreateCategory(content);
  }

  public ISyndicationItem ParseItem(string value)
  {
    var content = ParseContent(value);

    if (content.Name != RssElementNames.Item || content.Namespace != RssConstants.Rss20Namespace)
    {
      throw new FormatException("Invalid Rss item");
    }

    return CreateItem(content);
  }

  public ISyndicationLink ParseLink(string value)
  {
    var content = ParseContent(value);

    if (content.Name != RssElementNames.Link || content.Namespace != RssConstants.Rss20Namespace)
    {
      throw new FormatException("Invalid Rss link");
    }

    return CreateLink(content);
  }

  public ISyndicationPerson ParsePerson(string value)
  {
    var content = ParseContent(value);

    if ((content.Name != RssElementNames.Author && content.Name != RssElementNames.ManagingEditor) || content.Namespace != RssConstants.Rss20Namespace)
    {
      throw new FormatException("Invalid Rss Person");
    }

    return CreatePerson(content);
  }

  public ISyndicationImage ParseImage(string value)
  {
    var content = ParseContent(value);

    if (content.Name != RssElementNames.Image || content.Namespace != RssConstants.Rss20Namespace)
    {
      throw new FormatException("Invalid Rss Image");
    }

    return CreateImage(content);
  }

  public ISyndicationContent ParseContent(string value)
  {
    if (string.IsNullOrEmpty(value))
    {
      throw new ArgumentNullException(nameof(value));
    }

    using var reader = XmlUtils.CreateXmlReader(value);
    reader.MoveToContent();

    return ReadSyndicationContent(reader);
  }

  public virtual bool TryParseValue<T>(string value, out T result)
  {
    return Converter.TryParseValue(value, out result);
  }

  public virtual ISyndicationItem CreateItem(ISyndicationContent content)
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    var item = new SyndicationItem();

    foreach (var field in content.Fields)
    {
      if (field.Namespace != RssConstants.Rss20Namespace)
      {
        continue;
      }

      switch (field.Name)
      {
        case RssElementNames.Title:
          item.Title = field.Value;
          break;

        case RssElementNames.Link:
          item.AddLink(CreateLink(field));
          break;

        case RssElementNames.Description:
          item.Description = field.Value;
          break;

        case RssElementNames.Author:
          item.AddContributor(CreatePerson(field));
          break;

        case RssElementNames.Category:
          item.AddCategory(CreateCategory(field));
          break;

        case RssElementNames.Comments:
        case RssElementNames.Enclosure:
        case RssElementNames.Source:
          item.AddLink(CreateLink(field));
          break;

        case RssElementNames.Guid:
          item.Id = field.Value;

          var isPermaLinkAttr = field.Attributes.GetRss(RssConstants.IsPermaLink);

          if ((isPermaLinkAttr == null || (TryParseValue(isPermaLinkAttr, out bool isPermalink) && isPermalink)) &&
              TryParseValue(field.Value, out Uri permaLink))
          {
            item.AddLink(new SyndicationLink(permaLink, RssLinkTypes.Guid));
          }

          break;

        case RssElementNames.PubDate:
          if (TryParseValue(field.Value, out DateTimeOffset dt))
          {
            item.Published = dt;
          }

          break;

        default:
          break;
      }
    }

    return item;
  }

  public virtual ISyndicationLink CreateLink(ISyndicationContent content)
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    string title = content.Value;

    Uri uri;
    var url = content.Attributes.GetRss("url");

    if (url != null)
    {
      if (!TryParseValue(url, out uri))
      {
        throw new FormatException("Invalid url attribute");
      }
    }
    else
    {
      if (!TryParseValue(content.Value, out uri))
      {
        throw new FormatException("Invalid url");
      }

      title = null;
    }

    TryParseValue(content.Attributes.GetRss("length"), out long length);

    var type = content.Attributes.GetRss("type");

    var rel = (content.Name == RssElementNames.Link) ? RssLinkTypes.Alternate : content.Name;

    return new SyndicationLink(uri, rel)
    {
      Title = title,
      Length = length,
      MediaType = type,
    };
  }

  public virtual ISyndicationPerson CreatePerson(ISyndicationContent content)
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    if (string.IsNullOrEmpty(content.Value))
    {
      throw new ArgumentNullException(nameof(content), "Content value is required");
    }

    // Handle real name parsing
    // Ex: <author>abc@def.com (John Doe)</author>
    var email = content.Value;
    string name = null;

    var nameStart = content.Value.IndexOf('(');

    if (nameStart != -1)
    {
      var end = content.Value.IndexOf(')');

      if (end == -1 || end - nameStart - 1 < 0)
      {
        throw new FormatException("Invalid Rss person");
      }

      email = content.Value.Substring(0, nameStart).Trim();

      name = content.Value.Substring(nameStart + 1, end - nameStart - 1);
    }

    return new SyndicationPerson(name, email, content.Name);
  }

  public virtual ISyndicationImage CreateImage(ISyndicationContent content)
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    string title = null;
    string description = null;
    Uri url = null;
    ISyndicationLink link = null;

    foreach (var field in content.Fields)
    {
      if (field.Namespace != RssConstants.Rss20Namespace)
      {
        continue;
      }

      switch (field.Name)
      {
        case RssElementNames.Title:
          title = field.Value;
          break;

        case RssElementNames.Url:
          if (!TryParseValue(field.Value, out url))
          {
            throw new FormatException($"Invalid image url '{field.Value}'");
          }

          break;

        case RssElementNames.Link:
          link = CreateLink(field);
          break;

        case RssElementNames.Description:
          description = field.Value;
          break;

        default:
          break;
      }
    }

    if (url == null)
    {
      throw new FormatException("Image url not found");
    }

    return new SyndicationImage(url, RssElementNames.Image)
    {
      Title = title,
      Description = description,
      Link = link,
    };
  }

  public virtual ISyndicationCategory CreateCategory(ISyndicationContent content)
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    if (content.Value == null)
    {
      throw new FormatException("Invalid Rss category name");
    }

    return new SyndicationCategory(content.Value)
    {
      Scheme = content.Attributes.GetRss(RssConstants.Domain),
    };
  }

  private static ISyndicationContent ReadSyndicationContent(XmlReader reader)
  {
    var content = new SyndicationContent(reader.LocalName, reader.NamespaceURI, null);

    if (reader.HasAttributes)
    {
      while (reader.MoveToNextAttribute())
      {
        var attr = reader.ReadSyndicationAttribute();

        if (attr != null)
        {
          content.AddAttribute(attr);
        }
      }

      reader.MoveToContent();
    }

    if (!reader.IsEmptyElement)
    {
      reader.ReadStartElement();

      if (reader.HasValue)
      {
        content.Value = reader.ReadContentAsString();
      }
      else
      {
        while (reader.IsStartElement())
        {
          content.AddField(ReadSyndicationContent(reader));
        }
      }

      reader.ReadEndElement();
    }
    else
    {
      reader.Skip();
    }

    return content;
  }
}