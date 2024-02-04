// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Linq;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Interfaces;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Atom;

public partial class AtomFormatterTests
{
  [TestCase(null, "Value cannot be null. (Parameter 'link')")]
  public void CreateContentShouldThrowExceptionIfLinkIsInvalid(ISyndicationLink link, string expectedMessage)
  {
    var sut = new AtomFormatter();
    var ex = Assert.Throws<ArgumentNullException>(() => sut.CreateContent(link));
    TestContext.WriteLine(ex.Message);
    Assert.That(ex, Is.Not.Null.And.Message.EqualTo(expectedMessage));
  }

  [TestCaseSource(nameof(CreateLinkValidData))]
  public void CreateContentShouldThrowExceptionIfLinkIsValid(ISyndicationLink link, int expectedNrOfAttributes)
  {
    var sut = new AtomFormatter();
    var content = sut.CreateContent(link);
    Assert.That(content, Is.Not.Null);
    var attributes = content.Attributes.ToArray();
    Assert.That(attributes, Has.Length.EqualTo(expectedNrOfAttributes));
    Assert.That(attributes[0].Name, Is.EqualTo("href"));
    Assert.That(attributes[0].Value, Is.EqualTo(AtomFormatterResources.UrlCortosoPodcast));
  }

  private static IEnumerable CreateLinkValidData()
  {
    yield return new TestCaseData(SyndicationLinkBuilder.CreateLink().Build(), 1);
  }

  internal class SyndicationLinkBuilder
  {
    private readonly SyndicationLink _item;

    private SyndicationLinkBuilder(Uri uri, string linkTypes)
    {
      _item = new SyndicationLink(uri, linkTypes);
      //if (linkTypes == AtomLinkTypes.Source)
      //{
      //  _item.Title = uri.AbsoluteUri;
      //}
    }

    public static SyndicationLinkBuilder CreateLink(string linkType = "", Uri uri = null)
    {
      if (uri == null)
      {
        uri = new Uri(AtomFormatterResources.UrlCortosoPodcast);
      }

      var builder = new SyndicationLinkBuilder(uri, linkType);
      return builder;
    }

    public SyndicationLinkBuilder WithTitle(string title = "ATitle")
    {
      _item.Title = title;
      return this;
    }

    public ISyndicationLink Build()
    {
      return _item;
    }
  }
}