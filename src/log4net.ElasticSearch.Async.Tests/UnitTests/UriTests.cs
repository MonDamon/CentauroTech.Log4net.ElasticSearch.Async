namespace CentauroTech.Log4net.ElasticSearch.Async.Tests.UnitTests
{
    using System;

    using FluentAssertions;

    using CentauroTech.Log4net.ElasticSearch.Async.Infrastructure;
    using CentauroTech.Log4net.ElasticSearch.Async.Models;

    using Xunit;

    public class UriTests
    {
        const string RollingConnectionString = "Server=localhost;Index=log;Port=9200;rolling=true";
        const string ImplicityNonRollingConnectionString = "Server=localhost;Index=log;Port=9200";
        const string ExplicitlyNonRollingConnectionString = "Server=localhost;Index=log;Port=9200;rolling=false";
        const string RollingPortLessConnectionString = "Server=localhost;Index=log;rolling=true";
        const string ImplicitlyNonRollingPortLessConnectionString = "Server=localhost;Index=log";
        const string ExplicitlyNonRollingPortLessConnectionString = "Server=localhost;Index=log;rolling=false";
        const string BulkConnectionString = "Server=localhost;Index=log;BufferSize=10";
        const string RoutingConnectionString = "Server=localhost;Index=log;BufferSize=10;Routing=foo";
        const string RoutingAndPipelineConnectionString = "Server=localhost;Index=log;BufferSize=10;Routing=foo;Pipeline=auto-timestamp";

        [Fact]
        public void Implicit_non_rolling_connectionstring_is_parsed_into_index_uri_without_date_suffix()
        {
            UriFor(ImplicityNonRollingConnectionString, useBulkApi: false).
                AbsoluteUri.Should().
                Be("http://localhost:9200/log/logEvent");
        }

        [Fact]
        public void Explicit_non_rolling_connectionstring_is_parsed_into_index_uri_without_date_suffix()
        {
            UriFor(ExplicitlyNonRollingConnectionString, useBulkApi: false).
                AbsoluteUri.Should().
                Be("http://localhost:9200/log/logEvent");
        }

        [Fact(Skip = "Haven't figured out why this occasionally fails yet")]
        public void Rolling_connectionstring_is_parsed_into_index_uri_with_date_suffix()
        {
            using (Clock.Freeze(new DateTime(2015, 01, 05)))
            {
                UriFor(RollingConnectionString, useBulkApi: false).
                    AbsoluteUri.Should().
                    Be("http://localhost:9200/log-2015.01.05/logEvent");
            }
        }

        [Fact]
        public void Subsequent_calls_for_rolling_connection_string_over_two_days_creates_different_indexes()
        {
            using (Clock.Freeze(new DateTime(2015, 01, 05)))
            {
                UriFor(RollingConnectionString, useBulkApi: false).
                    AbsoluteUri.Should().
                    Be("http://localhost:9200/log-2015.01.05/logEvent");
            }
            using (Clock.Freeze(new DateTime(2015, 01, 06)))
            {
                UriFor(RollingConnectionString, useBulkApi: false).
                    AbsoluteUri.Should().
                    Be("http://localhost:9200/log-2015.01.06/logEvent");
            }
        }

        [Fact]
        public void Implicit_non_rolling_portless_connectionstring_is_parsed_into_index_uri_without_date_suffix()
        {
            UriFor(ImplicitlyNonRollingPortLessConnectionString, useBulkApi: false).
                AbsoluteUri.Should().
                Be("http://localhost/log/logEvent");
        }

        [Fact]
        public void Connection_string_with_buffersize_over_one_uses_bulk_api_call()
        {
            UriFor(BulkConnectionString, useBulkApi: true).
                AbsoluteUri.Should().
                Be("http://localhost/log/logEvent/_bulk");
        }

        [Fact]
        public void Explicit_non_rolling_portless_connectionstring_is_parsed_into_index_uri_without_date_suffix()
        {
            UriFor(ExplicitlyNonRollingPortLessConnectionString, useBulkApi: false).
                AbsoluteUri.Should().
                Be("http://localhost/log/logEvent");
        }

        [Fact]
        public void Rolling_portless_connectionstring_is_parsed_into_index_uri_wit_date_suffix()
        {
            using (Clock.Freeze(new DateTime(2015,3,31)))
            {
                UriFor(RollingPortLessConnectionString, useBulkApi: false).
                    AbsoluteUri.Should().
                    Be("http://localhost/log-2015.03.31/logEvent");
            }
        }

        [Fact]
        public void Routing_connection_string_is_appended_as_query_string_parameter()
        {
            UriFor(RoutingConnectionString, useBulkApi: true).
                AbsoluteUri.Should().
                Be("http://localhost/log/logEvent/_bulk?routing=foo");
        }

        [Fact]
        public void Routing_and_pipeline_connection_string_is_appended_as_query_string_parameter()
        {
            UriFor(RoutingAndPipelineConnectionString, useBulkApi: true).
                AbsoluteUri.Should().
                BeOneOf("http://localhost/log/logEvent/_bulk?routing=foo&pipeline=auto-timestamp", "http://localhost/log/logEvent/_bulk?pipeline=auto-timestamp&routing=foo");
        }

        static Uri UriFor(string connectionString, bool useBulkApi)
        {
            return ElasticSearchUri.For(connectionString).GetUri(useBulkApi);
        }
    }
}