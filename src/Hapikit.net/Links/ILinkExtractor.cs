﻿using System;
using System.Collections.Generic;

namespace Hapikit.Links
{
    /// <summary>
    /// This interface can be implemented by media type parsers to provide a generic way to access links in a representation
    /// </summary>
    public interface ILinkExtractor
    {
            Type SupportedType { get; }
            ILink GetLink(Func<string, ILink> factory, object content, string relation, string anchor = null);
            IEnumerable<ILink> GetLinks(Func<string,ILink> factory, object content, string relation = null, string anchor = null);
    }
}
