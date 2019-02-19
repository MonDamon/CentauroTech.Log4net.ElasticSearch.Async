namespace CentauroTech.Log4net.ElasticSearch.Async.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using CentauroTech.Log4net.ElasticSearch.Async.Infrastructure;

    internal class ElasticSearchUri
    {
        private readonly CaseInsensitiveStringDictionary<string> connectionStringParts;

        public ElasticSearchUri(CaseInsensitiveStringDictionary<string> connectionStringParts)
        {
            this.connectionStringParts = connectionStringParts;
        }

        public Uri GetUri(bool useBulkApi)
        {
            if (!string.IsNullOrWhiteSpace(this.User()) && !string.IsNullOrWhiteSpace(this.Password()))
            {
                return new Uri(
                    string.Format(
                        "{0}://{1}:{2}@{3}:{4}/{5}/logEvent{6}{7}",
                        this.Scheme(),
                        this.User(),
                        this.Password(),
                        this.Server(),
                        this.Port(),
                        this.Index(),
                        useBulkApi ? this.Bulk() : string.Empty,
                        this.UrlParams()));
            }

            return string.IsNullOrEmpty(this.Port())
                       ? new Uri(
                           string.Format(
                               "{0}://{1}/{2}/logEvent{3}{4}",
                               this.Scheme(),
                               this.Server(),
                               this.Index(),
                               useBulkApi ? this.Bulk() : string.Empty,
                               this.UrlParams()))
                       : new Uri(
                           string.Format(
                               "{0}://{1}:{2}/{3}/logEvent{4}{5}",
                               this.Scheme(),
                               this.Server(),
                               this.Port(),
                               this.Index(),
                               useBulkApi ? this.Bulk() : string.Empty,
                               this.UrlParams()));
        }

        public static ElasticSearchUri For(string connectionString)
        {
            return new ElasticSearchUri(connectionString.ConnectionStringParts());
        }

        private string User()
        {
            return this.connectionStringParts.Get(Keys.User);
        }

        private string Password()
        {
            return this.connectionStringParts.Get(Keys.Password);
        }

        private string Scheme()
        {
            return this.connectionStringParts.Get(Keys.Scheme) ?? "http";
        }

        private string Server()
        {
            return this.connectionStringParts.Get(Keys.Server);
        }

        private string Port()
        {
            return this.connectionStringParts.Get(Keys.Port);
        }
        
        private string Bulk()
        {
            return "/_bulk";
        }

        private string Index()
        {
            var index = this.connectionStringParts.Get(Keys.Index);

            return IsRollingIndex(this.connectionStringParts)
                       ? "{0}-{1}".With(index, Clock.Date.ToString("yyyy.MM.dd"))
                       : index;
        }

        private static bool IsRollingIndex(CaseInsensitiveStringDictionary<string> parts)
        {
            return parts.Contains(Keys.Rolling) && parts.Get(Keys.Rolling).ToBool();
        }

        private string UrlParams()
        {
            var urlParamsDict = new Dictionary<string, string>();

            var routing = this.connectionStringParts.Get(Keys.Routing);
            if (!string.IsNullOrWhiteSpace(routing))
            {
                urlParamsDict["routing"] = routing;
            }

            var pipeline = this.connectionStringParts.Get(Keys.Pipeline);
            if (!string.IsNullOrWhiteSpace(pipeline))
            {
                urlParamsDict["pipeline"] = pipeline;
            }

            var urlParams = string.Join("&", urlParamsDict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return !string.IsNullOrWhiteSpace(urlParams) ? $"?{urlParams}" : string.Empty;
        }

        private static class Keys
        {
            public const string Scheme = "Scheme";
            public const string User = "User";
            public const string Password = "Pwd";
            public const string Server = "Server";
            public const string Port = "Port";
            public const string Index = "Index";
            public const string Rolling = "Rolling";
            public const string Routing = "Routing";
            public const string Pipeline = "Pipeline";
        }
    }
}
