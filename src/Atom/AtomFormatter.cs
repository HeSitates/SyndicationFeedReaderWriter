// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.SyndicationFeed.Atom.Extentions;
using Microsoft.SyndicationFeed.Extentions;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Utils;

namespace Microsoft.SyndicationFeed.Atom;

public class AtomFormatter : ISyndicationFeedFormatter
{
  private readonly XmlWriter _writer;
  private readonly StringBuilder _buffer;

  public AtomFormatter()
      : this(null, null)
  {
    // Intentionally left empty
  }

  public AtomFormatter(IEnumerable<ISyndicationAttribute> knownAttributes, XmlWriterSettings settings)
  {
    _buffer = new StringBuilder();
    _writer = XmlUtils.CreateXmlWriter(settings?.Clone() ?? new XmlWriterSettings(), EnsureAtomNs(knownAttributes ?? Enumerable.Empty<ISyndicationAttribute>()), _buffer);
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
    return Format(CreateContent(category));
  }

  public string Format(ISyndicationImage image)
  {
    return Format(CreateContent(image));
  }

  public string Format(ISyndicationPerson person)
  {
    return Format(CreateContent(person));
  }

  public string Format(ISyndicationItem item)
  {
    return Format(CreateContent(item));
  }

  public string Format(IAtomEntry entry)
  {
    return Format(CreateContent(entry));
  }

  public string Format(ISyndicationLink link)
  {
    return Format(CreateContent(link));
  }

  public virtual string FormatValue<T>(T value)
  {
    if (value == null)
    {
      return null;
    }

    Type type = typeof(T);

    if (type == typeof(DateTimeOffset))
    {
      return DateTimeUtils.ToRfc3339String((DateTimeOffset)(object)value);
    }

    if (type == typeof(DateTime))
    {
      return DateTimeUtils.ToRfc3339String(new DateTimeOffset((DateTime)(object)value));
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
      throw new ArgumentNullException(nameof(link), "Uri cannot be null.");
    }

    switch (link.RelationshipType)
    {
      case AtomLinkTypes.Content:
        return CreateFromContentLink(link);

      case AtomLinkTypes.Source:
        return CreateFromSourceLink(link);

      default:
        return CreateFromLink(link);
    }
  }

  public virtual ISyndicationContent CreateContent(ISyndicationCategory category)
  {
    if (category == null)
    {
      throw new ArgumentNullException(nameof(category));
    }

    if (string.IsNullOrEmpty(category.Name))
    {
      throw new ArgumentNullException(nameof(category), "Name");
    }

    var result = new SyndicationContent(AtomElementNames.Category);

    result.AddAttribute(new SyndicationAttribute(AtomConstants.Term, category.Name));

    if (!string.IsNullOrEmpty(category.Scheme))
    {
      result.AddAttribute(new SyndicationAttribute(AtomConstants.Scheme, category.Scheme));
    }

    if (!string.IsNullOrEmpty(category.Label))
    {
      result.AddAttribute(new SyndicationAttribute(AtomConstants.Label, category.Label));
    }

    return result;
  }

  public virtual ISyndicationContent CreateContent(ISyndicationPerson person)
  {
    if (person == null)
    {
      throw new ArgumentNullException(nameof(person));
    }

    if (string.IsNullOrEmpty(person.Name))
    {
      throw new ArgumentNullException(nameof(person), "Name");
    }

    string contributorType = person.RelationshipType ?? AtomContributorTypes.Author;

    if (contributorType != AtomContributorTypes.Author &&
        contributorType != AtomContributorTypes.Contributor)
    {
      throw new ArgumentException("RelationshipType");
    }

    var result = new SyndicationContent(contributorType);

    result.AddField(new SyndicationContent(AtomElementNames.Name, person.Name));

    if (!string.IsNullOrEmpty(person.Email))
    {
      result.AddField(new SyndicationContent(AtomElementNames.Email, person.Email));
    }

    if (person.Uri != null)
    {
      result.AddField(new SyndicationContent(AtomElementNames.Uri, FormatValue(person.Uri)));
    }

    return result;
  }

