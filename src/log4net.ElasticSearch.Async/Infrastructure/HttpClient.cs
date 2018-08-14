namespace log4net.ElasticSearch.Async.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;

    using log4net.ElasticSearch.Async.Interfaces;
    using log4net.ElasticSearch.Async.Models;
    
    internal class HttpClient : IHttpClient
    {
        const string ContentType = "application/json";
        const string Method = "POST";

        public void Post(Uri uri, RequestOptions options, logEvent item)
        {
            var httpWebRequest = RequestFor(uri, options);

            using (var streamWriter = GetRequestStream(httpWebRequest))
            {
                streamWriter.Write(item.ToJson());
                streamWriter.Flush();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                httpResponse.Close();

                if (httpResponse.StatusCode != HttpStatusCode.Created)
                {
                    throw new WebException(
                        "Failed to post {0} to {1}.".With(item.GetType().Name, uri));
                }
            }
        }

        /// <summary>
        /// Post the events to the Elasticsearch _bulk API for faster inserts
        /// </summary>
        /// <param name="uri">Fully formed URI to the ES endpoint</param>
        /// <param name="options">Additional request options</param>
        /// <param name="items">List of logEvents</param>
        public void PostBulk(Uri uri, RequestOptions options, IEnumerable<logEvent> items)
        {
            var httpWebRequest = RequestFor(uri, options);

            var postBody = new StringBuilder();

            // For each logEvent, we build a bulk API request which consists of one line for
            // the action, one line for the document. In this case "index" (idempotent) and then the doc
            // Since we're appending _bulk to the end of the Uri, ES will default to using the
            // index and type already specified in the Uri segments
            foreach (var item in items)
            {
                postBody.AppendLine("{\"index\" : {} }");
                postBody.AppendLine(item.ToJson());
            }

            using (var streamWriter = GetRequestStream(httpWebRequest))
            {
                streamWriter.Write(postBody.ToString());
                streamWriter.Flush();

                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpResponse.StatusCode != HttpStatusCode.Created
                        && httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException("Failed to post {0} to {1}.".With(postBody.ToString(), uri));
                    }
                }
            }
        }

        public static HttpWebRequest RequestFor(Uri uri, RequestOptions options)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

            httpWebRequest.ContentType = ContentType;
            httpWebRequest.Method = Method;

#if NET45
            if (options.SkipCertificateValidation)
            {
                httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }
#endif

            if (options.SkipProxy)
            {
                httpWebRequest.Proxy = new WebProxy();
            }
            else if (options.HttpProxy != null)
            {
                var proxyUri = new Uri(options.HttpProxy);
                httpWebRequest.Proxy = new WebProxy(proxyUri);
            }

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                httpWebRequest.Headers.Remove(HttpRequestHeader.Authorization);
                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(uri.UserInfo)));
            }

            return httpWebRequest;
        }

        private static StreamWriter GetRequestStream(WebRequest httpWebRequest)
        {
            return new StreamWriter(httpWebRequest.GetRequestStream());
        }
    }
}