namespace CentauroTech.Log4net.ElasticSearch.Async.Tests.UnitTests
{
    using CentauroTech.Log4net.ElasticSearch.Async;
    using CentauroTech.Log4net.ElasticSearch.Async.Interfaces;

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