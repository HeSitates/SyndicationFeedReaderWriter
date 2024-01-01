using System.Collections.Generic;
using System.Linq;
using Microsoft.SyndicationFeed.Interfaces;

namespace Microsoft.SyndicationFeed.Atom.Extentions;

internal static class AtomAttributeExtentions
{
  public static string GetAtom(this IEnumerable<ISyndicationAttribute> attributes, string name)
  {
    return attributes.FirstOrDefault(a => IsAtom(a, name))?.Value;
  }

  public static bool IsAtom(this ISyndicationAttribute attr, string name)
  {
    return attr.Name == name && (string.IsNullOrEmpty(attr.Namespace) || attr.Namespace == AtomConstants.Atom10Namespace);
  }
}