namespace log4net.ElasticSearch.Async.Tests.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using FluentAssertions;

    using log4net.ElasticSearch.Async.Infrastructure;
    using log4net.ElasticSearch.Async.Models;
    using log4net.ElasticSearch.Async.Tests.UnitTests.Stubs;

    using Xunit;

    public class RepositoryTests
    {
        [Fact]
        public void Index_rolls_over_when_date_changes_during_single_call_to_add_multiple_log_entries()
        {
            var logEvents = new[]
                    {
                        new logEvent(), new logEvent(), new logEvent(), new logEvent()
                    };

            using (Clock.Freeze(new DateTime(2015, 01, 01, 23, 59, 57)))
            {
                var httpClientStub = new HttpClientStub(() => Clock.Freeze(Clock.Now.AddSeconds(5)));

                var repository = Repository.Create("Server=localhost;Index=log;Port=9200;rolling=true", new RequestOptions(new CaseInsensitiveStringDictionary<string>()), httpClientStub);

                repository.Add(logEvents);
                repository.Add(logEvents);

                httpClientStub.Items.Count().Should().Be(2);
                httpClientStub.Items.First().Value.Count.Should().Be(1);
                httpClientStub.Items.Second().Value.Count.Should().Be(1);
            }
        }
    }
}