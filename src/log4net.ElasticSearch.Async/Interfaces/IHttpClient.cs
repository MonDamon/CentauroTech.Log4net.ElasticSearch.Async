namespace CentauroTech.Log4net.ElasticSearch.Async.Interfaces
{
    using System;
    using System.Collections.Generic;

    using CentauroTech.Log4net.ElasticSearch.Async.Models;

    internal interface IHttpClient
    {
        void Post(Uri uri, RequestOptions options, logEvent item);
        void PostBulk(Uri uri, RequestOptions options, IEnumerable<logEvent> items);
    }
}