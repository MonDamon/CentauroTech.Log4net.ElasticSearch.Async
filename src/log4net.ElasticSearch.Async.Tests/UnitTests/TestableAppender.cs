namespace log4net.ElasticSearch.Async.Tests.UnitTests
{
    using log4net.ElasticSearch.Async;
    using log4net.ElasticSearch.Async.Interfaces;

    internal class TestableAppender : ElasticSearchAsyncAppender
    {
        public TestableAppender(IRepository repository)
            : base(repository)
        {
        }

        public bool? FailSend { get; set; }

        public bool? FailClose { get; set; }
    }
}