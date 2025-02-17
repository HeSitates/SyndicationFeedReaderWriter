// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.SyndicationFeed.Interfaces;

namespace Microsoft.SyndicationFeed;

public sealed class SyndicationAttribute : ISyndicationAttribute
{
  public SyndicationAttribute(string name, string value)
    : this(name, null, value)
  {
    // Intentionally left empty.
  }

  public SyndicationAttribute(string name, string ns, string value)
  {
    Name = name ?? throw new ArgumentNullException(nameof(name));
    Value = value ?? throw new ArgumentNullException(nameof(value));
    Namespace = ns;
  }

  public string Name { get; }

  public string Namespace { get; }

  public string Value { get; }
}