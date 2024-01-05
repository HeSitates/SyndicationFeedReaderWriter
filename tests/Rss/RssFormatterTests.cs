using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.SyndicationFeed.ReaderWriter.Tests.Extentions;
using Microsoft.SyndicationFeed.Rss;

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Rss;

public class RssFormatterTests
{
  [TestCaseSource(nameof(CreateContentMissingTitleAndDescriptionData))]
  public void CreateContentSyndicationItemShouldHaveTitleOrDescription(SyndicationItem syndicationItem, string expectedExceptionMessage)
  {
    var sut = new RssFormatter();

    var ex = Assert.Throws<ArgumentNullException>(() => sut.CreateContent(syndicationItem));
    Assert.That(ex, Is.Not.Null);
    Assert.That(ex.Message, Is.EqualTo(expectedExceptionMessage));
  }

  [TestCaseSource(nameof(CreateContentValidData))]
  public void CreateContentSyndicationItemShouldNotThrowAnException(bool useConstructorWithParams, SyndicationItem syndicationItem, string expectedResult)
  {
    RssFormatter sut;
    if (useConstructorWithParams)
    {
      List<SyndicationAttribute> knownAttributes = [new SyndicationAttribute("xmlns:example", TestFeedResources.ExampleNs)];

      var settings = new XmlWriterSettings { Indent = true };

      sut = new RssFormatter(knownAttributes, settings);
    }
    else
    {
      sut = new RssFormatter();
    }

    var result = sut.CreateContent(syndicationItem);
    Assert.That(result, Is.Not.Null);
    Assert.That(result, Is.TypeOf<SyndicationContent>());
    var serialized = result.Serialize(true);
    TestContext.WriteLine(serialized);
    Assert.That(serialized, Is.EqualTo(expectedResult));
  }

  private static IEnumerable CreateContentMissingTitleAndDescriptionData()
  {
    yield return new TestCaseData(null, "Value cannot be null. (Parameter 'item')");
    yield return new TestCaseData(new SyndicationItem(), "RSS Item requires a title or a description (Parameter 'item')");
    yield return new TestCaseData(SyndicationItemBuilder.Create().Build(), "RSS Item requires a title or a description (Parameter 'item')");
  }

  private static IEnumerable CreateContentValidData()
  {
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().WithTitle().Build(), TestFeedResources.CreateContentWithTitleOnly).SetName("With only title");
    yield return new TestCaseData(true, SyndicationItemBuilder.Create().WithTitle().Build(), TestFeedResources.CreateContentWithTitleOnly).SetName("RssFormatter with parameters");
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().WithDescription().Build(), TestFeedResources.CreateContentWithDescriptionOnly).SetName("With only description");
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().WithTitle().WithDescription().Build(), TestFeedResources.CreateContentWithTitleDescription).SetName("With title and description");
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().AddLink().Build(), TestFeedResources.WithContosoLink).SetName("With contoso link");
    var link1 = new SyndicationLink(new Uri(TestFeedResources.GithubUrl));
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().AddLink(link1).Build(), TestFeedResources.WithGithubLink).SetName("With github link");
    var guidlink = new SyndicationLink(new Uri(TestFeedResources.ExampleUrl), RssElementNames.Guid);
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().AddLink(guidlink).Build(), TestFeedResources.WithPermalinkNoId).SetName("With guid link");
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().AddLink(guidlink).WithId().Build(), TestFeedResources.WithPermalinkAndId).SetName("With guid link and id");
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().WithId().Build(), TestFeedResources.WithId).SetName("With id");
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().AddContributor().Build(), TestFeedResources.WithContributor).SetName("With contributor");
    yield return new TestCaseData(false, SyndicationItemBuilder.Create().AddCategory().Build(), TestFeedResources.WithCategory).SetName("With category");
  }

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

    public SyndicationItemBuilder AddLink(SyndicationLink syndicationLink = null)
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      syndicationLink ??= new SyndicationLink(new Uri(TestFeedResources.ContosoUrl))
      {
        Title = "Test title",
        Length = 123,
        MediaType = "mp3/video"
      };

      _item.AddLink(syndicationLink);
      return this;
    }

    public SyndicationItemBuilder WithId(string id = "AnId")
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      _item.Id = id;
      return this;
    }

    public SyndicationItemBuilder AddContributor(SyndicationPerson person = null)
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      person ??= new SyndicationPerson("managingeditor", "managingeditor@contoso.com", RssContributorTypes.ManagingEditor);

      _item.AddContributor(person);
      return this;
    }

    public SyndicationItemBuilder AddCategory(SyndicationCategory category = null)
    {
      if (string.IsNullOrWhiteSpace(_item.Title))
      {
        WithTitle();
      }

      category ??= new SyndicationCategory("Technology");

      _item.AddCategory(category);
      return this;
    }

    public SyndicationItem Build()
    {
      return _item;
    }
  }
}