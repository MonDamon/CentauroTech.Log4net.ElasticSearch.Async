namespace log4net.ElasticSearch.Async.Tests.UnitTests.Stubs
{
    using System;
    using System.Collections.Generic;

    using log4net.ElasticSearch.Async.Interfaces;
    using log4net.ElasticSearch.Async.Models;

    internal class HttpClientStub : IHttpClient
    {
        readonly Action action;
        readonly IDictionary<Uri, IList<object>> items;

        public HttpClientStub(Action action)
        {
            this.action = action;

            this.items = new Dictionary<Uri, IList<object>>();
        }

        public void Post(Uri uri, RequestOptions options, logEvent logEntry)
        {
            if (!this.items.ContainsKey(uri))
            {
                this.items[uri] = new List<object>();
            }
            this.items[uri].Add(logEntry);

            this.action();
        }

        public void PostBulk(Uri uri, RequestOptions options, IEnumerable<logEvent> logEntries)
        {
            if (!this.items.ContainsKey(uri))
            {
                this.items[uri] = new List<object>();
            }
            this.items[uri].Add(logEntries);

            this.action();
        }

        public IEnumerable<KeyValuePair<Uri, IList<object>>> Items { get { return this.items; } }
    }
}