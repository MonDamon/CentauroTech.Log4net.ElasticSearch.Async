namespace log4net.ElasticSearch.Async.Tests.UnitTests
{
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;

    using FluentAssertions;

    using log4net.ElasticSearch.Async.Tests.Infrastructure;
    using log4net.ElasticSearch.Async.Tests.Infrastructure.Builders;

    using Xunit;
    using Xunit.Sdk;

    public class ElasticSearchAppenderTests
    {
        [Fact]
        public void When_number_of_LogEvents_exceeds_Buffer_by_1_then_Buffer_is_sent_to_ElasticSearch()
        {
            using (var context = UnitTestContext.Create())
            {
                var loggingEvents = LoggingEventsBuilder.OfSize(100 + 1).ToArray();

                context.Appender.DoAppend(loggingEvents);

                Retry.Ignoring<XunitException>(() => context.Repository.LogEntries.TotalCount()
                                                             .Should()
                                                             .Be(loggingEvents.Count(),
                                                                 "buffer should be sent to ElasticSearch"));
            }            
        }
        [Fact]
        public void When_number_of_LogEvents_greatly_exceeds_Buffer_then_remaining_entries_are_sent_to_ElasticSearch_when_Appender_closes()
        {
            using (var context = UnitTestContext.Create())
            {
                var loggingEvents = LoggingEventsBuilder.GreaterThan(100 + 1).ToArray();

                context.Appender.DoAppend(loggingEvents);
                context.Appender.Close();

                Retry.Ignoring<XunitException>(() => context.Repository.LogEntries.TotalCount()
                                                             .Should()
                                                             .Be(loggingEvents.Count(),
                                                                 "all events should be logged by the time the buffer closes"));
            }            

        }

        [Fact]
        public void Appender_logs_on_sepearate_threads()
        {
            using (var context = UnitTestContext.Create())
            {
                var loggingEvents = LoggingEventsBuilder.MultiplesOf(100).ToArray();

                context.Appender.AppendAndClose(loggingEvents);

                Retry.Ignoring<XunitException>(() =>
                {
                    context.Repository.LogEntries.TotalCount()
                           .Should()
                           .Be(loggingEvents.Count(), "all long entries should be sent to ElasticSearch");

                    context.Repository.LogEntriesByThread.Select(pair => pair.Key)
                           .All(i => i != Thread.CurrentThread.ManagedThreadId)
                           .Should()
                           .BeTrue("appender shouldn't log on calling thread");
                });
            }            
        }

        [Fact]
        public void Repository_exceptions_dont_bubble_up()
        {
            using (var context = UnitTestContext.Create(1))
            {
                context.Repository.OnAddThrow<SocketException>();

                Action logErrorWhenElasticSearch =
                    () =>
                    context.Appender.AppendAndClose(LoggingEventsBuilder.MultiplesOf(100).ToArray());

                logErrorWhenElasticSearch.ShouldNotThrow();
            }
        }

        [Fact]
        public void Repository_exceptions_are_handled_by_appender_ErrorHandler()
        {
            using (var context = UnitTestContext.Create())
            {
                var socketException = new SocketException();
                context.Repository.OnAddThrow(socketException);

                context.Appender.AppendAndClose(LoggingEventsBuilder.MultiplesOf(100).ToArray());

                Retry.Ignoring<XunitException>(
                    () =>
                    context.ErrorHandler.Exceptions.Contains(socketException)
                           .Should()
                           .BeTrue("repository errors should be handled by appender ErrorHandler"));
            }
        }
    }
}