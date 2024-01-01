using System.Collections.Generic;
using System.Linq;
using Microsoft.SyndicationFeed.Interfaces;
using Microsoft.SyndicationFeed.Rss;

namespace Microsoft.SyndicationFeed.Extentions;

internal static class RssAttributeExtentions
{
  public static string GetRss(this IEnumerable<ISyndicationAttribute> attributes, string name)
  {
    return attributes.FirstOrDefault(a => a.Name == name && a.Namespace is RssConstants.Rss20Namespace or null)?.Value;
  }
}