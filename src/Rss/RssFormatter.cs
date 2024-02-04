// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using Microsoft.SyndicationFeed.Extentions;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Utils;

namespace Microsoft.SyndicationFeed.Rss;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global", Justification = "It is allowed to overwrite them...")]
public class RssFormatter : ISyndicationFeedFormatter, IDisposable
{
  private readonly XmlWriter _writer;
  private readonly StringBuilder _buffer;

  public RssFormatter()
      : this(null, null)
  {
  }

  public RssFormatter(IEnumerable<ISyndicationAttribute> knownAttributes, XmlWriterSettings settings)
  {
    _buffer = new StringBuilder();
    _writer = XmlUtils.CreateXmlWriter(settings?.Clone() ?? new XmlWriterSettings(), knownAttributes, _buffer);
  }

  public bool UseCDATA { get; set; }

  public string Format(ISyndicationContent content)
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    try
    {
      WriteSyndicationContent(content);

      _writer.Flush();

      return _buffer.ToString();
    }
    finally
    {
      _buffer.Clear();
    }
  }

  public string Format(ISyndicationCategory category)
  {
    var content = CreateContent(category);

    return Format(content);
  }

  public string Format(ISyndicationImage image)
  {
    var content = CreateContent(image);

    return Format(content);
  }

  public string Format(ISyndicationPerson person)
  {
    var content = CreateContent(person);

    return Format(content);
  }

  public string Format(ISyndicationItem item)
  {
    var content = CreateContent(item);

    return Format(content);
  }

  public string Format(ISyndicationLink link)
  {
    var content = CreateContent(link);

    return Format(content);
  }

  public virtual string FormatValue<T>(T value)
  {
    if (value == null)
    {
      return null;
    }

    var type = typeof(T);

    if (type == typeof(DateTimeOffset))
    {
      return DateTimeUtils.ToRfc1123String((DateTimeOffset)(object)value);
    }

    if (type == typeof(DateTime))
    {
      return DateTimeUtils.ToRfc1123String(new DateTimeOffset((DateTime)(object)value));
    }

    return value.ToString();
  }

  public virtual ISyndicationContent CreateContent(ISyndicationLink link)
  {
    if (link == null)
    {
      throw new ArgumentNullException(nameof(link));
    }

    if (link.Uri == null)
    {
      throw new ArgumentNullException(nameof(link), "Invalid link uri");
    }

    return link.RelationshipType switch
    {
      RssElementNames.Enclosure => CreateEnclosureContent(link),
      RssElementNames.Comments => CreateCommentsContent(link),
      RssElementNames.Source => CreateSourceContent(link),
      _ => CreateLinkContent(link),
    };
  }

  public virtual ISyndicationContent CreateContent(ISyndicationCategory category)
  {
    if (category == null)
    {
      throw new ArgumentNullException(nameof(category));
    }

    if (string.IsNullOrEmpty(category.Name))
    {
      throw new FormatException("Invalid category name");
    }

    var content = new SyndicationContent(RssElementNames.Category, category.Name);

    if (category.Scheme != null)
    {
      content.AddAttribute(new SyndicationAttribute(RssConstants.Domain, category.Scheme));
    }

    return content;
  }

  public virtual ISyndicationContent CreateContent(ISyndicationPerson person)
  {
    if (person == null)
    {
      throw new ArgumentNullException(nameof(person));
    }

    // RSS requires Email
    if (string.IsNullOrEmpty(person.Email))
    {
      throw new ArgumentNullException(nameof(person), "Invalid person Email");
    }

    // Real name recommended with RSS e-mail addresses
    // Ex: <author>email@address.com (John Doe)</author>
    var value = string.IsNullOrEmpty(person.Name) ? person.Email : $"{person.Email} ({person.Name})";

    return new SyndicationContent(person.RelationshipType ?? RssElementNames.Author, value);
  }

  public virtual ISyndicationContent CreateContent(ISyndicationImage image)
  {
    if (image == null)
    {
      throw new ArgumentNullException(nameof(image));
    }

    if (string.IsNullOrEmpty(image.Title))
    {
      throw new ArgumentNullException(nameof(image), "Image requires a title");
    }

    if (image.Link == null)
    {
      throw new ArgumentNullException(nameof(image), "Image requires a link");
    }

    if (image.Url == null)
    {
      throw new ArgumentNullException(nameof(image), "Image requires an url");
    }

    var content = new SyndicationContent(RssElementNames.Image);

    content.AddField(new SyndicationContent(RssElementNames.Url, FormatValue(image.Url)));
    content.AddField(new SyndicationContent(RssElementNames.Title, image.Title));
    content.AddField(CreateContent(image.Link));

    if (!string.IsNullOrEmpty(image.Description))
    {
      content.AddField(new SyndicationContent(RssElementNames.Description, image.Description));
    }

    return content;
  }

  public virtual ISyndicationContent CreateContent(ISyndicationItem item)
  {
    if (item == null)
    {
      throw new ArgumentNullException(nameof(item));
    }

    if (string.IsNullOrEmpty(item.Title) && string.IsNullOrEmpty(item.Description))
    {
      throw new ArgumentNullException(nameof(item), "RSS Item requires a title or a description");
    }

    var content = new SyndicationContent(RssElementNames.Item);

    CreateAndAddField(content, RssElementNames.Title, item.Title);

    var guidLink = CreateAndAddLinks(content, item);

    CreateAndAddField(content, RssElementNames.Description, item.Description);

    CreateAndAddContents(content, item.Contributors, CreateContent);

    CreateAndAddContents(content, item.Categories, CreateContent);

    CreateAndAddGuidLink(content, guidLink, item.Id);

    if (item.Published != DateTimeOffset.MinValue)
    {
      content.AddField(new SyndicationContent(RssElementNames.PubDate, FormatValue(item.Published)));
    }

    return content;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  internal ISyndicationContent CreateEnclosureContent(ISyndicationLink link)
  {
    var content = new SyndicationContent(RssElementNames.Enclosure);

    content.AddAttribute(new SyndicationAttribute(RssElementNames.Url, FormatValue(link.Uri)));

    if (link.Length == 0)
    {
      throw new ArgumentException("Enclosure requires length attribute", nameof(link));
    }

    content.AddAttribute(new SyndicationAttribute(RssConstants.Length, FormatValue(link.Length)));

    if (string.IsNullOrEmpty(link.MediaType))
    {
      throw new ArgumentNullException(nameof(link), "Enclosure requires a MediaType");
    }

    content.AddAttribute(new SyndicationAttribute(RssConstants.Type, link.MediaType));
    return content;
  }

  internal ISyndicationContent CreateCommentsContent(ISyndicationLink link)
  {
    return new SyndicationContent(link.RelationshipType)
    {
      Value = FormatValue(link.Uri),
    };
  }

  internal ISyndicationContent CreateSourceContent(ISyndicationLink link)
  {
    var content = new SyndicationContent(link.RelationshipType);

    var url = FormatValue(link.Uri);
    if (link.Title != url)
    {
      content.AddAttribute(new SyndicationAttribute(RssElementNames.Url, url));
    }

    if (!string.IsNullOrEmpty(link.Title))
    {
      content.Value = link.Title;
    }

    return content;
  }

  internal ISyndicationContent CreateLinkContent(ISyndicationLink link)
  {
    SyndicationContent content;

    if (string.IsNullOrEmpty(link.RelationshipType) || link.RelationshipType == RssLinkTypes.Alternate)
    {
      content = new SyndicationContent(RssElementNames.Link);
    }
    else
    {
      content = new SyndicationContent(link.RelationshipType);
    }

    if (!string.IsNullOrEmpty(link.Title))
    {
      content.Value = link.Title;
    }

    var url = FormatValue(link.Uri);

    if (content.Value != null)
    {
      content.AddAttribute(new SyndicationAttribute(RssElementNames.Url, url));
    }
    else
    {
      content.Value = url;
    }

    if (!string.IsNullOrEmpty(link.MediaType))
    {
      content.AddAttribute(new SyndicationAttribute(RssConstants.Type, link.MediaType));
    }

    if (link.Length != 0)
    {
      content.AddAttribute(new SyndicationAttribute(RssConstants.Length, FormatValue(link.Length)));
    }

    return content;
  }

  protected virtual void Dispose(bool disposing)
  {
    if (disposing)
    {
      _writer?.Dispose();
    }
  }

  private static void CreateAndAddField(SyndicationContent content, string rssElementName, string value)
  {
    if (string.IsNullOrEmpty(value))
    {
      return;
    }

    content.AddField(new SyndicationContent(rssElementName, value));
  }

  private static void CreateAndAddContents<T>(SyndicationContent content, IEnumerable<T> items, Func<T, ISyndicationContent> action)
  {
    if (items != null)
    {
      foreach (var person in items)
      {
        content.AddField(action(person));
      }
    }
  }

  private static void CreateAndAddGuidLink(SyndicationContent content, ISyndicationLink guidLink, string itemId)
  {
    if (guidLink != null || string.IsNullOrEmpty(itemId))
    {
      return;
    }

    var guid = new SyndicationContent(RssElementNames.Guid, itemId);

    guid.AddAttribute(new SyndicationAttribute(RssConstants.IsPermaLink, "false"));

    content.AddField(guid);
  }

  private void WriteSyndicationContent(ISyndicationContent content)
  {
    _writer.WriteStartSyndicationContent(content, null);

    if (content.Attributes != null)
    {
      foreach (var a in content.Attributes)
      {
        _writer.WriteSyndicationAttribute(a);
      }
    }

    if (content.Value != null)
    {
      _writer.WriteString(content.Value, UseCDATA);
    }
    else
    {
      if (content.Fields != null)
      {
        foreach (var field in content.Fields)
        {
          WriteSyndicationContent(field);
        }
      }
    }

    _writer.WriteEndElement();
  }

  private ISyndicationLink CreateAndAddLinks(SyndicationContent content, ISyndicationItem item)
  {
    if (item.Links == null)
    {
      return null;
    }

    ISyndicationLink guidLink = null;
    foreach (var link in item.Links)
    {
      if (link.RelationshipType == RssElementNames.Guid)
      {
        guidLink = link;
      }

      content.AddField(CreateContent(link));
    }

    return guidLink;
  }
}