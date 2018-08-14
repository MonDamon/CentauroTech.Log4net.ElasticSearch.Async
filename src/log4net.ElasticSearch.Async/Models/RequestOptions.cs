namespace log4net.ElasticSearch.Async.Models
{
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Additional request options for ElasticSearch connection.</summary>
    internal class RequestOptions
    {
        /// <summary>The connection string parts.</summary>
        private readonly StringDictionary connectionStringParts;

        /// <summary>Initializes a new instance of the <see cref="RequestOptions"/> class.</summary>
        /// <param name="connectionStringParts">The connection string parts.</param>
        public RequestOptions(StringDictionary connectionStringParts)
        {
            this.connectionStringParts = connectionStringParts;
        }

        /// <summary>Gets a value indicating whether TLS certificate verification should be omitted.</summary>
        public bool SkipCertificateValidation =>
            this.connectionStringParts.Contains(Keys.SkipCertificateValidation)
            && this.connectionStringParts[Keys.SkipCertificateValidation].ToBool();

        /// <summary>Gets a value indicating whether HTTP(S) proxy should be forcefully skipped</summary>
        public bool SkipProxy =>
            this.connectionStringParts.Contains(Keys.SkipProxy)
            && this.connectionStringParts[Keys.SkipProxy].ToBool();

        /// <summary>Gets the default HTTP connection limit.</summary>
        public int? HttpDefaultConnectionLimit =>
            this.connectionStringParts.Contains(Keys.HttpDefaultConnectionLimit)
                ? (int?)int.Parse(this.connectionStringParts[Keys.HttpDefaultConnectionLimit])
                : null;

        /// <summary>Gets the HTTP(s) proxy</summary>
        public string HttpProxy =>
            this.connectionStringParts.Contains(Keys.HttpProxy)
                ? this.connectionStringParts[Keys.HttpProxy]
                : null;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here. Names and purpose of the class is self-explanatory")]
        private static class Keys
        {
            public const string SkipCertificateValidation = "SkipCertificateValidation";
            public const string SkipProxy = "SkipProxy";
            public const string HttpDefaultConnectionLimit = "HttpDefaultConnectionLimit";
            public const string HttpProxy = "HttpProxy";
        }
    }
}
