namespace log4net.ElasticSearch.Async.Tests.IntegrationTests
{
    using System;

    using Nest;

    using Xunit;

    public class IntegrationTestFixture : IDisposable
    {
        readonly string defaultIndex;

        public IntegrationTestFixture()
        {
            this.defaultIndex = GetDefaultIndex();

            this.Client = new ElasticClient(ConnectionSettings(this.defaultIndex));

            this.DeleteDefaultIndex();
        }

        public ElasticClient Client { get; private set; }

        public void Dispose()
        {
            this.DeleteDefaultIndex();            
        }

        static string GetDefaultIndex()
        {
            return string.Format("{0}", "log_test");
        }

        static ConnectionSettings ConnectionSettings(string index)
        {
            var defaultConnectionSettings = new ConnectionSettings(ElasticSearchUri()).
                DefaultIndex(index).                
                DefaultTypeNameInferrer(t => t.Name).
                DefaultFieldNameInferrer(p => p);

            return !AppSettings.Instance.UseFiddler()
                       ? defaultConnectionSettings
                       : defaultConnectionSettings.
                             DisableAutomaticProxyDetection(false).
                             Proxy(new Uri("http://localhost:8888"), "", "");
        }

        static Uri ElasticSearchUri()
        {
            return new Uri(string.Format("http://{0}:9200", "127.0.0.1".ToString()));
        }

        void DeleteDefaultIndex()
        {
            this.Client.DeleteIndex(new DeleteIndexRequest(this.defaultIndex));
        }
    }

    [CollectionDefinition("IndexCollection")]
    public class DatabaseCollection : ICollectionFixture<IntegrationTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}