// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.SyndicationFeed.Rss;

[SuppressMessage("Security", "S5332: Using http protocol is insecure", Justification = "Just a namespace here.")]
[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Suppressing is ok here.")]
internal static class RssConstants
{
  public const string Rss20Namespace = "";
  public const string SpecificationLink = "http://blogs.law.harvard.edu/tech/rss";
  public const string Version = "2.0";

  public const string IsPermaLink = "isPermaLink";
  public const string Length = "length";
  public const string Type = "type";
  public const string Domain = "domain";
}
