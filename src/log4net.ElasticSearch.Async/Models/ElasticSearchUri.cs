namespace log4net.ElasticSearch.Async.Models
{
    using System;
    using System.Collections.Specialized;

    using log4net.ElasticSearch.Async.Infrastructure;

    internal class ElasticSearchUri
    {
        private readonly StringDictionary connectionStringParts;

        public ElasticSearchUri(StringDictionary connectionStringParts)
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
                        this.Routing(),
                        useBulkApi ? this.Bulk() : string.Empty));
            }

            return string.IsNullOrEmpty(this.Port())
                       ? new Uri(
                           string.Format(
                               "{0}://{1}/{2}/logEvent{3}{4}",
                               this.Scheme(),
                               this.Server(),
                               this.Index(),
                               this.Routing(),
                               useBulkApi ? this.Bulk() : string.Empty))
                       : new Uri(
                           string.Format(
                               "{0}://{1}:{2}/{3}/logEvent{4}{5}",
                               this.Scheme(),
                               this.Server(),
                               this.Port(),
                               this.Index(),
                               this.Routing(),
                               useBulkApi ? this.Bulk() : string.Empty));
        }

        public static ElasticSearchUri For(string connectionString)
        {
            return new ElasticSearchUri(connectionString.ConnectionStringParts());
        }

        private string User()
        {
            return this.connectionStringParts[Keys.User];
        }

        private string Password()
        {
            return this.connectionStringParts[Keys.Password];
        }

        private string Scheme()
        {
            return this.connectionStringParts[Keys.Scheme] ?? "http";
        }

        private string Server()
        {
            return this.connectionStringParts[Keys.Server];
        }

        private string Port()
        {
            return this.connectionStringParts[Keys.Port];
        }

        private string Routing()
        {
            var routing = this.connectionStringParts[Keys.Routing];
            if (!string.IsNullOrWhiteSpace(routing))
            {
                return string.Format("?routing={0}", routing);
            }

            return string.Empty;
        }

        private string Bulk()
        {
            return "/_bulk";
        }

        private string Index()
        {
            var index = this.connectionStringParts[Keys.Index];

            return IsRollingIndex(this.connectionStringParts)
                       ? "{0}-{1}".With(index, Clock.Date.ToString("yyyy.MM.dd"))
                       : index;
        }

        private static bool IsRollingIndex(StringDictionary parts)
        {
            return parts.Contains(Keys.Rolling) && parts[Keys.Rolling].ToBool();
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
        }
    }
}
