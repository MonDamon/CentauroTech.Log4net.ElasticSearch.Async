namespace log4net.ElasticSearch.Async.Infrastructure
{
    using System;
    using System.Collections.Generic;

    /// <summary>String-keyed dictionary with case insensitive key comparisons</summary>
    /// <typeparam name="T">Type of dictionary values</typeparam>
    internal class CaseInsensitiveStringDictionary<T> : Dictionary<string, T>
    {
        /// <summary>Initializes a new instance of the <see cref="CaseInsensitiveStringDictionary{T}"/> class.</summary>
        public CaseInsensitiveStringDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
