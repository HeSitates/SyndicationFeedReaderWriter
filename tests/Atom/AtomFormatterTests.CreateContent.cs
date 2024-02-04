// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions;
using Moq;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Atom;

public partial class AtomFormatterTests
{
  [TestCaseSource(nameof(CreateInvalidContentData))]
  public void CreateContentSyndicationItemShouldHaveTitleOrDescription(ISyndicationItem syndicationItem, Type expectedExceptionType, string expectedExceptionMessage)
  {
    var sut = new AtomFormatter();

    var ex = Assert.Throws(expectedExceptionType, () => sut.CreateContent(syndicationItem));
    Assert.That(ex, Is.Not.Null);
    Assert.That(ex.Message, Is.EqualTo(expectedExceptionMessage));
  }

  [TestCaseSource(nameof(CreateContentValidData))]
  public void CreateContentSyndicationItemShouldNotThrowAnException(bool useConstructorWithParams, ISyndicationItem syndicationItem, string expectedResult)
  {
    AtomFormatter sut;
    if (useConstructorWithParams)
    {
      List<SyndicationAttribute> knownAttributes = [new SyndicationAttribute("xmlns:example", TestFeedResources.ExampleNs)];

      var settings = new XmlWriterSettings { Indent = true };

      sut = new AtomFormatter(knownAttributes, settings);
    }
    else
    {
      sut = new AtomFormatter();
    }

    var result = sut.CreateContent(syndicationItem);
    Assert.That(result, Is.Not.Null);
    Assert.That(result, Is.TypeOf<SyndicationContent>());
    var serialized = result.Serialize(true);
    TestContext.WriteLine(serialized);
    Assert.That(serialized, Is.EqualTo(expectedResult));
  }

  [TestCaseSource(nameof(CreateContentLinkMissingUriData))]
  public void CreateContentWithSyndicationLinkShouldHaveTitleOrDescription(Type exceptionType, ISyndicationLink syndicationLink, string expectedExceptionMessage)
  {
    var sut = new AtomFormatter();

    var ex = Assert.Throws(exceptionType, () => sut.CreateContent(syndicationLink));
    Assert.That(ex, Is.Not.Null);
    Assert.That(ex.Message, Is.EqualTo(expectedExceptionMessage));
  }

  private static IEnumerable CreateInvalidContentData()
  {
    yield return new TestCaseData(null, typeof(ArgumentNullException), "Value cannot be null. (Parameter 'item')");
    yield return new TestCaseData(new SyndicationItem(), typeof(ArgumentNullException), "Missing required  (Parameter 'item')");
    yield return new TestCaseData(SyndicationItemBuilder.Create().Build(), typeof(ArgumentNullException), "Missing required  (Parameter 'item')");
    yield return new TestCaseData(SyndicationItemBuilder.Create().WithTitle().Build(), typeof(ArgumentNullException), "Missing required  (Parameter 'item')");
    yield return new TestCaseData(SyndicationItemBuilder.Create().WithId().Build(), typeof(ArgumentNullException), "Missing required  (Parameter 'item')");
    yield return new TestCaseData(SyndicationItemBuilder.Create().WithTitleAndId().Build(), typeof(ArgumentException), "Invalid LastUpdated (Parameter 'item')");
    yield return new TestCaseData(SyndicationItemBuilder.Create().WithTitleIdAndValidDate().Build(), typeof(ArgumentException), "Author is required");
    yield return new TestCaseData(SyndicationItemBuilder.Create().WithTitleIdAndValidDate().AddContributor().Build(), typeof(ArgumentException), "Description or alternate link is required");
  }

  private static IEnumerable CreateContentLinkMissingUriData()
  {
    yield return new TestCaseData(typeof(ArgumentNullException), null, "Value cannot be null. (Parameter 'link')");
    var syndicationLinkMock = new Mock<ISyndicationLink>();
    yield return new TestCaseData(typeof(ArgumentNullException), syndicationLinkMock.Object, "Uri cannot be null. (Parameter 'link')");
  }

  private static IEnumerable CreateContentValidData()
  {
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().MinimalBuild(), AtomFormatterResources.CreateContentWithTitleOnly).SetName("With minimal");
  }

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "Leave for now.")]
  [SuppressMessage("Minor Code Smell", "S3241:Methods should not return values that are never used", Justification = "<Pending>")]
  private class SyndicationItemBuilder
  {
    private readonly SyndicationItem _item;

    private SyndicationItemBuilder()
    {
      _item = new SyndicationItem();
    }

    public static SyndicationItemBuilder Create()
    {
      var builder = new SyndicationItemBuilder();
      return builder;
    }

    public SyndicationItemBuilder WithTitle(string title = "ATitle")
    {
      _item.Title = title;
      return this;
    }

    public SyndicationItemBuilder WithDescription(string description = "ADescription")
    {
      _item.Description = description;
      return this;
    }

    public SyndicationItemBuilder WithId(string id = "AnId")
    {
      _item.Id = id;
      return this;
    }

    public SyndicationItemBuilder WithLastUpdated(DateTime? newDateTime = null)
    {
      if (newDateTime.GetValueOrDefault() == default)
      {
        _item.LastUpdated = new DateTimeOffset(new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Local));
      }

      return this;
    }

    public SyndicationItemBuilder AddContributor(SyndicationPerson person = null)
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      person ??= new SyndicationPerson("APerson", "a@b.nl", AtomContributorTypes.Author);

      _item.AddContributor(person);
      return this;
    }

    public SyndicationItemBuilder WithTitleAndId()
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      if (string.IsNullOrWhiteSpace(_item.Id))
      {
        WithId();
      }

      return this;
    }

    public SyndicationItemBuilder WithTitleIdAndValidDate()
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      if (string.IsNullOrWhiteSpace(_item.Id))
      {
        WithId();
      }

      if (_item.LastUpdated == default)
      {
        WithLastUpdated();
      }

      return this;
    }

    public SyndicationItem Build()
    {
      return _item;
    }

    public SyndicationItem MinimalBuild()
    {
      WithTitleIdAndValidDate();

      if (_item.Contributors.All(c => c.RelationshipType != AtomContributorTypes.Author))
      {
        AddContributor();
      }

      if (string.IsNullOrWhiteSpace(_item.Description))
      {
        WithDescription();
      }

      return _item;
    }
  }
}
