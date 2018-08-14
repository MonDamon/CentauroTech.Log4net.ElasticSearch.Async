namespace log4net.ElasticSearch.Async
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using log4net.ElasticSearch.Async.Infrastructure;
    using log4net.ElasticSearch.Async.Interfaces;
    using log4net.ElasticSearch.Async.Models;

    /// <summary>ElasticSearch data repository.</summary>
    internal class Repository : IRepository
    {
        /// <summary>Additional request options.</summary>
        private readonly RequestOptions options;

        /// <summary>ElasticSearch base URI information.</summary>
        private readonly ElasticSearchUri elasticSearchUri;

        /// <summary>HTTP client used for communication with ElasticSearch.</summary>
        private readonly IHttpClient httpClient;

        /// <summary>Initializes a new instance of the <see cref="Repository"/> class.</summary>
        /// <param name="elasticSearchUri">The elastic search uri.</param>
        /// <param name="options">The options.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public Repository(ElasticSearchUri elasticSearchUri, RequestOptions options, IHttpClient httpClient)
        {
            this.elasticSearchUri = elasticSearchUri;
            this.httpClient = httpClient;
            this.options = options;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here. Name and arguments are self-explanatory")]
        public static IRepository Create(string connectionString, RequestOptions options)
        {
            return Create(connectionString, options, new HttpClient());
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here. Name and arguments are self-explanatory")]
        public static IRepository Create(string connectionString, RequestOptions options, IHttpClient httpClient)
        {
            return new Repository(ElasticSearchUri.For(connectionString), options, httpClient);
        }

        /// <summary>
        /// Post the event(s) to the Elasticsearch API. If the bufferSize in the connection
        /// string is set to more than 1, assume we use the _bulk API for better speed and
        /// efficiency
        /// </summary>
        /// <param name="logEvents">A collection of logEvents</param>
        public void Add(IList<logEvent> logEvents)
        {
            if (logEvents.Count == 1)
            {
                // Post the logEvents one at a time through the ES insert API
                // Generating URI each time is required for rolling indexes
                var insertUri = this.elasticSearchUri.GetUri(useBulkApi: false);
                this.httpClient.Post(insertUri, this.options, logEvents.First());
            }
            else if (logEvents.Count > 1)
            {
                // Post the logEvents all at once using the ES _bulk API
                // Generating URI each time is required for rolling indexes
                var bulkUri = this.elasticSearchUri.GetUri(useBulkApi: true);
                this.httpClient.PostBulk(bulkUri, this.options, logEvents);
            }
        }
    }
}
