// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Interfaces;

namespace Microsoft.SyndicationFeed.Atom.Extentions;

internal static class AtomContentExtentions
{
  public static bool IsAtom(this ISyndicationContent content, string name)
  {
    return content.Name == name && content.Namespace is null or AtomConstants.Atom10Namespace;
  }

  public static bool IsAtom(this ISyndicationContent content)
  {
    return content.Namespace is null or AtomConstants.Atom10Namespace;
  }
}