  public virtual ISyndicationContent CreateContent(ISyndicationImage image)
  {
    if (image == null)
    {
      throw new ArgumentNullException(nameof(image));
    }

    if (image.Url == null)
    {
      throw new ArgumentNullException(nameof(image), "Url");
    }

    return new SyndicationContent(!string.IsNullOrEmpty(image.RelationshipType) ? image.RelationshipType : AtomImageTypes.Icon, FormatValue(image.Url));
  }

  public virtual ISyndicationContent CreateContent(ISyndicationItem item)
  {
    if (item == null)
    {
      throw new ArgumentNullException(nameof(item));
    }

    if (string.IsNullOrEmpty(item.Id))
    {
      throw new ArgumentNullException(nameof(item), $"Missing required {item.Id}");
    }

    if (string.IsNullOrEmpty(item.Title))
    {
      throw new ArgumentNullException(nameof(item), $"Missing required {item.Title}");
    }

    if (item.LastUpdated == default)
    {
      throw new ArgumentException($"Invalid {nameof(item.LastUpdated)}", nameof(item));
    }

    var result = new SyndicationContent(AtomElementNames.Entry);

    result.AddField(new SyndicationContent(AtomElementNames.Id, item.Id));

    result.AddField(new SyndicationContent(AtomElementNames.Title, item.Title));

    result.AddField(new SyndicationContent(AtomElementNames.Updated, FormatValue(item.LastUpdated)));

    if (item.Published != default)
    {
      result.AddField(new SyndicationContent(AtomElementNames.Published, FormatValue(item.Published)));
    }

    var hasContentLink = false;
    var hasAlternateLink = false;

    if (item.Links != null)
    {
      foreach (var link in item.Links)
      {
        if (link.RelationshipType == AtomLinkTypes.Content)
        {
          if (hasContentLink)
          {
            throw new ArgumentException("Multiple content links are not allowed", nameof(item));
          }

          hasContentLink = true;
        }
        else if (link.RelationshipType == null || link.RelationshipType == AtomLinkTypes.Alternate)
        {
          hasAlternateLink = true;
        }

        result.AddField(CreateContent(link));
      }
    }

    var hasAuthor = false;

    if (item.Contributors != null)
    {
      foreach (var c in item.Contributors)
      {
        if (c.RelationshipType == null || c.RelationshipType == AtomContributorTypes.Author)
        {
          hasAuthor = true;
        }

        result.AddField(CreateContent(c));
      }
    }

    if (!hasAuthor)
    {
      throw new ArgumentException("Author is required");
    }

    if (item.Categories != null)
    {
      foreach (var category in item.Categories)
      {
        result.AddField(CreateContent(category));
      }
    }

    IAtomEntry entry = item as IAtomEntry;

    if (!string.IsNullOrEmpty(item.Description))
    {
      if (hasContentLink)
      {
        throw new ArgumentException("Description and content link are not allowed simultaneously");
      }

      var content = new SyndicationContent(AtomElementNames.Content, item.Description);

      if (entry != null && !(string.IsNullOrEmpty(entry.ContentType) || entry.ContentType.Equals(AtomConstants.PlainTextContentType, StringComparison.OrdinalIgnoreCase)))
      {
        content.AddAttribute(new SyndicationAttribute(AtomConstants.Type, entry.ContentType));
      }

      result.AddField(content);
    }
    else
    {
      if (!(hasContentLink || hasAlternateLink))
      {
        throw new ArgumentException("Description or alternate link is required");
      }
    }

    if (entry != null)
    {
      // summary
      if (!string.IsNullOrEmpty(entry.Summary))
      {
        result.AddField(new SyndicationContent(AtomElementNames.Summary, entry.Summary));
      }

      // rights
      if (!string.IsNullOrEmpty(entry.Rights))
      {
        result.AddField(new SyndicationContent(AtomElementNames.Rights, entry.Rights));
      }
    }

    return result;
  }

