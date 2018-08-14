namespace log4net.ElasticSearch.Async.Tests.IntegrationTests
{
    using FluentAssertions;

    using log4net.ElasticSearch.Async.Models;
    using log4net.ElasticSearch.Async.Tests.Infrastructure;
    using log4net.ElasticSearch.Async.Tests.Infrastructure.Builders;

    using Nest;

    using Xunit;
    using Xunit.Sdk;

    [Collection("IndexCollection")]
    public class ElasticSearchTests
    {
        private ElasticClient elasticClient;
        private IntegrationTestFixture testFixture;

        public ElasticSearchTests(IntegrationTestFixture testFixture)
        {
            this.testFixture = testFixture;
            this.elasticClient = testFixture.Client;
        }

        [Fact]
        public void Can_insert_record()
        {
            var indexResponse = this.elasticClient.Index(LogEventBuilder.Default.LogEvent);

            indexResponse.Id.Should().NotBeNull();
        }

        [Fact]
        public void Can_read_indexed_document()
        {
            var logEvent = LogEventBuilder.Default.LogEvent;

            this.elasticClient.Index(logEvent);    

            Retry.Ignoring<XunitException>(() =>
                {
                    var logEntries =
                        this.elasticClient.Search<logEvent>(
                            sd => sd.Query(qd => qd.Term(le => le.className, logEvent.className)));

                    logEntries.Total.Should().Be(1);                    
                });
        }

    }
}