﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.SyndicationFeed.ReaderWriter.Tests.Atom {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AtomFormatterResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AtomFormatterResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.SyndicationFeed.ReaderWriter.Tests.Atom.AtomFormatterResources", typeof(AtomFormatterResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;Name&quot;: &quot;entry&quot;,
        ///  &quot;Namespace&quot;: null,
        ///  &quot;Value&quot;: null,
        ///  &quot;Fields&quot;: [
        ///    {
        ///      &quot;Name&quot;: &quot;id&quot;,
        ///      &quot;Namespace&quot;: null,
        ///      &quot;Value&quot;: &quot;AnId&quot;,
        ///      &quot;Fields&quot;: [],
        ///      &quot;Attributes&quot;: []
        ///    },
        ///    {
        ///      &quot;Name&quot;: &quot;title&quot;,
        ///      &quot;Namespace&quot;: null,
        ///      &quot;Value&quot;: &quot;ATitle&quot;,
        ///      &quot;Fields&quot;: [],
        ///      &quot;Attributes&quot;: []
        ///    },
        ///    {
        ///      &quot;Name&quot;: &quot;updated&quot;,
        ///      &quot;Namespace&quot;: null,
        ///      &quot;Value&quot;: &quot;2024-01-20T00:00:00\u002B01:00&quot;,
        ///      &quot;Fields&quot;: [],
        ///      &quot;Attributes&quot;: []
        ///    },
        ///    { [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateContentWithTitleOnly {
            get {
                return ResourceManager.GetString("CreateContentWithTitleOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://contoso.com/podcast.
        /// </summary>
        internal static string UrlCortosoPodcast {
            get {
                return ResourceManager.GetString("UrlCortosoPodcast", resourceCulture);
            }
        }
    }
}