  internal ISyndicationContent CreateFromLink(ISyndicationLink link)
  {
    var result = new SyndicationContent(AtomElementNames.Link);

    if (!string.IsNullOrEmpty(link.Title))
    {
      result.AddAttribute(new SyndicationAttribute(AtomElementNames.Title, link.Title));
    }

    result.AddAttribute(new SyndicationAttribute(AtomConstants.Href, FormatValue(link.Uri)));

    if (!string.IsNullOrEmpty(link.RelationshipType))
    {
      result.AddAttribute(new SyndicationAttribute(AtomConstants.Rel, link.RelationshipType));
    }

    if (!string.IsNullOrEmpty(link.MediaType))
    {
      result.AddAttribute(new SyndicationAttribute(AtomConstants.Type, link.MediaType));
    }

    if (link.Length > 0)
    {
      result.AddAttribute(new SyndicationAttribute(AtomConstants.Length, FormatValue(link.Length)));
    }

    return result;
  }

  internal ISyndicationContent CreateFromContentLink(ISyndicationLink link)
  {
    var result = new SyndicationContent(AtomElementNames.Content);

    result.AddAttribute(new SyndicationAttribute(AtomConstants.Source, FormatValue(link.Uri)));

    if (!string.IsNullOrEmpty(link.MediaType))
    {
      result.AddAttribute(new SyndicationAttribute(AtomConstants.Type, link.MediaType));
    }

    return result;
  }

  internal ISyndicationContent CreateFromSourceLink(ISyndicationLink link)
  {
    var result = new SyndicationContent(AtomElementNames.Source);

    if (!string.IsNullOrEmpty(link.Title))
    {
      result.AddField(new SyndicationContent(AtomElementNames.Title, link.Title));
    }

    result.AddField(CreateFromLink(new SyndicationLink(link.Uri)
    {
      MediaType = link.MediaType,
      Length = link.Length,
    }));

    if (link.LastUpdated != default(DateTimeOffset))
    {
      result.AddField(new SyndicationContent(AtomElementNames.Updated, FormatValue(link.LastUpdated)));
    }

    return result;
  }

  private static IEnumerable<ISyndicationAttribute> EnsureAtomNs(IEnumerable<ISyndicationAttribute> attributes)
  {
    // Insert Atom namespace if it doesn't already exist
    if (!attributes.Any(a => a.Name.StartsWith("xmlns") && a.Value == AtomConstants.Atom10Namespace))
    {
      var list = new List<ISyndicationAttribute>(attributes);
      list.Insert(0, new SyndicationAttribute("xmlns", AtomConstants.Atom10Namespace));

      attributes = list;
    }

    return attributes;
  }

  private void WriteSyndicationContent(ISyndicationContent content)
  {
    string type = null;

    _writer.WriteStartSyndicationContent(content, AtomConstants.Atom10Namespace);

    if (content.Attributes != null)
    {
      foreach (var a in content.Attributes)
      {
        if (type == null && a.Name == AtomConstants.Type)
        {
          type = a.Value;
        }

        _writer.WriteSyndicationAttribute(a);
      }
    }

    if (content.Value != null)
    {
      if (XmlUtils.IsXhtmlMediaType(type) && content.IsAtom())
      {
        _writer.WriteStartElement("div", AtomConstants.XhtmlNamespace);
        _writer.WriteXmlFragment(content.Value, AtomConstants.XhtmlNamespace);
        _writer.WriteEndElement();
      }
      else if (XmlUtils.IsXmlMediaType(type) && content.IsAtom(AtomElementNames.Content))
      {
        _writer.WriteXmlFragment(content.Value, string.Empty);
      }
      else
      {
        _writer.WriteString(content.Value, UseCDATA);
      }
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
}